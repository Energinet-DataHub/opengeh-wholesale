import uuid

import pytest
from pyspark.sql import SparkSession

from fixtures import DBUtilsFixture
from settlement_report_job.domain.calculation_type import CalculationType
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.report_generator import execute_quarterly_time_series
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
import test_data.standard_wholesale_fixing_data_generator as test_data_generators


def test_execute_quarterly_time_series__when_default_wholesale_scenario__returns_expected_number_of_files(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_args: SettlementReportArgs,
    standard_wholesale_fixing_data_written_to_delta_table,
):
    # Arrange
    expected_file_count = 2

    # Act
    execute_quarterly_time_series(spark, dbutils, standard_wholesale_fixing_args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get("quarterly_time_series_files")
    assert len(actual_files) == expected_file_count
