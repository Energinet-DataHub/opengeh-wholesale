from pyspark.sql import SparkSession

from fixtures import DBUtilsFixture
from settlement_report_job.domain.report_generator import execute_hourly_time_series
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs


def test_execute_hourly_time_series(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    any_settlement_report_args: SettlementReportArgs,
):
    # Arrange
    # dbutils.jobs.taskValues.set = MagicMock()

    # Act
    execute_hourly_time_series(spark, dbutils, any_settlement_report_args)

    # Assert
