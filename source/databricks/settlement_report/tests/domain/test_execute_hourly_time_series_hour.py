from pyspark.sql import SparkSession

from fixtures import DBUtilsFixture
from settlement_report_job.domain.report_generator import execute_hourly_time_series
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs


def test_execute_hourly_time_series(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    default_wholesale_fixing_settlement_report_args: SettlementReportArgs,
    metering_point_time_series_written_to_delta_table: None,
):
    # Arrange

    # Act
    execute_hourly_time_series(
        spark, dbutils, default_wholesale_fixing_settlement_report_args
    )

    # Assert
    print(dbutils.jobs.taskValues.get("hourly_time_series_files"))
