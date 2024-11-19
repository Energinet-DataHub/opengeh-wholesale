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
import os
import re
from dataclasses import dataclass
from pathlib import Path
from typing import Any

from pyspark.sql import DataFrame, Window, functions as F

from telemetry_logging import Logger, use_span
from settlement_report_job.domain.utils.report_data_type import ReportDataType
from settlement_report_job.infrastructure.report_name_factory import FileNameFactory
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.domain.utils.csv_column_names import EphemeralColumns
from settlement_report_job.infrastructure.paths import get_report_output_path


log = Logger(__name__)


@dataclass
class TmpFile:
    src: Path
    dst: Path
    tmp_dst: Path


@use_span()
def write(
    dbutils: Any,
    args: SettlementReportArgs,
    df: DataFrame,
    report_data_type: ReportDataType,
    order_by_columns: list[str],
    rows_per_file: int = 1_000_000,
) -> list[str]:

    report_output_path = get_report_output_path(args)
    spark_output_path = f"{report_output_path}/{_get_folder_name(report_data_type)}"

    partition_columns = []
    if EphemeralColumns.grid_area_code_partitioning in df.columns:
        partition_columns.append(EphemeralColumns.grid_area_code_partitioning)

    if args.prevent_large_text_files:
        partition_columns.append(EphemeralColumns.chunk_index)

    headers = _write_files(
        df=df,
        path=spark_output_path,
        partition_columns=partition_columns,
        order_by=order_by_columns,
        rows_per_file=rows_per_file,
    )

    file_name_factory = FileNameFactory(report_data_type, args)
    new_files = _get_new_files(
        spark_output_path,
        report_output_path,
        file_name_factory,
        partition_columns=partition_columns,
    )
    files_paths = _merge_files(
        dbutils=dbutils,
        new_files=new_files,
        headers=headers,
    )

    file_names = [os.path.basename(file_path) for file_path in files_paths]

    return file_names


def _get_folder_name(report_data_type: ReportDataType) -> str:
    if report_data_type == ReportDataType.TimeSeriesHourly:
        return "time_series_points_hourly"
    elif report_data_type == ReportDataType.TimeSeriesQuarterly:
        return "time_series_points_quarterly"
    elif report_data_type == ReportDataType.MeteringPointPeriods:
        return "metering_point_periods"
    elif report_data_type == ReportDataType.ChargeLinks:
        return "charge_link_periods"
    elif report_data_type == ReportDataType.ChargePricePoints:
        return "charge_price_points"
    elif report_data_type == ReportDataType.EnergyResults:
        return "energy_results"
    elif report_data_type == ReportDataType.MonthlyAmounts:
        return "monthly_amounts"
    elif report_data_type == ReportDataType.WholesaleResults:
        return "wholesale_results"
    else:
        raise ValueError(f"Unsupported report data type: {report_data_type}")


@use_span()
def _write_files(
    df: DataFrame,
    path: str,
    partition_columns: list[str],
    order_by: list[str],
    rows_per_file: int,
) -> list[str]:
    """Write a DataFrame to multiple files.

    Args:
        df (DataFrame): DataFrame to write.
        path (str): Path to write the files.
        rows_per_file (int): Number of rows per file.
        partition_columns: list[str]: Columns to partition by.
        order_by (list[str]): Columns to order by.

    Returns:
        list[str]: Headers for the csv file.
    """
    if EphemeralColumns.chunk_index in partition_columns:
        partition_columns_without_chunk = [
            col for col in partition_columns if col != EphemeralColumns.chunk_index
        ]
        w = Window().partitionBy(partition_columns_without_chunk).orderBy(order_by)
        chunk_index_col = F.ceil((F.row_number().over(w)) / F.lit(rows_per_file))
        df = df.withColumn(EphemeralColumns.chunk_index, chunk_index_col)

    if len(order_by) > 0:
        df = df.orderBy(*order_by)

    csv_writer_options = _get_csv_writer_options()

    if partition_columns:
        df.write.mode("overwrite").options(**csv_writer_options).partitionBy(
            partition_columns
        ).csv(path)
    else:
        df.write.mode("overwrite").options(**csv_writer_options).csv(path)

    return [c for c in df.columns if c not in partition_columns]


