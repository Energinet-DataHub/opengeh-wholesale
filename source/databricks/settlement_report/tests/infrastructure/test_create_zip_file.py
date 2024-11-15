from pathlib import Path
import pytest
from datetime import datetime
from tempfile import TemporaryDirectory
from pyspark.sql import SparkSession, functions as F
from pyspark.sql.types import StructType, StructField, StringType

import settlement_report_job.domain.utils.report_naming_convention as market_naming
from settlement_report_job.infrastructure.create_zip_file import create_zip_file
from settlement_report_job.infrastructure.wholesale.data_values import (
    ChargeTypeDataProductValue,
)
from settlement_report_job.infrastructure.wholesale.data_values.calculation_type import (
    CalculationTypeDataProductValue,
)
from settlement_report_job.infrastructure.wholesale.data_values.metering_point_type import (
    MeteringPointTypeDataProductValue,
)
from settlement_report_job.infrastructure.wholesale.data_values.settlement_method import (
    SettlementMethodDataProductValue,
)


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
