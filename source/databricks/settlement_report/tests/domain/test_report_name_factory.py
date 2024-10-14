import uuid
from datetime import datetime

import pytest
from pyspark.sql import SparkSession

from settlement_report_job.infrastructure.calculation_type import CalculationType
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.report_name_factory import (
    FileNameFactory,
    ReportDataType,
)
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs


@pytest.fixture(scope="session")
def default_settlement_report_args() -> SettlementReportArgs:
    """
    Note: Some tests depend on the values of `period_start` and `period_end`
    """
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
        energy_supplier_ids=["1234567890123"],
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        settlement_reports_output_path="some_output_volume_path",
        include_basis_data=True,
        locale="da-dk",
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


@pytest.mark.parametrize(
    "period_start,period_end,expected_start_date,expected_end_date",
    [
        (
            datetime(2024, 2, 29, 23, 0, 0),
            datetime(2024, 3, 31, 22, 0, 0),
            "01-03-2024",
            "31-03-2024",
        ),
        (
            datetime(2024, 9, 30, 22, 0, 0),
            datetime(2024, 10, 31, 23, 0, 0),
            "01-10-2024",
            "31-10-2024",
        ),
    ],
)
def test_create__when_daylight_saving_time__returns_expected_dates_in_file_name(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
    period_start: datetime,
    period_end: datetime,
    expected_start_date: str,
    expected_end_date: str,
):
    # Arrange
    default_settlement_report_args.period_start = period_start
    default_settlement_report_args.period_end = period_end
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
    assert (
        actual
        == f"TSSD60_123_222222222222_DDQ_{expected_start_date}_{expected_end_date}_17.csv"
    )


def test_create__when_energy_supplier_requests_energy_report_not_combined__returns_correct_file_name(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
):
    # Arrange
    default_settlement_report_args.split_report_by_grid_area = True
    default_settlement_report_args.requesting_actor_market_role = (
        MarketRole.ENERGY_SUPPLIER
    )

    factory = FileNameFactory(
        ReportDataType.EnergyResults, default_settlement_report_args
    )

    # Act
    actual = factory.create(
        grid_area_code="123", energy_supplier_id=None, chunk_index=None
    )

    # Assert
    assert (
        actual
        == f"RESULTENERGY_123_{default_settlement_report_args.requesting_actor_id}_DDQ_01-10-2024_31-10-2024.csv"
    )


def test_create__when_energy_supplier_requests_energy_report_combined__returns_correct_file_name(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
):
    # Arrange
    default_settlement_report_args.calculation_id_by_grid_area = {
        "123": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
        "456": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
    }

    default_settlement_report_args.split_report_by_grid_area = False
    default_settlement_report_args.requesting_actor_market_role = (
        MarketRole.ENERGY_SUPPLIER
    )

    factory = FileNameFactory(
        ReportDataType.EnergyResults, default_settlement_report_args
    )

    # Act
    actual = factory.create(
        grid_area_code=None, energy_supplier_id=None, chunk_index=None
    )

    # Assert
    assert (
        actual
        == f"RESULTENERGY_flere-net_{default_settlement_report_args.requesting_actor_id}_DDQ_01-10-2024_31-10-2024.csv"
    )


def test_create__when_grid_access_provider_requests_energy_report__returns_correct_file_name(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
):
    # Arrange
    default_settlement_report_args.requesting_actor_market_role = (
        MarketRole.GRID_ACCESS_PROVIDER
    )
    default_settlement_report_args.calculation_id_by_grid_area = {
        "456": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
    }

    factory = FileNameFactory(
        ReportDataType.EnergyResults, default_settlement_report_args
    )

    # Act
    actual = factory.create(
        grid_area_code="456", energy_supplier_id=None, chunk_index=None
    )

    # Assert
    assert (
        actual
        == f"RESULTENERGY_456_{default_settlement_report_args.requesting_actor_id}_DDM_01-10-2024_31-10-2024.csv"
    )


def test_create__when_datahub_administrator_requests_energy_report_single_grid__returns_correct_file_name(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
):
    # Arrange
    default_settlement_report_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
    )
    default_settlement_report_args.energy_supplier_ids = None
    default_settlement_report_args.calculation_id_by_grid_area = {
        "456": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
    }

    factory = FileNameFactory(
        ReportDataType.EnergyResults, default_settlement_report_args
    )

    # Act
    actual = factory.create(
        grid_area_code="456", energy_supplier_id=None, chunk_index=None
    )

    # Assert
    assert actual == "RESULTENERGY_456_01-10-2024_31-10-2024.csv"


def test_create__when_datahub_administrator_requests_energy_report_multi_grid_not_combined__returns_correct_file_name(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
):
    # Arrange
    default_settlement_report_args.calculation_id_by_grid_area = {
        "123": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
        "456": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
    }
    default_settlement_report_args.split_report_by_grid_area = True
    default_settlement_report_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
    )
    default_settlement_report_args.energy_supplier_ids = None

    factory = FileNameFactory(
        ReportDataType.EnergyResults, default_settlement_report_args
    )

    # Act
    actual = factory.create(
        grid_area_code="456", energy_supplier_id=None, chunk_index=None
    )

    # Assert
    assert actual == "RESULTENERGY_456_01-10-2024_31-10-2024.csv"


def test_create__when_datahub_administrator_requests_energy_report_multi_grid_single_provider_combined__returns_correct_file_name(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
):
    # Arrange
    default_settlement_report_args.calculation_id_by_grid_area = {
        "123": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
        "456": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
    }
    default_settlement_report_args.split_report_by_grid_area = False
    default_settlement_report_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
    )
    default_settlement_report_args.energy_supplier_ids = ["1234567890123"]

    factory = FileNameFactory(
        ReportDataType.EnergyResults, default_settlement_report_args
    )

    # Act
    actual = factory.create(
        grid_area_code=None, energy_supplier_id=None, chunk_index=None
    )

    # Assert
    assert actual == "RESULTENERGY_flere-net_1234567890123_01-10-2024_31-10-2024.csv"


def test_create__when_datahub_administrator_requests_energy_report_multi_grid_all_providers_combined__returns_correct_file_name(
    spark: SparkSession,
    default_settlement_report_args: SettlementReportArgs,
):
    # Arrange
    default_settlement_report_args.calculation_id_by_grid_area = {
        "123": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
        "456": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373"),
    }
    default_settlement_report_args.split_report_by_grid_area = False
    default_settlement_report_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
    )
    default_settlement_report_args.energy_supplier_ids = None

    factory = FileNameFactory(
        ReportDataType.EnergyResults, default_settlement_report_args
    )

    # Act
    actual = factory.create(
        grid_area_code=None, energy_supplier_id=None, chunk_index=None
    )

    # Assert
    assert actual == "RESULTENERGY_flere-net_01-10-2024_31-10-2024.csv"
