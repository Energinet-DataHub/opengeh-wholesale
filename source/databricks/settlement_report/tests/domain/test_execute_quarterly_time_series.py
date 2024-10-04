from pyspark.sql import SparkSession
import pytest

from tests.fixtures import DBUtilsFixture
from settlement_report_job.domain.report_generator import execute_quarterly_time_series
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.infrastructure.column_names import (
    TimeSeriesPointCsvColumnNames,
)


def reset_task_values_quarterly(dbutils: DBUtilsFixture):
    try:
        dbutils.jobs.taskValues.set("quarterly_time_series_files", [])
    except Exception:
        pass


def test_execute_quarterly_time_series__when_standard_wholesale_fixing_scenario__returns_expected_number_of_files(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    try:
        # Arrange
        expected_file_count = 2  # corresponding to the number of grid areas in standard_wholesale_fixing_scenario
        expected_columns = [
            TimeSeriesPointCsvColumnNames.metering_point_id,
            TimeSeriesPointCsvColumnNames.metering_point_type,
            TimeSeriesPointCsvColumnNames.start_of_day,
        ] + [f"ENERGYQUANTITY{i}" for i in range(1, 101)]

        # Act
        execute_quarterly_time_series(
            spark, dbutils, standard_wholesale_fixing_scenario_args
        )

        # Assert
        actual_files = dbutils.jobs.taskValues.get("quarterly_time_series_files")
        assert len(actual_files) == expected_file_count
        for file_path in actual_files:
            df = spark.read.csv(file_path, header=True)
            assert df.count() > 0
            assert df.columns == expected_columns
    finally:
        reset_task_values_quarterly(dbutils)


@pytest.mark.parametrize("include_basis_data", [False, True])
def test_execute_quarterly_time_series__when_include_basis_data__returns_valid_csv_file_paths(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
    include_basis_data: bool,
):
    try:
        # Arrange
        standard_wholesale_fixing_scenario_args.include_basis_data = include_basis_data

        if include_basis_data:
            expected_file_count = 2
            expected_columns = [
                TimeSeriesPointCsvColumnNames.metering_point_id,
                TimeSeriesPointCsvColumnNames.metering_point_type,
                TimeSeriesPointCsvColumnNames.start_of_day,
            ] + [f"ENERGYQUANTITY{i}" for i in range(1, 101)]
        else:
            expected_file_count = 0

        # Act
        execute_quarterly_time_series(
            spark, dbutils, standard_wholesale_fixing_scenario_args
        )

        # Assert
        actual_files = dbutils.jobs.taskValues.get("quarterly_time_series_files")
        assert len(actual_files) == expected_file_count
        for file_path in actual_files:
            df = spark.read.csv(file_path, header=True)
            assert df.count() > 0
            assert df.columns == expected_columns
    finally:
        reset_task_values_quarterly(dbutils)
