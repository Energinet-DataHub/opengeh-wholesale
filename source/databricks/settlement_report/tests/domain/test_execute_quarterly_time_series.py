from pyspark.sql import SparkSession
import pytest

from tests.fixtures import DBUtilsFixture

from data_seeding import standard_wholesale_fixing_scenario_data_generator
from settlement_report_job.domain.report_generator import execute_quarterly_time_series
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
)


@pytest.fixture(scope="function", autouse=True)
def reset_task_values(dbutils: DBUtilsFixture):
    print("Resetting task values before test")
    yield
    print("Resetting task values")
    dbutils.jobs.taskValues.reset()


def test_execute_quarterly_time_series__when_energy_supplier__returns_expected_number_of_files(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    expected_file_count = 2  # corresponding to the number of grid areas in standard_wholesale_fixing_scenario
    energy_supplier_id = (
        standard_wholesale_fixing_scenario_data_generator.ENERGY_SUPPLIER_IDS[0]
    )
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = [energy_supplier_id]
    expected_columns = [
        CsvColumnNames.metering_point_id,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.start_of_day,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 101)]

    # Act
    execute_quarterly_time_series(
        spark, dbutils, standard_wholesale_fixing_scenario_args
    )

    # Assert
    actual_files = dbutils.jobs.taskValues.get(
        key="quarterly_time_series_files", default=[]
    )
    assert len(actual_files) == expected_file_count
    for file_path in actual_files:
        df = spark.read.option("delimiter", ";").csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns
        assert energy_supplier_id in file_path


def test_execute_quarterly_time_series__when_grid_access_provider__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    expected_file_names = [
        f"RESULTENERGY_804_1000000000000_02-01-2024_02-01-2024.csv",
        f"RESULTENERGY_805_1000000000000_02-01-2024_02-01-2024.csv",
    ]
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = None
    expected_columns = [
        CsvColumnNames.metering_point_id,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.start_of_day,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 101)]

    # Act
    execute_quarterly_time_series(
        spark, dbutils, standard_wholesale_fixing_scenario_args
    )

    # Assert
    actual_files = dbutils.jobs.taskValues.get("quarterly_time_series_files")
    assert set(actual_files) == set(expected_file_names)
    for file_path in actual_files:
        df = spark.read.option("delimiter", ";").csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns
        assert energy_supplier_id in file_path


@pytest.mark.parametrize("include_basis_data", [False, True])
def test_execute_quarterly_time_series__when_include_basis_data__returns_valid_csv_file_paths(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
    include_basis_data: bool,
):
    # Arrange
    standard_wholesale_fixing_scenario_args.include_basis_data = include_basis_data

    if include_basis_data:
        expected_file_count = 2
        expected_columns = [
            CsvColumnNames.energy_supplier_id,
            CsvColumnNames.metering_point_id,
            CsvColumnNames.metering_point_type,
            CsvColumnNames.start_of_day,
        ] + [f"ENERGYQUANTITY{i}" for i in range(1, 101)]
    else:
        expected_file_count = 0

    # Act
    execute_quarterly_time_series(
        spark, dbutils, standard_wholesale_fixing_scenario_args
    )

    # Assert

    actual_files = dbutils.jobs.taskValues.get("quarterly_time_series_files")
    if include_basis_data:
        assert len(actual_files) == expected_file_count
        for file_path in actual_files:
            df = spark.read.option("delimiter", ";").csv(file_path, header=True)
            assert df.count() > 0
            assert df.columns == expected_columns
    else:
        assert actual_files is None or len(actual_files) == 0


def test_execute_quarterly_time_series__when_standard_balance_fixing_scenario__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_balance_fixing_scenario_args: SettlementReportArgs,
    standard_balance_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    expected_file_count = 2  # corresponding to the number of grid areas in standard_balance_fixing_scenario
    expected_columns = [
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.start_of_day,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 101)]

    # Act
    execute_quarterly_time_series(spark, dbutils, standard_balance_fixing_scenario_args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get("quarterly_time_series_files")
    assert len(actual_files) == expected_file_count
    for file_path in actual_files:
        df = spark.read.option("delimiter", ";").csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns
