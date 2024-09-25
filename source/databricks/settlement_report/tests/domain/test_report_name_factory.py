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
        requesting_actor_id="4123456789012",
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
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
    )


@pytest.mark.parametrize(
    "report_data_type,expected_pre_fix",
    [
        (ReportDataType.TimeSeriesHourly, "TSSD60"),
        (ReportDataType.TimeSeriesQuarterly, "TSSD15"),
    ],
)
def test_create__when_energy_supplier__returns_expected_file_name(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
    report_data_type: ReportDataType,
    expected_pre_fix: str,
):
    # Arrange
    default_settlement_report_args.requesting_actor_market_role = (
        MarketRole.ENERGY_SUPPLIER
    )
    energy_supplier_id = "1234567890123"
    grid_area_code = "123"
    sut = FileNameFactory(report_data_type, default_settlement_report_args)

    # Act
    actual = sut.create(grid_area_code, energy_supplier_id, chunk_index=None)

    # Assert
    assert (
        actual
        == f"{expected_pre_fix}_{grid_area_code}_{energy_supplier_id}_DDQ_01-07-2024_31-07-2024.csv"
    )


def test_create__when_grid_access_provider__returns_expected_file_name(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
):
    # Arrange
    grid_area_code = "123"
    requesting_actor_id = "1111111111111"
    default_settlement_report_args.requesting_actor_market_role = (
        MarketRole.GRID_ACCESS_PROVIDER
    )
    default_settlement_report_args.requesting_actor_id = requesting_actor_id
    sut = FileNameFactory(
        ReportDataType.TimeSeriesHourly, default_settlement_report_args
    )

    # Act
    actual = sut.create(grid_area_code, energy_supplier_id=None, chunk_index=None)

    # Assert
    assert (
        actual
        == f"TSSD60_{grid_area_code}_{requesting_actor_id}_DDM_01-07-2024_31-07-2024.csv"
    )


@pytest.mark.parametrize(
    "market_role, energy_supplier_id, expected_file_name",
    [
        (MarketRole.SYSTEM_OPERATOR, None, "TSSD60_123_DDM_01-07-2024_31-07-2024.csv"),
        (
            MarketRole.DATAHUB_ADMINISTRATOR,
            None,
            "TSSD60_123_DDM_01-07-2024_31-07-2024.csv",
        ),
        (
            MarketRole.SYSTEM_OPERATOR,
            "1987654321123",
            "TSSD60_123_1987654321123_DDQ_01-07-2024_31-07-2024.csv",
        ),
        (
            MarketRole.DATAHUB_ADMINISTRATOR,
            "1987654321123",
            "TSSD60_123_1987654321123_DDQ_01-07-2024_31-07-2024.csv",
        ),
    ],
)
def test_create__when_system_operator_or_datahub_admin__returns_expected_file_name(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
    market_role: MarketRole,
    energy_supplier_id: str,
    expected_file_name: str,
):
    # Arrange
    default_settlement_report_args.requesting_actor_market_role = market_role
    grid_area_code = "123"
    sut = FileNameFactory(
        ReportDataType.TimeSeriesHourly, default_settlement_report_args
    )

    # Act
    actual = sut.create(grid_area_code, energy_supplier_id, chunk_index=None)

    # Assert
    assert actual == expected_file_name


def test_create__when_split_index_is_set__returns_file_name_that_include_split_index(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
):
    # Arrange
    default_settlement_report_args.requesting_actor_market_role = (
        MarketRole.ENERGY_SUPPLIER
    )
    sut = FileNameFactory(
        ReportDataType.TimeSeriesHourly, default_settlement_report_args
    )

    # Act
    actual = sut.create(
        grid_area_code="123", energy_supplier_id="222222222222", chunk_index="17"
    )

    # Assert
    assert actual == "TSSD60_123_222222222222_DDQ_01-07-2024_31-07-2024_17.csv"
