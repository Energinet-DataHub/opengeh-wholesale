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

from settlement_report_job.infrastructure.column_names import (
    DataProductColumnNames,
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


def write_files(
    df: DataFrame,
    path: str,
    split_large_files: bool = False,
    split_by_grid_area: bool = True,
    rows_per_file: int = 1_000_000,
    order_by: list[str] = [],
) -> list[str]:
    """Write a DataFrame to multiple files.

    Args:
        df (DataFrame): DataFrame to write.
        path (str): Path to write the files.
        rows_per_file (int): Number of rows per file.

    Returns:
        list[str]: Headers for the csv file.
    """
    grid_col = F.col(DataProductColumnNames.grid_area_code)
    split_col = F.lit(0)

    if split_large_files is True:
        w = Window().orderBy(*order_by)
        split_col = F.floor(F.row_number().over(w) / F.lit(rows_per_file))
    if split_by_grid_area is False:
        grid_col = F.lit("ALL")

    df = df.withColumn(DataProductColumnNames.grid_area_code, grid_col)
    df = df.withColumn("split", split_col)

    df.write.mode("overwrite").partitionBy(
        DataProductColumnNames.grid_area_code, "split"
    ).csv(path)
    return df.columns


def get_new_files(result_path: str, file_name_template: str) -> list[TmpFile]:
    """Get the new files to move to the final location.

    Args:
        result_path (str): The path where the files are written.
        file_name_template (str): The template for the new file names. The template
            should contain two placeholders for the {grid_area} and {split}.
            For example: "TSSD1H-{grid_area}-{split}.csv"

    Returns:
        list[dict[str, Path]]: List of dictionaries with the source and destination
            paths for the new files.
    """
    files = [f for f in Path(result_path).rglob("*.csv")]
    new_files = []
    regex = f"{result_path}/{DataProductColumnNames.grid_area_code}=(\\d+)/split=(\\d+)"
    for f in files:
        partition_match = re.match(regex, str(f))
        if partition_match is None:
            raise ValueError(f"File {f} does not match the expected pattern")
        grid_area, split = partition_match.groups()
        file_name = file_name_template.format(grid_area=grid_area, split=split)
        new_name = Path(result_path) / file_name
        tmp_dst = Path("/tmp") / file_name
        new_files.append(TmpFile(f, new_name, tmp_dst))
    return new_files


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
    for dst in [f.tmp_dst for f in new_files]:
        dst.parent.mkdir(parents=True, exist_ok=True)
        with dst.open("w+") as f:
            f.write(",".join(headers) + "\n")

    for _file in new_files:
        with _file.src.open("r") as src:
            with _file.tmp_dst.open("a") as tmp_dst:
                tmp_dst.write(src.read())

    for _file in new_files:
        print("Moving " + str(_file.tmp_dst) + " to " + str(_file.dst))
        dbutils.fs.mv("file:" + str(_file.tmp_dst), str(_file.dst))

    return [str(_file.dst) for _file in new_files]