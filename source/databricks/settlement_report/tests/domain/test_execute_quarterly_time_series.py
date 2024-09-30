import uuid
from datetime import datetime

import pytest
from pyspark.sql import SparkSession

from fixtures import DBUtilsFixture
from settlement_report_job.domain.calculation_type import CalculationType
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.report_generator import execute_quarterly_time_series
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs


@pytest.fixture(scope="session")
def default_wholesale_fixing_settlement_report_args(
    settlement_reports_output_path: str,
) -> SettlementReportArgs:
    return SettlementReportArgs(
        report_id=str(uuid.uuid4()),
        period_start=datetime(2024, 6, 30, 22, 0, 0),
        period_end=datetime(2024, 7, 31, 22, 0, 0),
        calculation_type=CalculationType.WHOLESALE_FIXING,
        calculation_id_by_grid_area={
            "804": uuid.UUID("6aea02f6-6f20-40c5-9a95-f419a1245d7e"),
            "805": uuid.UUID("6aea02f6-6f20-40c5-9a95-f419a1245d7e"),
        },
        split_report_by_grid_area=True,
        prevent_large_text_files=False,
        time_zone="Europe/Copenhagen",
        catalog_name="spark_catalog",
        energy_supplier_id="1234567890123",
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id="1111111111111",
        settlement_reports_output_path=settlement_reports_output_path,
    )


def test_execute_quarterly_time_series__when_default_wholesale_scenario__returns_expected_number_of_files(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    default_wholesale_fixing_settlement_report_args: SettlementReportArgs,
):
    # Arrange
    expected_file_count = 2

    # Act
    execute_quarterly_time_series(
        spark, dbutils, default_wholesale_fixing_settlement_report_args
    )

    # Assert
    actual_files = dbutils.jobs.taskValues.get("quarterly_time_series_files")
    assert len(actual_files) == expected_file_count
