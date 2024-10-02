import pytest
from pyspark.sql import SparkSession

from tests.fixtures import DBUtilsFixture
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.report_generator import execute_hourly_time_series
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.infrastructure.column_names import (
    TimeSeriesPointCsvColumnNames,
)


@pytest.mark.parametrize(
    "requesting_actor_market_role",
    [
        MarketRole.DATAHUB_ADMINISTRATOR,
        MarketRole.SYSTEM_OPERATOR,
    ],
)
def test_execute_hourly_time_series__when_datahub_administrator_or_system_operator__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    requesting_actor_market_role: MarketRole,
    wholesale_args_datahub_admin: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = wholesale_args_datahub_admin
    args.requesting_actor_market_role = requesting_actor_market_role
    expected_file_count = 2  # corresponding to the number of grid areas in standard_wholesale_fixing_scenario
    expected_columns = [
        TimeSeriesPointCsvColumnNames.metering_point_id,
        TimeSeriesPointCsvColumnNames.energy_supplier_id,
        TimeSeriesPointCsvColumnNames.metering_point_type,
        TimeSeriesPointCsvColumnNames.start_of_day,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 26)]

    # Act
    execute_hourly_time_series(spark, dbutils, args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get("hourly_time_series_files")
    assert len(actual_files) == expected_file_count
    for file_path in actual_files:
        df = spark.read.csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns


def test_execute_hourly_time_series__when_datahub_administrator_as_energy_supplier__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    wholesale_args_datahub_admin_as_energy_supplier: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    expected_file_count = 2  # corresponding to the number of grid areas in standard_wholesale_fixing_scenario
    expected_columns = [
        TimeSeriesPointCsvColumnNames.metering_point_id,
        TimeSeriesPointCsvColumnNames.metering_point_type,
        TimeSeriesPointCsvColumnNames.start_of_day,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 26)]

    # Act
    execute_hourly_time_series(
        spark, dbutils, wholesale_args_datahub_admin_as_energy_supplier
    )

    # Assert
    actual_files = dbutils.jobs.taskValues.get("hourly_time_series_files")
    assert len(actual_files) == expected_file_count
    for file_path in actual_files:
        df = spark.read.csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns


def test_execute_hourly_time_series__when_energy_supplier__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    wholesale_args_energy_supplier: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    expected_file_count = 2  # corresponding to the number of grid areas in standard_wholesale_fixing_scenario
    expected_columns = [
        TimeSeriesPointCsvColumnNames.metering_point_id,
        TimeSeriesPointCsvColumnNames.metering_point_type,
        TimeSeriesPointCsvColumnNames.start_of_day,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 26)]

    # Act
    execute_hourly_time_series(spark, dbutils, wholesale_args_energy_supplier)

    # Assert
    actual_files = dbutils.jobs.taskValues.get("hourly_time_series_files")
    assert len(actual_files) == expected_file_count
    for file_path in actual_files:
        df = spark.read.csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns


def test_execute_hourly_time_series__when_grid_access_provider__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    wholesale_args_grid_access_provider: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    expected_file_count = 2  # corresponding to the number of grid areas in standard_wholesale_fixing_scenario
    expected_columns = [
        TimeSeriesPointCsvColumnNames.metering_point_id,
        TimeSeriesPointCsvColumnNames.metering_point_type,
        TimeSeriesPointCsvColumnNames.start_of_day,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 26)]

    # Act
    execute_hourly_time_series(spark, dbutils, wholesale_args_grid_access_provider)

    # Assert
    actual_files = dbutils.jobs.taskValues.get("hourly_time_series_files")
    assert len(actual_files) == expected_file_count
    for file_path in actual_files:
        df = spark.read.csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns
