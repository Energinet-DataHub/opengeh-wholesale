from pathlib import Path
import pytest
from tempfile import TemporaryDirectory
from pyspark.sql import SparkSession, functions as F

from settlement_report_job.utils import (
    create_zip_file,
    get_dbutils,
    map_from_dict,
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
