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
from dataclasses import dataclass
import itertools
from pathlib import Path
import re
import zipfile

from typing import Any
from pyspark.sql import DataFrame
from pyspark.sql import Column, SparkSession
from pyspark.sql import functions as F
from pyspark.sql.window import Window
from telemetry_logging import use_span
from settlement_report_job.infrastructure.report_name_factory import FileNameFactory
from settlement_report_job.domain.utils.csv_column_names import (
    EphemeralColumns,
)


@dataclass
class TmpFile:
    src: Path
    dst: Path
    tmp_dst: Path


def map_from_dict(d: dict) -> Column:
    """Converts a dictionary to a Spark map column

    Args:
        d (dict): Dictionary to convert to a Spark map column

    Returns:
        Column: Spark map column
    """
    return F.create_map([F.lit(x) for x in itertools.chain(*d.items())])


@use_span()
def create_zip_file(
    dbutils: Any, report_id: str, save_path: str, files_to_zip: list[str]
) -> None:
    """Creates a zip file from a list of files and saves it to the specified path.

    Notice that we have to create the zip file in /tmp and then move it to the desired
    location. This is done as `direct-append` or `non-sequential` writes are not
    supported in Databricks.

    Args:
        dbutils (Any): The DBUtils object.
        report_id (str): The report ID.
        save_path (str): The path to save the zip file.
        files_to_zip (list[str]): The list of files to zip.

    Raises:
        Exception: If there are no files to zip.
        Exception: If the save path does not end with .zip.
    """
    if len(files_to_zip) == 0:
        raise Exception("No files to zip")
    if not save_path.endswith(".zip"):
        raise Exception("Save path must end with .zip")

    tmp_path = f"/tmp/{report_id}.zip"
    with zipfile.ZipFile(tmp_path, "a", zipfile.ZIP_DEFLATED) as ref:
        for fp in files_to_zip:
            file_name = fp.split("/")[-1]
            ref.write(fp, arcname=file_name)
    dbutils.fs.mv(f"file:{tmp_path}", save_path)


def get_dbutils(spark: SparkSession) -> Any:
    """Get the DBUtils object from the SparkSession.

    Args:
        spark (SparkSession): The SparkSession object.

    Returns:
        DBUtils: The DBUtils object.
    """
    try:
        from pyspark.dbutils import DBUtils  # type: ignore

        dbutils = DBUtils(spark)
    except ImportError:
        raise ImportError(
            "DBUtils is not available in local mode. This is expected when running tests."  # noqa
        )
    return dbutils


def _get_csv_writer_options() -> dict[str, str]:
    return {"timestampFormat": "yyyy-MM-dd'T'HH:mm:ss'Z'"}


@use_span()
def write_files(
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
def get_new_files(
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
    files = [f for f in Path(spark_output_path).rglob("*.csv")]
    new_files = []

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

        file_name = file_name_factory.create(
            grid_area_code=grid_area,
            chunk_index=chunk_index,
        )
        new_name = Path(report_output_path) / file_name
        tmp_dst = Path("/tmp") / file_name
        new_files.append(TmpFile(f, new_name, tmp_dst))

    return new_files


@use_span()
def merge_files(
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