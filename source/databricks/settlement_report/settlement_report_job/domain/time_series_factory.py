# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
from typing import Union
from pyspark.sql import DataFrame, Column, functions as F, types as T
from pyspark.sql.session import SparkSession

from settlement_report_job.domain.report_naming_convention import (
    METERING_POINT_TYPES,
)
from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.infrastructure.database_definitions import (
    get_metering_point_time_series_view_name,
)
from settlement_report_job.logger import Logger
from settlement_report_job.infrastructure.column_names import (
    DataProductColumnNames,
    TimeSeriesPointCsvColumnNames,
    EphemeralColumns,
)
from settlement_report_job.utils import (
    map_from_dict,
    get_dbutils,
    write_files,
    get_new_files,
    merge_files,
)
from settlement_report_job.infrastructure import logging_configuration

log = Logger(__name__)


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory.create_time_series"
)
def create_time_series(
    spark: SparkSession,
    args: SettlementReportArgs,
    report_directory: str,
    resolution: DataProductMeteringPointResolution,
) -> list[str]:
    log.info("Creating time series points")
    dbutils = get_dbutils(spark)
    time_series_points = _get_filtered_time_series_points(spark, args, resolution)
    prepared_time_series = _generate_time_series(
        time_series_points,
        _get_desired_quantity_column_count(resolution),
        args.time_zone,
    )

    result_path = f"{report_directory}/time_series_{resolution.value}"
    headers = write_files(
        df=prepared_time_series,
        path=result_path,
        split_large_files=args.prevent_large_text_files,
        split_by_grid_area=args.split_report_by_grid_area,
        order_by=[
            DataProductColumnNames.grid_area_code,
            TimeSeriesPointCsvColumnNames.metering_point_type,
            TimeSeriesPointCsvColumnNames.metering_point_id,
            TimeSeriesPointCsvColumnNames.start_of_day,
        ],
    )
    resolution_name = (
        "TSSD60"
        if resolution.value == DataProductMeteringPointResolution.HOUR
        else "TSSD15"
    )
    new_files = get_new_files(
        result_path,
        file_name_template="_".join(
            [
                resolution_name,
                "{grid_area}",
                args.period_start.strftime("%d-%m-%Y"),
                args.period_end.strftime("%d-%m-%Y"),
                "{split}",
            ]
        ),
    )
    files = merge_files(
        dbutils=dbutils,
        new_files=new_files,
        headers=headers,
    )
    return files


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory.pad_array_col"
)
def pad_array_col(
    value_col: Union[str, Column], size: Union[int, Column], prefix: str = "QUANTITY"
) -> Column:
    if isinstance(size, int):
        size = F.lit(size) - F.size(value_col)
    elif isinstance(size, Column):
        size = F.coalesce(size, F.lit(0))
    else:
        raise ValueError("size must be an integer or a Column")
    values = F.transform(
        value_col,
        lambda x: F.struct(
            (
                F.to_timestamp(x.getField(DataProductColumnNames.observation_time))
                .cast(T.TimestampType())
                .alias(DataProductColumnNames.observation_time)
            ),
            (
                x.getField(DataProductColumnNames.quantity)
                .cast(T.DoubleType())
                .alias(DataProductColumnNames.quantity)
            ),
        ),
    )
    max_date = F.array_max(
        F.transform(
            value_col, lambda x: x.getField(DataProductColumnNames.observation_time)
        )
    ).cast(T.TimestampType())

    padding = F.transform(
        F.array_repeat(F.array_max(value_col), size),
        lambda _, i: F.struct(
            (
                F.date_add(max_date, i + 1)
                .cast(T.TimestampType())
                .alias(DataProductColumnNames.observation_time)
            ),
            (F.lit(None).cast(T.DoubleType()).alias(DataProductColumnNames.quantity)),
        ),
    )

    sorted_pad = F.array_sort(
        F.concat(values, padding),
        lambda x, y: F.when(
            x[DataProductColumnNames.observation_time]
            < y[DataProductColumnNames.observation_time],
            -1,
        )
        .when(
            x[DataProductColumnNames.observation_time]
            > y[DataProductColumnNames.observation_time],
            1,
        )
        .otherwise(0),
    )

    return F.transform(
        sorted_pad,
        lambda x, i: F.struct(
            F.concat(F.lit(prefix), i).alias(EphemeralColumns.uid),
            x.getField(DataProductColumnNames.quantity).alias(
                DataProductColumnNames.quantity
            ),
        ),
    )


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory._read_and_filter_from_view"
)
def _read_and_filter_from_view(
    spark: SparkSession, args: SettlementReportArgs, view_name: str
) -> DataFrame:
    df = spark.read.table(view_name).where(
        (F.col(DataProductColumnNames.observation_time) >= args.period_start)
        & (F.col(DataProductColumnNames.observation_time) < args.period_end)
    )

    calculation_id_by_grid_area_structs = [
        F.struct(F.lit(grid_area_code), F.lit(str(calculation_id)))
        for grid_area_code, calculation_id in args.calculation_id_by_grid_area.items()
    ]

    df_filtered = df.where(
        F.struct(
            F.col(DataProductColumnNames.grid_area_code),
            F.col(DataProductColumnNames.calculation_id),
        ).isin(calculation_id_by_grid_area_structs)
    )

    return df_filtered


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory._generate_time_series"
)
def _generate_time_series(
    filtered_metering_points: DataFrame,
    desired_quantity_column_count: int,
    time_zone: str,
) -> DataFrame:
    _result_columns = [
        F.col(DataProductColumnNames.grid_area_code),
        F.col(DataProductColumnNames.metering_point_id).alias(
            TimeSeriesPointCsvColumnNames.metering_point_id
        ),
        map_from_dict(METERING_POINT_TYPES)[
            F.col(DataProductColumnNames.metering_point_type)
        ].alias(TimeSeriesPointCsvColumnNames.metering_point_type),
        F.col(EphemeralColumns.start_of_day).alias(
            TimeSeriesPointCsvColumnNames.start_of_day
        ),
    ] + [
        f"{TimeSeriesPointCsvColumnNames.energy_prefix}{i+1}"
        for i in range(desired_quantity_column_count)
    ]

    df_with_array_column = (
        filtered_metering_points.withColumn(
            EphemeralColumns.start_of_day,
            F.date_trunc(
                "DAY",
                F.from_utc_timestamp(
                    DataProductColumnNames.observation_time, time_zone
                ),
            ),
        )
        .groupBy(
            F.col(DataProductColumnNames.grid_area_code),
            F.col(DataProductColumnNames.calculation_id),
            F.col(DataProductColumnNames.metering_point_id),
            F.col(DataProductColumnNames.metering_point_type),
            F.col(EphemeralColumns.start_of_day),
        )
        .agg(
            F.array_agg(
                F.struct(
                    F.col(DataProductColumnNames.observation_time),
                    F.col(DataProductColumnNames.quantity),
                )
            ).alias(EphemeralColumns.quantities)
        )
    )

    padded_array_column = (
        df_with_array_column.select(
            F.col(DataProductColumnNames.grid_area_code),
            F.col(DataProductColumnNames.calculation_id),
            F.col(DataProductColumnNames.metering_point_id),
            F.col(DataProductColumnNames.metering_point_type),
            F.to_utc_timestamp(F.col(EphemeralColumns.start_of_day), time_zone).alias(
                EphemeralColumns.start_of_day
            ),
            F.col(EphemeralColumns.quantities),
        )
        .select(
            F.col(DataProductColumnNames.grid_area_code),
            F.col(DataProductColumnNames.metering_point_id),
            F.col(DataProductColumnNames.metering_point_type),
            F.col(EphemeralColumns.start_of_day),
            F.explode(
                pad_array_col(
                    EphemeralColumns.quantities,
                    desired_quantity_column_count,
                    TimeSeriesPointCsvColumnNames.energy_prefix,
                )
            ).alias("exploded"),
        )
        .select("*", "exploded.*")
        .drop("exploded")
    )

    result = (
        padded_array_column.groupBy(
            F.col(DataProductColumnNames.grid_area_code),
            F.col(DataProductColumnNames.metering_point_id),
            F.col(DataProductColumnNames.metering_point_type),
            F.col(EphemeralColumns.start_of_day),
        ).pivot(EphemeralColumns.uid)
        # We can use whatever aggregation here as there is only one value per uid
        .sum(DataProductColumnNames.quantity)
    )

    return result.select(*_result_columns)


def _get_desired_quantity_column_count(
    resolution: DataProductMeteringPointResolution,
) -> int:
    if resolution == DataProductMeteringPointResolution.HOUR:
        return 25
    elif resolution == DataProductMeteringPointResolution.QUARTER:
        return 25 * 4
    else:
        raise ValueError(f"Unknown time series resolution: {resolution.value}")


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory._get_filtered_data"
)
def _get_filtered_time_series_points(
    spark: SparkSession,
    args: SettlementReportArgs,
    resolution: DataProductMeteringPointResolution,
) -> DataFrame:
    log.info("Getting filtered data")
    df = _read_and_filter_from_view(
        spark, args, get_metering_point_time_series_view_name()
    )
    return df.where(F.col(DataProductColumnNames.resolution) == resolution.value)