@use_span()
def _get_new_files(
    spark_output_path: str,
    report_output_path: str,
    file_name_factory: FileNameFactory,
    partition_columns: list[str],
) -> list[TmpFile]:
    """Get the new files to move to the final location.

    Args:
        partition_columns:
        spark_output_path (str): The path where the files are written.
        report_output_path: The path where the files will be moved.
        file_name_factory (FileNameFactory): Factory class for creating file names for the csv files.

    Returns:
        list[dict[str, Path]]: List of dictionaries with the source and destination
            paths for the new files.
    """
    new_files = []

    file_info_list = _get_file_info_list(
        spark_output_path=spark_output_path, partition_columns=partition_columns
    )

    distinct_chunk_indices = set([chunk_index for _, _, chunk_index in file_info_list])
    include_chunk_index = len(distinct_chunk_indices) > 1

    for f, grid_area, chunk_index in file_info_list:
        file_name = file_name_factory.create(
            grid_area_code=grid_area,
            chunk_index=chunk_index if include_chunk_index else None,
        )
        new_name = Path(report_output_path) / file_name
        tmp_dst = Path("/tmp") / file_name
        new_files.append(TmpFile(f, new_name, tmp_dst))

    return new_files


def _get_file_info_list(
    spark_output_path: str, partition_columns: list[str]
) -> list[tuple[Path, str | None, str | None]]:
    file_info_list = []

    files = [f for f in Path(spark_output_path).rglob("*.csv")]

    partition_by_grid_area = (
        EphemeralColumns.grid_area_code_partitioning in partition_columns
    )
    partition_by_chunk_index = EphemeralColumns.chunk_index in partition_columns

    regex = spark_output_path
    if partition_by_grid_area:
        regex = f"{regex}/{EphemeralColumns.grid_area_code_partitioning}=(\\w{{3}})"

    if partition_by_chunk_index:
        regex = f"{regex}/{EphemeralColumns.chunk_index}=(\\d+)"

    for f in files:
        partition_match = re.match(regex, str(f))
        if partition_match is None:
            raise ValueError(f"File {f} does not match the expected pattern")

        groups = partition_match.groups()
        group_count = 0

        if partition_by_grid_area:
            grid_area = groups[group_count]
            group_count += 1
        else:
            grid_area = None

        if partition_by_chunk_index and len(files) > 1:
            chunk_index = groups[group_count]
            group_count += 1
        else:
            chunk_index = None

        file_info_list.append((f, grid_area, chunk_index))

    return file_info_list


@use_span()
def _merge_files(
    dbutils: Any, new_files: list[TmpFile], headers: list[str]
) -> list[str]:
    """Merges the new files and moves them to the final location.

    Args:
        dbutils (Any): The DBUtils object.
        new_files (list[dict[str, Path]]): List of dictionaries with the source and
            destination paths for the new files.
        headers (list[str]): Headers for the csv file.

    Returns:
        list[str]: List of the final file paths.
    """
    print("Files to merge: " + str(new_files))
    for tmp_dst in set([f.tmp_dst for f in new_files]):
        tmp_dst.parent.mkdir(parents=True, exist_ok=True)
        with tmp_dst.open("w+") as f_tmp_dst:
            print("Creating " + str(tmp_dst))
            f_tmp_dst.write(",".join(headers) + "\n")

    for _file in new_files:
        with _file.src.open("r") as f_src:
            with _file.tmp_dst.open("a") as f_tmp_dst:
                f_tmp_dst.write(f_src.read())

    for tmp_dst, dst in set([(f.tmp_dst, f.dst) for f in new_files]):
        print("Moving " + str(tmp_dst) + " to " + str(dst))
        dbutils.fs.mv("file:" + str(tmp_dst), str(dst))

    return list(set([str(_file.dst) for _file in new_files]))


def _get_csv_writer_options() -> dict[str, str]:
    return {"timestampFormat": "yyyy-MM-dd'T'HH:mm:ss'Z'"}
