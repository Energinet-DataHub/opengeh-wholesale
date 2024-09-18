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
from pyspark.sql import DataFrame, functions as F, Window
from pyspark.sql.session import SparkSession

from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.utils import (
    map_from_dict,
    get_dbutils,
    write_files,
    get_new_files,
    merge_files,
)
from settlement_report_job.constants import (
    METERING_POINT_TYPE_DICT,
    get_metering_point_time_series_view_name,
)
from settlement_report_job.table_column_names import (
    DataProductColumnNames,
    TimeSeriesPointCsvColumnNames,
    EphemeralColumns,
)
from settlement_report_job.infrastructure import logging_configuration
from settlement_report_job.logger import Logger

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
    filtered_time_series_points: DataFrame,
    desired_number_of_quantity_columns: int,
    time_zone: str,
) -> DataFrame:
    filtered_time_series_points = _add_start_of_day_column(
        filtered_time_series_points, time_zone
    )

    win = Window.partitionBy(
        DataProductColumnNames.grid_area_code,
        DataProductColumnNames.metering_point_id,
        DataProductColumnNames.metering_point_type,
        EphemeralColumns.start_of_day,
    ).orderBy(DataProductColumnNames.observation_time)
    filtered_time_series_points = filtered_time_series_points.withColumn(
        "chronological_order", F.row_number().over(win)
    )

    pivoted_df = (
        filtered_time_series_points.groupBy(
            DataProductColumnNames.grid_area_code,
            DataProductColumnNames.metering_point_id,
            DataProductColumnNames.metering_point_type,
            EphemeralColumns.start_of_day,
        )
        .pivot(
            "chronological_order",
            list(range(1, desired_number_of_quantity_columns + 1)),
        )
        .agg(F.first(DataProductColumnNames.quantity))
    )

    quantity_column_names = [
        F.col(str(i)).alias(f"ENERGYQUANTITY{i}")
        for i in range(1, desired_number_of_quantity_columns + 1)
    ]

    return pivoted_df.select(
        F.col(DataProductColumnNames.grid_area_code),
        F.col(DataProductColumnNames.metering_point_id).alias(
            TimeSeriesPointCsvColumnNames.metering_point_id
        ),
        map_from_dict(METERING_POINT_TYPE_DICT)[
            F.col(DataProductColumnNames.metering_point_type)
        ].alias(TimeSeriesPointCsvColumnNames.metering_point_type),
        F.col(EphemeralColumns.start_of_day).alias(
            TimeSeriesPointCsvColumnNames.start_of_day
        ),
        *quantity_column_names,
    )


def _add_start_of_day_column(
    filtered_time_series_points: DataFrame, time_zone: str
) -> DataFrame:
    filtered_time_series_points = filtered_time_series_points.withColumn(
        EphemeralColumns.start_of_day,
        F.to_utc_timestamp(
            F.date_trunc(
                "DAY",
                F.from_utc_timestamp(
                    DataProductColumnNames.observation_time, time_zone
                ),
            ),
            time_zone,
        ),
    )
    return filtered_time_series_points


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
