from pyspark.sql import SparkSession

from fixtures import DBUtilsFixture
from settlement_report_job.domain.report_generator import execute_hourly_time_series
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs


def test_execute_hourly_time_series__when_default_wholesale_scenario__returns_expected_number_of_files(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    expected_file_count = 2

    # Act
    execute_hourly_time_series(spark, dbutils, standard_wholesale_fixing_scenario_args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get("hourly_time_series_files")
    assert len(actual_files) == expected_file_count
