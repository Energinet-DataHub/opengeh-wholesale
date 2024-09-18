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
from datetime import datetime, timedelta
from typing import Union, Any
from pyspark.sql import DataFrame, Column, functions as F, types as T
from pyspark.sql.window import Window
from pyspark.sql.session import SparkSession

from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.utils import map_from_dict, get_dbutils
from settlement_report_job.constants import (
    METERING_POINT_TYPE_DICT,
    get_metering_point_time_series_view_name,
    RESOLUTION_NAMES,
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
    spark: SparkSession, args: SettlementReportArgs, query_directory: str
) -> list[str]:
    log.info("Creating time series points")
    dbutils = get_dbutils(spark)
    hourly_data, quarterly_data = _get_filtered_data(spark, args)
    hourly_time_points = _generate_hourly_ts(hourly_data, args.time_zone)
    quarterly_time_points = _generate_quarterly_ts(quarterly_data, args.time_zone)
    files_to_zip = _write_time_series(
        dbutils=dbutils,
        query_directory=query_directory,
        args=args,
        hourly_time_points_by_grid_area=hourly_time_points,
        quarterly_time_points_by_grid_area=quarterly_time_points,
    )
    log.info(
        "Time series points created the following files:\n\t{}".format(
            "\n\t".join(files_to_zip)
        )
    )
    return files_to_zip


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory._write_time_series"
)
def _write_time_series(
    dbutils: Any,
    query_directory: str,
    args: SettlementReportArgs,
    hourly_time_points_by_grid_area: DataFrame,
    quarterly_time_points_by_grid_area: DataFrame,
) -> list[str]:
    files_to_zip = []
    for df, resolution in [
        (hourly_time_points_by_grid_area, "PT1H"),
        (quarterly_time_points_by_grid_area, "PT15M"),
    ]:
        write_success = _write_time_series_points(
            df=df,
            path=query_directory,
            order_by=[
                DataProductColumnNames.grid_area_code,
                TimeSeriesPointCsvColumnNames.start_of_day,
            ],
            split_by_grid_area=args.split_report_by_grid_area,
            split_large_files=args.prevent_large_text_files,
            rows_per_file=1_000_000,
        )
        if write_success is False:
            raise Exception(
                f"Failed to write time series points to storage with resolution: {resolution}"  # noqa
            )
        to_zip = _get_files_to_zip(
            dbutils=dbutils,
            path=query_directory,
            resolution=resolution,
            period_start=args.period_start,
            period_end=args.period_end,
        )
        files_to_zip.extend(to_zip)
    return files_to_zip


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
    desired_number_of_observations: int,
    time_zone: str,
) -> DataFrame:
    _result_columns = [
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
    ] + [
        f"{TimeSeriesPointCsvColumnNames.energy_prefix}{i+1}"
        for i in range(desired_number_of_observations)
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
                    desired_number_of_observations,
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


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory._generate_hourly_time_series"
)
def _generate_hourly_ts(df: DataFrame, time_zone: str) -> DataFrame:
    log.info("Generating hourly time series")
    ts = _generate_time_series(
        df,
        desired_number_of_observations=25,
        time_zone=time_zone,
    )
    log.info("Hourly time series generated")
    return ts


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory._generate_quarterly_time_series"
)
def _generate_quarterly_ts(
    filtered_metering_points: DataFrame, time_zone: str
) -> DataFrame:
    log.info("Generating quarterly time series")
    ts = _generate_time_series(
        filtered_metering_points,
        desired_number_of_observations=25 * 4,
        time_zone=time_zone,
    )
    log.info("Quarterly time series generated")
    return ts


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory._get_filtered_data"
)
def _get_filtered_data(
    spark: SparkSession, args: SettlementReportArgs
) -> tuple[DataFrame, DataFrame]:
    log.info("Getting filtered data")
    df = _read_and_filter_from_view(
        spark, args, get_metering_point_time_series_view_name()
    )
    hourly_data = df.filter(DataProductColumnNames.resolution == "PT1H")
    quarterly_data = df.filter(DataProductColumnNames.resolution == "PT15M")
    log.info("Filtered data retrieved")
    return hourly_data, quarterly_data


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory._write_time_series_points"
)
def _write_time_series_points(
    df: DataFrame,
    path: str,
    split_large_files: bool = False,
    split_by_grid_area: bool = True,
    rows_per_file: int = 1_000_000,
    order_by: list[str] = [],
) -> Union[bool, None]:
    log.info(f"Writing time series points to: {path}")
    grid_col = F.col(DataProductColumnNames.grid_area_code)
    split_col = F.lit(0)

    if split_large_files is True:
        w = Window().orderBy(order_by)
        split_col = F.floor(F.row_number().over(w) / F.lit(rows_per_file))
    if split_by_grid_area is False:
        grid_col = F.lit("ALL")

    df = df.withColumn(DataProductColumnNames.grid_area_code, grid_col)
    df = df.withColumn("split", split_col)

    (
        df.coalesce(1)
        .write.mode("overwrite")
        .partitionBy(DataProductColumnNames.grid_area_code, "split")
        .csv(path, header=True)
    )
    log.info(f"Time series points written to: {path}")
    return True


@logging_configuration.use_span(
    "settlement_report_job.time_series_factory._get_files_to_zip"
)
def _get_files_to_zip(
    dbutils: Any,
    path: str,
    resolution: str,
    period_start: datetime,
    period_end: datetime,
) -> list[str]:
    log.info(f"Getting files to zip for resolution: {resolution}")
    resolution_name = RESOLUTION_NAMES.get(resolution)
    if resolution_name is None:
        raise Exception(f"Unknown resolution: {resolution}")
    start_date_name = (period_start + timedelta(days=1)).strftime("%d-%m-%Y")
    end_date_name = period_end.strftime("%d-%m-%Y")
    RESULT_DIR = "result"
    files_to_zip = []
    for grid_file in dbutils.fs.ls(path):
        # Skip the result directory
        if grid_file.name.startswith(RESULT_DIR):
            continue

        # Skip files that are not grid area files
        if not grid_file.name.startswith(DataProductColumnNames.grid_area_code):  # noqa
            log.info(f"Removing {grid_file.path}")
            dbutils.fs.rm(grid_file.path, recurse=True)
            continue

        grid_area_name = grid_file.name.split("=")[-1].removesuffix("/")
        for split_file in dbutils.fs.ls(grid_file.path):
            # Skip files that are not split files
            if not split_file.name.startswith("split"):
                continue

            csv_files = [
                f for f in dbutils.fs.ls(split_file.path) if f.name.endswith(".csv")
            ]
            if len(csv_files) != 1:
                msg = (
                    f"Only one csv file expected per partition, found {len(csv_files)}"
                )
                raise Exception(msg)

            split_name = split_file.name.split("=")[-1].removesuffix("/")

            src = csv_files[0].path
            file_name = "_".join(
                [
                    resolution_name,
                    grid_area_name,
                    start_date_name,
                    end_date_name,
                    split_name,
                ]
            )
            dst = f"{path}/{RESULT_DIR}/{file_name}.csv"  # noqa
            files_to_zip.append(dst)
            log.info(f"Moving {src} to {dst}")
            dbutils.fs.mv(src, dst)
            log.info(f"Removing {grid_file.path}")
            dbutils.fs.rm(grid_file.path, recurse=True)

    log.info(f"Files to zip for resolution: {resolution} retrieved")
    return files_to_zip
