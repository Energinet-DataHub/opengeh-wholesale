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
from uuid import UUID
import zipfile

from typing import Any
from pyspark.sql import DataFrame
from pyspark.sql import Column, SparkSession
from pyspark.sql import functions as F
from pyspark.sql.window import Window
from pyspark.sql.types import DecimalType, DoubleType, FloatType

from settlement_report_job.domain.report_name_factory import FileNameFactory
from settlement_report_job.domain.csv_column_names import (
    EphemeralColumns,
    CsvColumnNames,
)
from settlement_report_job.wholesale.column_names import DataProductColumnNames


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


def _get_csv_writer_options_based_on_locale(locale: str) -> dict[str, str]:
    options_to_always_include = {"timestampFormat": "yyyy-MM-dd'T'HH:mm:ss'Z'"}
    if locale.lower() == "en-gb":
        return options_to_always_include | {"locale": "en-gb", "delimiter": ","}
    if locale.lower() == "da-dk":
        return options_to_always_include | {"locale": "da-dk", "delimiter": ";"}
    else:
        return options_to_always_include | {"locale": "en-us", "delimiter": ","}


def _convert_all_floats_to_danish_csv_format(df: DataFrame) -> DataFrame:
    data_types_to_convert = [
        FloatType().typeName(),
        DecimalType().typeName(),
        DoubleType().typeName(),
    ]
    fields_to_convert = [
        field
        for field in df.schema
        if field.dataType.typeName() in data_types_to_convert
    ]

    for field in fields_to_convert:
        df = df.withColumn(
            field.name, F.translate(F.col(field.name).cast("string"), ".", ",")
        )

    return df


def write_files(
    df: DataFrame,
    path: str,
    partition_columns: list[str],
    order_by: list[str],
    rows_per_file: int,
    locale: str = "en-us",
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
        w = Window().orderBy(order_by)
        chunk_index_col = F.ceil((F.row_number().over(w)) / F.lit(rows_per_file))
        df = df.withColumn(EphemeralColumns.chunk_index, chunk_index_col)

    if len(order_by) > 0:
        df = df.orderBy(*order_by)

    if locale.lower() == "da-dk":
        df = _convert_all_floats_to_danish_csv_format(df)

    csv_writer_options = _get_csv_writer_options_based_on_locale(locale)

    print("writing to path: " + path)
    if partition_columns:
        df.write.mode("overwrite").options(**csv_writer_options).partitionBy(
            partition_columns
        ).csv(path)
    else:
        df.write.mode("overwrite").options(**csv_writer_options).csv(path)

    return [c for c in df.columns if c not in partition_columns]


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

    regex = spark_output_path
    if CsvColumnNames.grid_area_code in partition_columns:
        regex = f"{regex}/{CsvColumnNames.grid_area_code}=(\\w{{3}})"

    if EphemeralColumns.grid_area_code in partition_columns:
        regex = f"{regex}/{EphemeralColumns.grid_area_code}=(\\w{{3}})"

    if CsvColumnNames.energy_supplier_id in partition_columns:
        regex = f"{regex}/{CsvColumnNames.energy_supplier_id}=(\\w+)"

    if EphemeralColumns.chunk_index in partition_columns:
        regex = f"{regex}/{EphemeralColumns.chunk_index}=(\\d+)"

    assert EphemeralColumns.chunk_index in partition_columns

    for f in files:
        partition_match = re.match(regex, str(f))
        if partition_match is None:
            raise ValueError(f"File {f} does not match the expected pattern")

        groups = partition_match.groups()
        group_count = 0

        if (
            CsvColumnNames.grid_area_code in partition_columns
            or EphemeralColumns.grid_area_code in partition_columns
        ):
            grid_area = groups[group_count]
            group_count += 1
        else:
            grid_area = None

        if CsvColumnNames.energy_supplier_id in partition_columns:
            energy_supplier_id = groups[group_count]
            group_count += 1
        else:
            energy_supplier_id = None

        if EphemeralColumns.chunk_index in partition_columns and len(files) > 1:
            chunk_index = groups[group_count]
            group_count += 1
        else:
            chunk_index = None

        file_name = file_name_factory.create(
            grid_area_code=grid_area,
            energy_supplier_id=energy_supplier_id,
            chunk_index=chunk_index,
        )
        new_name = Path(report_output_path) / file_name
        tmp_dst = Path("/tmp") / file_name
        new_files.append(TmpFile(f, new_name, tmp_dst))

    return new_files


def merge_files(
    dbutils: Any, new_files: list[TmpFile], headers: list[str], locale: str
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
    csv_delimiter = _get_csv_writer_options_based_on_locale(locale)["delimiter"]
    for tmp_dst in set([f.tmp_dst for f in new_files]):
        tmp_dst.parent.mkdir(parents=True, exist_ok=True)
        with tmp_dst.open("w+") as f_tmp_dst:
            print("Creating " + str(tmp_dst))
            f_tmp_dst.write(csv_delimiter.join(headers) + "\n")

    for _file in new_files:
        with _file.src.open("r") as f_src:
            with _file.tmp_dst.open("a") as f_tmp_dst:
                f_tmp_dst.write(f_src.read())

    for tmp_dst, dst in set([(f.tmp_dst, f.dst) for f in new_files]):
        print("Moving " + str(tmp_dst) + " to " + str(dst))
        dbutils.fs.mv("file:" + str(tmp_dst), str(dst))

    return list(set([str(_file.dst) for _file in new_files]))


def should_include_ephemeral_grid_area(
    calculation_id_by_grid_area: dict[str, UUID] | None,
    grid_area_codes: list[str] | None,
    split_report_by_grid_area: bool,
) -> bool:
    return (
        _check_if_only_one_grid_area_is_selected(
            calculation_id_by_grid_area, grid_area_codes
        )
        or split_report_by_grid_area
    )


def _check_if_only_one_grid_area_is_selected(
    calculation_id_by_grid_area: dict[str, UUID] | None,
    grid_area_codes: list[str] | None,
) -> bool:
    only_one_grid_area_from_calc_ids = (
        calculation_id_by_grid_area is not None
        and len(calculation_id_by_grid_area) == 1
    )

    only_one_grid_area_from_grid_area_codes = (
        grid_area_codes is not None and len(grid_area_codes) == 1
    )

    return only_one_grid_area_from_calc_ids or only_one_grid_area_from_grid_area_codes
