import uuid

import pytest
from pyspark.sql import SparkSession

from fixtures import DBUtilsFixture
from settlement_report_job.domain.calculation_type import CalculationType
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.report_generator import execute_quarterly_time_series
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
import test_data_generators.standard_wholesale_fixing_data_generator as test_data_generators


@pytest.fixture(scope="session")
def standard_wholesale_fixing_args(
    settlement_reports_output_path: str,
) -> SettlementReportArgs:
    return SettlementReportArgs(
        report_id=str(uuid.uuid4()),
        period_start=test_data_generators.FROM_DATE,
        period_end=test_data_generators.TO_DATE,
        calculation_type=CalculationType.WHOLESALE_FIXING,
        calculation_id_by_grid_area={
            test_data_generators.GRID_AREAS[0]: uuid.UUID(
                test_data_generators.CALCULATION_ID
            ),
            test_data_generators.GRID_AREAS[1]: uuid.UUID(
                test_data_generators.CALCULATION_ID
            ),
        },
        split_report_by_grid_area=True,
        prevent_large_text_files=False,
        time_zone="Europe/Copenhagen",
        catalog_name="spark_catalog",
        energy_supplier_id=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id="1111111111111",
        settlement_reports_output_path=settlement_reports_output_path,
    )


def test_execute_quarterly_time_series__when_default_wholesale_scenario__returns_expected_number_of_files(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    input_database_location: str,
    standard_wholesale_fixing_args: SettlementReportArgs,
):
    # Arrange
    expected_file_count = 2
    test_data_generators.write_metering_point_time_series(
        spark, input_database_location
    )

    # Act
    execute_quarterly_time_series(spark, dbutils, standard_wholesale_fixing_args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get("quarterly_time_series_files")
    assert len(actual_files) == expected_file_count
