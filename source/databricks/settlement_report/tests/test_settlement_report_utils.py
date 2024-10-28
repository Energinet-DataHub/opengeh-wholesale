from pathlib import Path
import pytest
from datetime import datetime
from tempfile import TemporaryDirectory
from pyspark.sql import SparkSession, DataFrame, Row, functions as F
from pyspark.sql.types import (
    StructType,
    StructField,
    FloatType,
    DecimalType,
    DoubleType,
)
from decimal import Decimal

from settlement_report_job.utils import (
    create_zip_file,
    get_dbutils,
    map_from_dict,
    write_files,
)


def test_map_from_dict__when_applied_to_new_col__returns_df_with_new_col(
    spark: SparkSession,
):
    # Arrange
    df = spark.createDataFrame([("a", 1), ("b", 2), ("c", 3)], ["key", "value"])

    # Act
    mapper = map_from_dict({"a": "another_a"})
    actual = df.select("*", mapper[F.col("key")].alias("new_key"))

    # Assert
    expected = spark.createDataFrame(
        [("a", 1, "another_a"), ("b", 2, None), ("c", 3, None)],
        ["key", "value", "new_key"],
    )
    assert actual.collect() == expected.collect()


def test_map_from_dict__when_applied_as_overwrite__returns_df_with_overwritten_column(
    spark: SparkSession,
):
    # Arrange
    df = spark.createDataFrame([("a", 1), ("b", 2), ("c", 3)], ["key", "value"])

    # Act
    mapper = map_from_dict({"a": "another_a"})
    actual = df.select(mapper[F.col("key")].alias("key"), "value")

    # Assert
    expected = spark.createDataFrame(
        [
            ("another_a", 1),
            (None, 2),
            (None, 3),
        ],
        ["key", "value"],
    )
    assert actual.collect() == expected.collect()


def test_get_dbutils__when_run_locally__raise_exception(spark: SparkSession):
    # Act
    with pytest.raises(Exception):
        get_dbutils(spark)


def test_create_zip_file__when_dbutils_is_none__raise_exception():
    # Arrange
    dbutils = None
    report_id = "report_id"
    save_path = "save_path.zip"
    files_to_zip = ["file1", "file2"]

    # Act
    with pytest.raises(Exception):
        create_zip_file(dbutils, report_id, save_path, files_to_zip)


def test_create_zip_file__when_save_path_is_not_zip__raise_exception():
    # Arrange
    dbutils = None
    report_id = "report_id"
    save_path = "save_path"
    files_to_zip = ["file1", "file2"]

    # Act
    with pytest.raises(Exception):
        create_zip_file(dbutils, report_id, save_path, files_to_zip)


def test_create_zip_file__when_no_files_to_zip__raise_exception():
    # Arrange
    dbutils = None
    report_id = "report_id"
    save_path = "save_path.zip"
    files_to_zip = ["file1", "file2"]

    # Act
    with pytest.raises(Exception):
        create_zip_file(dbutils, report_id, save_path, files_to_zip)


def test_create_zip_file__when_files_to_zip__create_zip_file(dbutils):
    # Arrange
    tmp_dir = TemporaryDirectory()
    with open(f"{tmp_dir.name}/file1", "w") as f:
        f.write("content1")
    with open(f"{tmp_dir.name}/file2", "w") as f:
        f.write("content2")

    report_id = "report_id"
    save_path = f"{tmp_dir.name}/save_path.zip"
    files_to_zip = [f"{tmp_dir.name}/file1", f"{tmp_dir.name}/file2"]

    # Act
    create_zip_file(dbutils, report_id, save_path, files_to_zip)

    # Assert
    assert Path(save_path).exists()
    tmp_dir.cleanup()


def test_write_files__when_locale_set_to_danish(spark: SparkSession):
    # Arrange
    df = spark.createDataFrame([("a", 1.1), ("b", 2.2), ("c", 3.3)], ["key", "value"])
    tmp_dir = TemporaryDirectory()
    csv_path = f"{tmp_dir.name}/csv_file"

    # Act
    write_files(
        df,
        csv_path,
        partition_columns=[],
        order_by=[],
        locale="da-dk",
        rows_per_file=1000,
    )

    # Assert
    assert Path(csv_path).exists()

    for x in Path(csv_path).iterdir():
        if x.is_file() and x.name[-4:] == ".csv":
            with x.open(mode="r") as f:
                all_lines_written = f.readlines()

                assert all_lines_written[0] == "a,1,1\n"
                assert all_lines_written[1] == "b,2,2\n"
                assert all_lines_written[2] == "c,3,3\n"

    tmp_dir.cleanup()


