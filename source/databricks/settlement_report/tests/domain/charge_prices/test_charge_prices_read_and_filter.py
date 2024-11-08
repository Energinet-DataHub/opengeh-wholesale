import uuid
from datetime import datetime
from unittest.mock import Mock
from uuid import UUID

import pytest
from pyspark.sql import SparkSession, DataFrame
import test_factories.default_test_data_spec as default_data
import test_factories.charge_link_periods_factory as charge_links_factory
import test_factories.metering_point_periods_factory as metering_point_periods_factory
import test_factories.charge_prices_factory as charge_prices_factory
import test_factories.charge_price_information_periods_factory as charge_price_information_periods_factory
from settlement_report_job.domain.charge_prices.read_and_filter import read_and_filter
from settlement_report_job.domain.market_role import MarketRole


DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
ENERGY_SUPPLIER_IDS = ["1234567890123", "2345678901234"]
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
SYSTEM_OPERATOR_ID = "3333333333333"
GRID_ACCESS_PROVIDER_ID = "4444444444444"
OTHER_ID = "9999999999999"
DEFAULT_CALCULATION_ID_BY_GRID_AREA = {
    default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(default_data.DEFAULT_CALCULATION_ID)
}

JAN_1ST = datetime(2023, 12, 31, 23)
JAN_2ND = datetime(2024, 1, 1, 23)
JAN_3RD = datetime(2024, 1, 2, 23)
JAN_4TH = datetime(2024, 1, 3, 23)
JAN_5TH = datetime(2024, 1, 4, 23)
JAN_6TH = datetime(2024, 1, 5, 23)
JAN_7TH = datetime(2024, 1, 6, 23)
JAN_8TH = datetime(2024, 1, 7, 23)
JAN_9TH = datetime(2024, 1, 8, 23)


def _get_repository_mock(
    metering_point_period: DataFrame,
    charge_link_periods: DataFrame,
    charge_prices: DataFrame,
    charge_price_information_periods: DataFrame | None = None,
) -> Mock:
    mock_repository = Mock()
    mock_repository.read_metering_point_periods.return_value = metering_point_period
    mock_repository.read_charge_link_periods.return_value = charge_link_periods
    mock_repository.read_charge_prices.return_value = charge_prices
    if charge_price_information_periods:
        mock_repository.read_charge_price_information_periods.return_value = (
            charge_price_information_periods
        )

    return mock_repository


