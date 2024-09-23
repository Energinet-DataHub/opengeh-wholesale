import uuid
from datetime import datetime

import pytest
from pyspark.sql import SparkSession

from settlement_report_job.domain.calculation_type import CalculationType
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.report_name_factory import (
    FileNameFactory,
    ReportDataType,
)
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs


@pytest.fixture(scope="session")
def default_settlement_report_args() -> SettlementReportArgs:
    return SettlementReportArgs(
        report_id=str(uuid.uuid4()),
        requesters_id="4123456789012",
        period_start=datetime(2024, 6, 30, 22, 0, 0),
        period_end=datetime(2024, 7, 31, 22, 0, 0),
        calculation_type=CalculationType.WHOLESALE_FIXING,
        calculation_id_by_grid_area={
            "016": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373")
        },
        split_report_by_grid_area=True,
        prevent_large_text_files=False,
        time_zone="Europe/Copenhagen",
        catalog_name="catalog_name",
        energy_supplier_id="1234567890123",
        requesters_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
    )


def test_create__when_requesting_actor_is_energy_supplier__(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
):
    # Arrange
    default_settlement_report_args.requesters_market_role = MarketRole.ENERGY_SUPPLIER
    energy_supplier = "1234567890123"
    grid_area_code = "123"
    sut = FileNameFactory(
        ReportDataType.TimeSeriesHourly, default_settlement_report_args
    )

    # Act
    actual = sut.create(grid_area_code, energy_supplier)

    # Assert
    assert actual == f"TSSD60_123_1234567890123_DDQ_01-07-2024_01-08-2024.csv"