def test_write_files__when_locale_set_to_english(spark: SparkSession):
    # Arrange
    df = spark.createDataFrame([("a", 1.1), ("b", 2.2), ("c", 3.3)], ["key", "value"])
    tmp_dir = TemporaryDirectory()
    csv_path = f"{tmp_dir.name}/csv_file"

    # Act
    columns = write_files(
        df,
        csv_path,
        partition_columns=[],
        order_by=[],
        locale="en-gb",
        rows_per_file=1000,
    )

    # Assert
    assert Path(csv_path).exists()

    for x in Path(csv_path).iterdir():
        if x.is_file() and x.name[-4:] == ".csv":
            with x.open(mode="r") as f:
                all_lines_written = f.readlines()

                assert all_lines_written[0] == "a,1.1\n"
                assert all_lines_written[1] == "b,2.2\n"
                assert all_lines_written[2] == "c,3.3\n"

    assert columns == ["key", "value"]

    tmp_dir.cleanup()


def test_write_files__when_order_by_specified_on_single_partition(spark: SparkSession):
    # Arrange
    df = spark.createDataFrame([("b", 2.2), ("a", 1.1), ("c", 3.3)], ["key", "value"])
    tmp_dir = TemporaryDirectory()
    csv_path = f"{tmp_dir.name}/csv_file"

    # Act
    columns = write_files(
        df,
        csv_path,
        partition_columns=[],
        order_by=["value"],
        locale="da-dk",
        rows_per_file=1000,
    )

    # Assert
    assert Path(csv_path).exists()

    for x in Path(csv_path).iterdir():
        if x.is_file() and x.name[-4:] == ".csv":
            with x.open(mode="r") as f:
                all_lines_written = f.readlines()

                assert all_lines_written[0] == "a,1,1\n"
                assert all_lines_written[1] == "b,2,2\n"
                assert all_lines_written[2] == "c,3,3\n"

    assert columns == ["key", "value"]

    tmp_dir.cleanup()


def test_write_files__when_order_by_specified_on_multiple_partitions(
    spark: SparkSession,
):
    # Arrange
    df = spark.createDataFrame(
        [("b", 2.2), ("b", 1.1), ("c", 3.3)],
        ["key", "value"],
    )
    tmp_dir = TemporaryDirectory()
    csv_path = f"{tmp_dir.name}/csv_file"

    # Act
    columns = write_files(
        df,
        csv_path,
        partition_columns=["key"],
        order_by=["value"],
        locale="da-dk",
        rows_per_file=1000,
    )

    # Assert
    assert Path(csv_path).exists()

    for x in Path(csv_path).iterdir():
        if x.is_file() and x.name[-4:] == ".csv":
            with x.open(mode="r") as f:
                all_lines_written = f.readlines()

                if len(all_lines_written == 1):
                    assert all_lines_written[0] == "c;3,3\n"
                elif len(all_lines_written == 2):
                    assert all_lines_written[0] == "b;1,1\n"
                    assert all_lines_written[1] == "b;2,2\n"
                else:
                    raise AssertionError("Found unexpected csv file.")

    assert columns == ["value"]

    tmp_dir.cleanup()


@pytest.mark.parametrize("locale", ["da-dk", "en-gb", "en-us"])
def test_write_files__when_df_includes_timestamps__creates_csv_without_milliseconds(
    spark: SparkSession, locale: str
):
    # Arrange
    df = spark.createDataFrame(
        [
            ("a", datetime(2024, 10, 21, 12, 10, 30, 0)),
            ("b", datetime(2024, 10, 21, 12, 10, 30, 30)),
            ("c", datetime(2024, 10, 21, 12, 10, 30, 123)),
        ],
        ["key", "value"],
    )
    tmp_dir = TemporaryDirectory()
    csv_path = f"{tmp_dir.name}/csv_file"

    # Act
    columns = write_files(
        df,
        csv_path,
        partition_columns=[],
        order_by=[],
        locale=locale,
        rows_per_file=1000,
    )

    # Assert
    assert Path(csv_path).exists()

    for x in Path(csv_path).iterdir():
        if x.is_file() and x.name[-4:] == ".csv":
            with x.open(mode="r") as f:
                all_lines_written = f.readlines()

                assert all_lines_written[0] == f"a,2024-10-21T12:10:30Z\n"
                assert all_lines_written[1] == f"b,2024-10-21T12:10:30Z\n"
                assert all_lines_written[2] == f"c,2024-10-21T12:10:30Z\n"

    assert columns == ["key", "value"]

    tmp_dir.cleanup()
