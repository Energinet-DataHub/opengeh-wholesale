import pytest
from pyspark.sql import SparkSession

from tests.data_seeding import standard_wholesale_fixing_scenario_data_generator
from tests.dbutils_fixture import DBUtilsFixture

from domain.assertion import assert_files
from settlement_report_job.domain.report_generator import execute_energy_results
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
)
from settlement_report_job.domain.market_role import MarketRole


@pytest.fixture(scope="function", autouse=True)
def reset_task_values(dbutils: DBUtilsFixture):
    yield
    print("Resetting task values")
    dbutils.jobs.taskValues.reset()


def test_execute_energy_results__when_standard_wholesale_fixing_scenario__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
    )
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = ["1000000000000"]
    expected_columns = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.calculation_type,
        CsvColumnNames.time,
        CsvColumnNames.resolution,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.quantity,
    ]

    expected_file_names = [
        "RESULTENERGY_804_1000000000000_02-01-2024_02-01-2024.csv",
        "RESULTENERGY_805_1000000000000_02-01-2024_02-01-2024.csv",
    ]

    # Act
    execute_energy_results(spark, dbutils, standard_wholesale_fixing_scenario_args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get("energy_result_files")
    assert_files(actual_files, expected_columns, expected_file_names, spark)


def test_execute_energy_results__when_split_report_by_grid_area_is_false__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
    )
    standard_wholesale_fixing_scenario_args.calculation_id_by_grid_area = {
        standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[
            0
        ]: standard_wholesale_fixing_scenario_args.calculation_id_by_grid_area[
            standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[0]
        ]
    }
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = None
    standard_wholesale_fixing_scenario_args.split_report_by_grid_area = True
    expected_columns = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.calculation_type,
        CsvColumnNames.time,
        CsvColumnNames.resolution,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.quantity,
    ]

    expected_file_names = [
        "RESULTENERGY_804_02-01-2024_02-01-2024.csv",
    ]

    # Act
    execute_energy_results(spark, dbutils, standard_wholesale_fixing_scenario_args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get("energy_result_files")
    assert_files(actual_files, expected_columns, expected_file_names, spark)


def test_execute_energy_results__when_standard_wholesale_fixing_scenario_grid_access__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.GRID_ACCESS_PROVIDER
    )
    standard_wholesale_fixing_scenario_args.requesting_actor_id = "1234567890123"

    expected_columns = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.calculation_type,
        CsvColumnNames.start_date_time,
        CsvColumnNames.resolution,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.quantity,
    ]

    expected_file_names = [
        "RESULTENERGY_804_1234567890123_DDM_02-01-2024_02-01-2024.csv",
        "RESULTENERGY_805_1234567890123_DDM_02-01-2024_02-01-2024.csv",
    ]

    # Act
    execute_energy_results(spark, dbutils, standard_wholesale_fixing_scenario_args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get("energy_result_files")
    assert_files(actual_files, expected_columns, expected_file_names, spark)


def test_execute_energy_results__when_standard_wholesale_fixing_scenario_energy_supplier__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.ENERGY_SUPPLIER
    )
    standard_wholesale_fixing_scenario_args.requesting_actor_id = "1000000000000"
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = ["1000000000000"]
    expected_columns = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.calculation_type,
        CsvColumnNames.time,
        CsvColumnNames.resolution,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.quantity,
    ]

    expected_file_names = [
        "RESULTENERGY_804_1000000000000_DDQ_02-01-2024_02-01-2024.csv",
        "RESULTENERGY_805_1000000000000_DDQ_02-01-2024_02-01-2024.csv",
    ]

    # Act
    execute_energy_results(spark, dbutils, standard_wholesale_fixing_scenario_args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get("energy_result_files")
    assert_files(actual_files, expected_columns, expected_file_names, spark)
