from uuid import UUID
from datetime import datetime
from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession

import test_factories.default_test_data_spec as default_data
from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.domain.wholesale_results.read_and_filter import (
    read_and_filter_from_view,
)
from test_factories.default_test_data_spec import create_amounts_per_charge_row
from test_factories.amounts_per_charge_factory import create


DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
SYSTEM_OPERATOR_ID = "3333333333333"
NOT_SYSTEM_OPERATOR_ID = "4444444444444"
DEFAULT_TIME_ZONE = "Europe/Copenhagen"
ENERGY_SUPPLIER_IDS = ["1234567890123", "2345678901234"]


@pytest.mark.parametrize(
    "args_start_date, args_end_date, expected_rows",
    [
        pytest.param(
            datetime(2024, 1, 2, 23),
            datetime(2024, 1, 10, 23),
            1,
            id="when time is within the range, return 1 row",
        ),
        pytest.param(
            datetime(2024, 1, 5, 23),
            datetime(2024, 1, 10, 23),
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
    time = datetime(2024, 1, 3, 23)

    df = create(spark, create_amounts_per_charge_row(time=time))
    mock_repository = Mock()
    mock_repository.read_amounts_per_charge.return_value = df

    # Act
    actual = read_and_filter_from_view(
        energy_supplier_ids=ENERGY_SUPPLIER_IDS,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: UUID(
                default_data.DEFAULT_CALCULATION_ID
            )
        },
        period_start=args_start_date,
        period_end=args_end_date,
        requesting_actor_market_role=MarketRole.ENERGY_SUPPLIER,
        requesting_actor_id=default_data.DEFAULT_CHARGE_OWNER_ID,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == expected_rows


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
    df = create(
        spark,
        create_amounts_per_charge_row(energy_supplier_id=energy_supplier_id),
    )
    mock_repository = Mock()
    mock_repository.read_amounts_per_charge.return_value = df

    # Act
    actual = read_and_filter_from_view(
        energy_supplier_ids=args_energy_supplier_ids,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: UUID(
                default_data.DEFAULT_CALCULATION_ID
            )
        },
        period_start=default_data.DEFAULT_FROM_DATE,
        period_end=default_data.DEFAULT_TO_DATE,
        requesting_actor_market_role=MarketRole.ENERGY_SUPPLIER,
        requesting_actor_id=default_data.DEFAULT_CHARGE_OWNER_ID,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == expected_rows


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
    df = create(
        spark,
        create_amounts_per_charge_row(
            calculation_id=default_data.DEFAULT_CALCULATION_ID, grid_area_code="804"
        ),
    )
    mock_repository = Mock()
    mock_repository.read_amounts_per_charge.return_value = df

    # Act
    actual = read_and_filter_from_view(
        energy_supplier_ids=ENERGY_SUPPLIER_IDS,
        calculation_id_by_grid_area=args_calculation_id_by_grid_area,
        period_start=default_data.DEFAULT_FROM_DATE,
        period_end=default_data.DEFAULT_TO_DATE,
        requesting_actor_market_role=MarketRole.ENERGY_SUPPLIER,
        requesting_actor_id=default_data.DEFAULT_CHARGE_OWNER_ID,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == expected_rows


@pytest.mark.parametrize(
    "args_requesting_actor_market_role, args_requesting_actor_id, is_tax, expected_rows",
    [
        pytest.param(
            MarketRole.GRID_ACCESS_PROVIDER,
            "1111111111111",
            True,
            1,
            id="When grid_access_provider and charge_owner_id equals requesting_actor_id and is_tax is True, return 1 row",
        ),
        pytest.param(
            MarketRole.GRID_ACCESS_PROVIDER,
            default_data.DEFAULT_CHARGE_OWNER_ID,
            False,
            1,
            id="When grid_access_provider and charge_owner_id equals requesting_actor_id and is_tax is False, return 0 rows",
        ),
        pytest.param(
            MarketRole.SYSTEM_OPERATOR,
            default_data.DEFAULT_CHARGE_OWNER_ID,
            True,
            0,
            id="When system_operator and charge_owner_id equals requesting_actor_id and is_tax is True, return 0 rows",
        ),
        pytest.param(
            MarketRole.SYSTEM_OPERATOR,
            default_data.DEFAULT_CHARGE_OWNER_ID,
            False,
            1,
            id="When system_operator and charge_owner_id equals requesting_actor_id and is_tax is False, return 1 rows",
        ),
    ],
)
def test_grid_access_provider_and_system_operator_scenarios(
    spark: SparkSession,
    args_requesting_actor_market_role: MarketRole,
    args_requesting_actor_id: str,
    is_tax: bool,
    expected_rows: int,
) -> None:
    # Arrange
    df = create(
        spark,
        create_amounts_per_charge_row(is_tax=is_tax),
    )
    mock_repository = Mock()
    mock_repository.read_amounts_per_charge.return_value = df

    # Act
    actual = read_and_filter_from_view(
        energy_supplier_ids=ENERGY_SUPPLIER_IDS,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: UUID(
                default_data.DEFAULT_CALCULATION_ID
            )
        },
        period_start=default_data.DEFAULT_FROM_DATE,
        period_end=default_data.DEFAULT_TO_DATE,
        requesting_actor_market_role=args_requesting_actor_market_role,
        requesting_actor_id=args_requesting_actor_id,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == expected_rows