def test_default_case(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_periods = metering_point_periods_factory.create(
        spark,
        default_data.create_metering_point_periods_row(),
    )

    charge_link_periods = charge_links_factory.create(
        spark,
        default_data.create_charge_link_periods_row(),
    )

    charge_prices = charge_prices_factory.create(
        spark,
        default_data.create_charge_prices_row(),
    )

    charge_price_information_periods = charge_price_information_periods_factory.create(
        spark,
        default_data.create_charge_price_information_periods_row(),
    )

    mock_repository = _get_repository_mock(
        metering_point_periods,
        charge_link_periods,
        charge_prices,
        charge_price_information_periods,
    )

    # Act
    actual_df = read_and_filter(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area=DEFAULT_CALCULATION_ID_BY_GRID_AREA,
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        repository=mock_repository,
    )

    # Assert
    assert actual_df.count() == 1


@pytest.mark.parametrize(
    "args_energy_supplier_ids, charge_time, expected_rows",
    [
        pytest.param(
            ["1"],
            JAN_2ND,
            1,
            id="When energy_supplier_ids is set to ['1'] and charge_time is JAN_2ND, then 1 row is returned",
        ),
        pytest.param(
            ["2"],
            JAN_2ND,
            0,
            id="When energy_supplier_ids is set to ['2'] and charge_time is JAN_2ND, then 0 row is returned",
        ),
        pytest.param(
            ["1", "2"],
            JAN_2ND,
            1,
            id="When energy_supplier_ids is set to ['1', '2'] and charge_time is JAN_2ND, then 1 row is returned",
        ),
        pytest.param(
            ["1", "2"],
            JAN_3RD,
            1,
            id="When energy_supplier_ids is set to ['1', '2'] and charge_time is JAN_3RD, then 1 row is returned",
        ),
        pytest.param(
            ["1", "2"],
            JAN_5TH,
            0,
            id="When energy_supplier_ids is set to ['1', '2'] and charge_time is JAN_5TH, then 0 row is returned",
        ),
    ],
)
def test_changing_energy_supplier_ids_and_charge_time(
    spark: SparkSession,
    args_energy_supplier_ids: list[str] | None,
    charge_time: datetime,
    expected_rows: int,
) -> None:
    # Arrange
    metering_point_periods = metering_point_periods_factory.create(
        spark,
        [
            default_data.create_metering_point_periods_row(
                metering_point_id="1",
                energy_supplier_id="1",
                from_date=JAN_1ST,
                to_date=JAN_4TH,
            ),
            default_data.create_metering_point_periods_row(
                metering_point_id="2",
                energy_supplier_id="2",
                from_date=JAN_3RD,
                to_date=JAN_4TH,
            ),
        ],
    )

    charge_link_periods = charge_links_factory.create(
        spark,
        [
            default_data.create_charge_link_periods_row(
                metering_point_id="1", from_date=JAN_1ST, to_date=JAN_4TH
            ),
            default_data.create_charge_link_periods_row(
                metering_point_id="2", from_date=JAN_3RD, to_date=JAN_4TH
            ),
        ],
    )

    charge_prices = charge_prices_factory.create(
        spark,
        default_data.create_charge_prices_row(charge_time=charge_time),
    )

    charge_price_information_periods = charge_price_information_periods_factory.create(
        spark,
        default_data.create_charge_price_information_periods_row(),
    )

    mock_repository = _get_repository_mock(
        metering_point_periods,
        charge_link_periods,
        charge_prices,
        charge_price_information_periods,
    )

    # Act
    actual_df = read_and_filter(
        period_start=JAN_1ST,
        period_end=JAN_4TH,
        calculation_id_by_grid_area=DEFAULT_CALCULATION_ID_BY_GRID_AREA,
        energy_supplier_ids=args_energy_supplier_ids,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        repository=mock_repository,
    )

    # Assert
    assert actual_df.count() == expected_rows


@pytest.mark.parametrize(
    "args_start_date, args_end_date, expected_rows",
    [
        pytest.param(
            JAN_2ND,
            JAN_9TH,
            1,
            id="when time is within the range, return 1 row",
        ),
        pytest.param(
            JAN_5TH,
            JAN_9TH,
            0,
            id="when time is outside the range, return 0 rows",
        ),
    ],
)
def test_time_within_and_outside_of_date_range_scenarios(
    spark: SparkSession,
    args_start_date: datetime,
    args_end_date: datetime,
    expected_rows: int,
) -> None:
    # Arrange
    charge_time = JAN_3RD

    metering_point_periods = metering_point_periods_factory.create(
        spark,
        default_data.create_metering_point_periods_row(
            from_date=JAN_1ST,
            to_date=JAN_4TH,
        ),
    )

    charge_link_periods = charge_links_factory.create(
        spark,
        default_data.create_charge_link_periods_row(from_date=JAN_1ST, to_date=JAN_4TH),
    )

    charge_prices = charge_prices_factory.create(
        spark,
        default_data.create_charge_prices_row(charge_time=charge_time),
    )

    charge_price_information_periods = charge_price_information_periods_factory.create(
        spark,
        default_data.create_charge_price_information_periods_row(),
    )

    mock_repository = _get_repository_mock(
        metering_point_periods,
        charge_link_periods,
        charge_prices,
        charge_price_information_periods,
    )

    # Act
    actual_df = read_and_filter(
        period_start=args_start_date,
        period_end=args_end_date,
        calculation_id_by_grid_area=DEFAULT_CALCULATION_ID_BY_GRID_AREA,
        energy_supplier_ids=ENERGY_SUPPLIER_IDS,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        repository=mock_repository,
    )

    # Assert
    assert actual_df.count() == expected_rows


@pytest.mark.parametrize(
    "args_energy_supplier_ids, expected_rows",
    [
        pytest.param(
            ["1234567890123"],
            1,
            id="when energy_supplier_id is in energy_supplier_ids, return 1 row",
        ),
        pytest.param(
            ["2345678901234"],
            0,
            id="when energy_supplier_id is not in energy_supplier_ids, return 0 rows",
        ),
        pytest.param(
            None,
            1,
            id="when energy_supplier_ids is None, return 1 row",
        ),
    ],
)
def test_energy_supplier_ids_scenarios(
    spark: SparkSession,
    args_energy_supplier_ids: list[str] | None,
    expected_rows: int,
) -> None:
    # Arrange
    energy_supplier_id = "1234567890123"

    metering_point_periods = metering_point_periods_factory.create(
        spark,
        default_data.create_metering_point_periods_row(
            energy_supplier_id=energy_supplier_id
        ),
    )

    charge_link_periods = charge_links_factory.create(
        spark, default_data.create_charge_link_periods_row()
    )

    charge_prices = charge_prices_factory.create(
        spark,
        default_data.create_charge_prices_row(),
    )

    charge_price_information_periods = charge_price_information_periods_factory.create(
        spark,
        default_data.create_charge_price_information_periods_row(),
    )

    mock_repository = _get_repository_mock(
        metering_point_periods,
        charge_link_periods,
        charge_prices,
        charge_price_information_periods,
    )

    # Act
    actual_df = read_and_filter(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area=DEFAULT_CALCULATION_ID_BY_GRID_AREA,
        energy_supplier_ids=args_energy_supplier_ids,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        repository=mock_repository,
    )

    # Assert
    assert actual_df.count() == expected_rows


@pytest.mark.parametrize(
    "args_calculation_id_by_grid_area, expected_rows",
    [
        pytest.param(
            {"804": UUID(default_data.DEFAULT_CALCULATION_ID)},
            1,
            id="when calculation_id and grid_area_code is in calculation_id_by_grid_area, return 1 row",
        ),
        pytest.param(
            {"500": UUID(default_data.DEFAULT_CALCULATION_ID)},
            0,
            id="when grid_area_code is not in calculation_id_by_grid_area, return 0 rows",
        ),
        pytest.param(
            {"804": UUID("11111111-1111-2222-1111-111111111111")},
            0,
            id="when calculation_id is not in calculation_id_by_grid_area, return 0 row",
        ),
        pytest.param(
            {"500": UUID("11111111-1111-2222-1111-111111111111")},
            0,
            id="when calculation_id and grid_area_code is not in calculation_id_by_grid_area, return 0 row",
        ),
    ],
)
def test_calculation_id_by_grid_area_scenarios(
    spark: SparkSession,
    args_calculation_id_by_grid_area: dict[str, UUID],
    expected_rows: int,
) -> None:
    # Arrange
    metering_point_periods = metering_point_periods_factory.create(
        spark,
        default_data.create_metering_point_periods_row(
            calculation_id=default_data.DEFAULT_CALCULATION_ID, grid_area_code="804"
        ),
    )

    charge_link_periods = charge_links_factory.create(
        spark,
        default_data.create_charge_link_periods_row(
            calculation_id=default_data.DEFAULT_CALCULATION_ID
        ),
    )

    charge_prices = charge_prices_factory.create(
        spark,
        default_data.create_charge_prices_row(
            calculation_id=default_data.DEFAULT_CALCULATION_ID
        ),
    )

    charge_price_information_periods = charge_price_information_periods_factory.create(
        spark,
        default_data.create_charge_price_information_periods_row(
            calculation_id=default_data.DEFAULT_CALCULATION_ID
        ),
    )

    mock_repository = _get_repository_mock(
        metering_point_periods,
        charge_link_periods,
        charge_prices,
        charge_price_information_periods,
    )

    # Act
    actual_df = read_and_filter(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area=args_calculation_id_by_grid_area,
        energy_supplier_ids=ENERGY_SUPPLIER_IDS,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        repository=mock_repository,
    )

    # Assert
    assert actual_df.count() == expected_rows
