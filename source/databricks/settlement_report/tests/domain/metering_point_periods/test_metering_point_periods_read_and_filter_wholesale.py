import uuid
from datetime import datetime
from functools import reduce
from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession, DataFrame
import test_factories.default_test_data_spec as default_data
import test_factories.charge_link_periods_factory as charge_link_periods_factory
import test_factories.metering_point_periods_factory as metering_point_periods_factory
import test_factories.charge_price_information_periods_factory as charge_price_information_periods_factory
from settlement_report_job.domain.metering_point_periods.read_and_filter_wholesale import (
    read_and_filter,
)
from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from utils import Dates as d

DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
SYSTEM_OPERATOR_ID = "3333333333333"
GRID_ACCESS_PROVIDER_ID = "4444444444444"
OTHER_ID = "9999999999999"
DEFAULT_CALCULATION_ID_BY_GRID_AREA = {
    default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(default_data.DEFAULT_CALCULATION_ID)
}


DEFAULT_SELECT_COLUMNS = [
    DataProductColumnNames.metering_point_id,
    DataProductColumnNames.from_date,
    DataProductColumnNames.to_date,
    DataProductColumnNames.grid_area_code,
    DataProductColumnNames.from_grid_area_code,
    DataProductColumnNames.to_grid_area_code,
    DataProductColumnNames.metering_point_type,
    DataProductColumnNames.settlement_method,
    DataProductColumnNames.energy_supplier_id,
]


def _get_repository_mock(
    metering_point_period: DataFrame,
    charge_link_periods: DataFrame | None = None,
    charge_price_information_periods: DataFrame | None = None,
) -> Mock:
    mock_repository = Mock()
    mock_repository.read_metering_point_periods.return_value = metering_point_period

    if charge_link_periods:
        mock_repository.read_charge_link_periods.return_value = charge_link_periods

    if charge_price_information_periods:
        mock_repository.read_charge_price_information_periods.return_value = (
            charge_price_information_periods
        )

    return mock_repository


@pytest.mark.parametrize(
    "from_date,to_date,is_included",
    [
        pytest.param(
            d.JAN_1ST,
            d.JAN_2ND,
            False,
            id="metering point period stops before selected period",
        ),
        pytest.param(
            d.JAN_1ST,
            d.JAN_3RD,
            True,
            id="metering point starts before and ends within selected period",
        ),
        pytest.param(
            d.JAN_3RD,
            d.JAN_4TH,
            True,
            id="metering point period is within selected period",
        ),
        pytest.param(
            d.JAN_3RD,
            d.JAN_5TH,
            True,
            id="metering point starts within but stops after selected period",
        ),
        pytest.param(
            d.JAN_4TH,
            d.JAN_5TH,
            False,
            id="metering point starts after selected period",
        ),
    ],
)
def test_read_and_filter__returns_charge_link_periods_that_overlap_with_selected_period(
    spark: SparkSession,
    from_date: datetime,
    to_date: datetime,
    is_included: bool,
) -> None:
    # Arrange
    calculation_period_start = d.JAN_2ND
    calculation_period_end = d.JAN_4TH

    metering_point_periods = metering_point_periods_factory.create(
        spark,
        default_data.create_metering_point_periods_row(
            from_date=from_date, to_date=to_date
        ),
    )
    mock_repository = _get_repository_mock(metering_point_periods)

    # Act
    actual_df = read_and_filter(
        period_start=calculation_period_start,
        period_end=calculation_period_end,
        calculation_id_by_grid_area=DEFAULT_CALCULATION_ID_BY_GRID_AREA,
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        select_columns=DEFAULT_SELECT_COLUMNS,
        repository=mock_repository,
    )

    # Assert
    assert (actual_df.count() > 0) == is_included


def test_read_and_filter__returns_only_selected_grid_area(
    spark: SparkSession,
) -> None:
    # Arrange
    selected_grid_area_code = "805"
    not_selected_grid_area_code = "806"
    selected_metering_point = "555"
    not_selected_metering_point = "666"

    metering_point_periods = metering_point_periods_factory.create(
        spark,
        default_data.create_metering_point_periods_row(
            grid_area_code=selected_grid_area_code,
            metering_point_id=selected_metering_point,
        ),
    ).union(
        metering_point_periods_factory.create(
            spark,
            default_data.create_metering_point_periods_row(
                grid_area_code=not_selected_grid_area_code,
                metering_point_id=not_selected_metering_point,
            ),
        )
    )
    mock_repository = _get_repository_mock(metering_point_periods)

    # Act
    actual_df = read_and_filter(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area={
            selected_grid_area_code: uuid.UUID(default_data.DEFAULT_CALCULATION_ID)
        },
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        select_columns=DEFAULT_SELECT_COLUMNS,
        repository=mock_repository,
    )

    # Assert
    actual_grid_area_codes = (
        actual_df.select(DataProductColumnNames.grid_area_code).distinct().collect()
    )
    assert len(actual_grid_area_codes) == 1
    assert actual_grid_area_codes[0][0] == selected_grid_area_code


def test_read_and_filter__returns_only_rows_from_selected_calculation_id(
    spark: SparkSession,
) -> None:
    # Arrange
    selected_calculation_id = "11111111-9fc8-409a-a169-fbd49479d718"
    not_selected_calculation_id = "22222222-9fc8-409a-a169-fbd49479d718"
    expected_metering_point_id = "123456789012345678901234567"
    other_metering_point_id = "765432109876543210987654321"
    metering_point_periods = metering_point_periods_factory.create(
        spark,
        default_data.create_metering_point_periods_row(
            calculation_id=selected_calculation_id,
            metering_point_id=expected_metering_point_id,
        ),
    ).union(
        metering_point_periods_factory.create(
            spark,
            default_data.create_metering_point_periods_row(
                calculation_id=not_selected_calculation_id,
                metering_point_id=other_metering_point_id,
            ),
        )
    )
    mock_repository = _get_repository_mock(metering_point_periods)

    # Act
    actual_df = read_and_filter(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(selected_calculation_id)
        },
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        select_columns=DEFAULT_SELECT_COLUMNS,
        repository=mock_repository,
    )

    # Assert
    actual_metering_point_ids = (
        actual_df.select(DataProductColumnNames.metering_point_id).distinct().collect()
    )
    assert len(actual_metering_point_ids) == 1
    assert (
        actual_metering_point_ids[0][DataProductColumnNames.metering_point_id]
        == expected_metering_point_id
    )


ENERGY_SUPPLIER_A = "1000000000000"
ENERGY_SUPPLIER_B = "2000000000000"
ENERGY_SUPPLIER_C = "3000000000000"
ENERGY_SUPPLIERS_ABC = [ENERGY_SUPPLIER_A, ENERGY_SUPPLIER_B, ENERGY_SUPPLIER_C]
METERING_POINT_ID_ABC = ["123", "456", "789"]


@pytest.mark.parametrize(
    "selected_energy_supplier_ids,expected_energy_supplier_ids",
    [
        (None, ENERGY_SUPPLIERS_ABC),
        ([ENERGY_SUPPLIER_B], [ENERGY_SUPPLIER_B]),
        (
            [ENERGY_SUPPLIER_A, ENERGY_SUPPLIER_B],
            [ENERGY_SUPPLIER_A, ENERGY_SUPPLIER_B],
        ),
        (ENERGY_SUPPLIERS_ABC, ENERGY_SUPPLIERS_ABC),
    ],
)
def test_read_and_filter__returns_data_for_expected_energy_suppliers(
    spark: SparkSession,
    selected_energy_supplier_ids: list[str] | None,
    expected_energy_supplier_ids: list[str],
) -> None:
    # Arrange
    metering_point_periods = reduce(
        lambda df1, df2: df1.union(df2),
        [
            metering_point_periods_factory.create(
                spark,
                default_data.create_metering_point_periods_row(
                    energy_supplier_id=energy_supplier_id,
                    metering_point_id=metering_point_id,
                ),
            )
            for energy_supplier_id, metering_point_id in zip(
                ENERGY_SUPPLIERS_ABC, METERING_POINT_ID_ABC
            )
        ],
    )
    mock_repository = _get_repository_mock(metering_point_periods)

    # Act
    actual_df = read_and_filter(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area=DEFAULT_CALCULATION_ID_BY_GRID_AREA,
        energy_supplier_ids=selected_energy_supplier_ids,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        select_columns=DEFAULT_SELECT_COLUMNS,
        repository=mock_repository,
    )

    # Assert
    assert set(
        row[DataProductColumnNames.energy_supplier_id] for row in actual_df.collect()
    ) == set(expected_energy_supplier_ids)


@pytest.mark.parametrize(
    "charge_owner_id,is_tax,return_rows",
    [
        pytest.param(
            SYSTEM_OPERATOR_ID, False, True, id="system operator without tax: include"
        ),
        pytest.param(
            SYSTEM_OPERATOR_ID, True, False, id="system operator with tax: exclude"
        ),
        pytest.param(
            OTHER_ID, False, False, id="other charge owner without tax: exclude"
        ),
        pytest.param(OTHER_ID, True, False, id="other charge owner with tax: exclude"),
    ],
)
def test_read_and_filter__when_system_operator__returns_expected_metering_points(
    spark: SparkSession,
    charge_owner_id: str,
    is_tax: bool,
    return_rows: bool,
) -> None:
    # Arrange
    metering_point_periods = metering_point_periods_factory.create(
        spark,
        default_data.create_metering_point_periods_row(),
    )
    charge_price_information_periods = charge_price_information_periods_factory.create(
        spark,
        default_data.create_charge_price_information_periods_row(
            charge_owner_id=charge_owner_id,
            is_tax=is_tax,
        ),
    )
    charge_link_periods = charge_link_periods_factory.create(
        spark,
        default_data.create_charge_link_periods_row(charge_owner_id=charge_owner_id),
    )
    mock_repository = _get_repository_mock(
        metering_point_periods, charge_link_periods, charge_price_information_periods
    )

    # Act
    actual = read_and_filter(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area=DEFAULT_CALCULATION_ID_BY_GRID_AREA,
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.SYSTEM_OPERATOR,
        requesting_actor_id=SYSTEM_OPERATOR_ID,
        select_columns=DEFAULT_SELECT_COLUMNS,
        repository=mock_repository,
    )

    # Assert
    assert (actual.count() > 0) == return_rows


def test_read_and_filter__when_balance_responsible_party_changes_on_metering_point__returns_single_period(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_periods = metering_point_periods_factory.create(
        spark,
        [
            default_data.create_metering_point_periods_row(
                balance_responsible_party_id="1", from_date=d.JAN_1ST, to_date=d.JAN_2ND
            ),
            default_data.create_metering_point_periods_row(
                balance_responsible_party_id="2", from_date=d.JAN_2ND, to_date=d.JAN_3RD
            ),
        ],
    )
    charge_link_periods = charge_link_periods_factory.create(
        spark,
        default_data.create_charge_link_periods_row(
            from_date=d.JAN_1ST,
            to_date=d.JAN_3RD,
            charge_owner_id=GRID_ACCESS_PROVIDER_ID,
        ),
    )
    charge_price_information_periods = charge_price_information_periods_factory.create(
        spark,
        default_data.create_charge_price_information_periods_row(
            from_date=d.JAN_1ST,
            to_date=d.JAN_3RD,
            is_tax=True,
            charge_owner_id=GRID_ACCESS_PROVIDER_ID,
        ),
    )
    mock_repository = _get_repository_mock(
        metering_point_periods, charge_link_periods, charge_price_information_periods
    )

    # Act
    actual = read_and_filter(
        period_start=d.JAN_1ST,
        period_end=d.JAN_3RD,
        calculation_id_by_grid_area=DEFAULT_CALCULATION_ID_BY_GRID_AREA,
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.GRID_ACCESS_PROVIDER,
        requesting_actor_id=GRID_ACCESS_PROVIDER_ID,
        select_columns=DEFAULT_SELECT_COLUMNS,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == 1
    assert actual.select(DataProductColumnNames.from_date).collect()[0][0] == d.JAN_1ST
    assert actual.select(DataProductColumnNames.to_date).collect()[0][0] == d.JAN_3RD


def test_read_and_filter__when_datahub_user_and_energy_supplier_changes_on_metering_point__returns_two_link_periods(
    spark: SparkSession,
) -> None:
    # Arrange
    es_id_a = "111"
    es_id_b = "222"
    metering_point_periods = metering_point_periods_factory.create(
        spark,
        [
            default_data.create_metering_point_periods_row(
                energy_supplier_id=es_id_a,
                from_date=d.JAN_1ST,
                to_date=d.JAN_2ND,
            ),
            default_data.create_metering_point_periods_row(
                energy_supplier_id=es_id_b,
                from_date=d.JAN_2ND,
                to_date=d.JAN_3RD,
            ),
        ],
    )
    charge_link_periods = charge_link_periods_factory.create(
        spark,
        default_data.create_charge_link_periods_row(
            from_date=d.JAN_1ST, to_date=d.JAN_3RD
        ),
    )
    mock_repository = _get_repository_mock(metering_point_periods, charge_link_periods)

    # Act
    actual = read_and_filter(
        period_start=d.JAN_1ST,
        period_end=d.JAN_3RD,
        calculation_id_by_grid_area=DEFAULT_CALCULATION_ID_BY_GRID_AREA,
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        select_columns=DEFAULT_SELECT_COLUMNS,
        repository=mock_repository,
    )

    # Assert
    actual = actual.orderBy(DataProductColumnNames.from_date)
    assert actual.count() == 2

    actual_row_1 = actual.collect()[0]
    assert actual_row_1[DataProductColumnNames.energy_supplier_id] == es_id_a
    assert actual_row_1[DataProductColumnNames.from_date] == d.JAN_1ST
    assert actual_row_1[DataProductColumnNames.to_date] == d.JAN_2ND

    actual_row_2 = actual.collect()[1]
    assert actual_row_2[DataProductColumnNames.energy_supplier_id] == es_id_b
    assert actual_row_2[DataProductColumnNames.from_date] == d.JAN_2ND
    assert actual_row_2[DataProductColumnNames.to_date] == d.JAN_3RD


def test_read_and_filter__when_duplicate_metering_point_periods__returns_one_period_per_duplicate(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_periods = metering_point_periods_factory.create(
        spark,
        [
            default_data.create_metering_point_periods_row(),
            default_data.create_metering_point_periods_row(),
        ],
    )
    charge_link_periods = charge_link_periods_factory.create(
        spark,
        default_data.create_charge_link_periods_row(),
    )
    mock_repository = _get_repository_mock(metering_point_periods, charge_link_periods)

    # Act
    actual = read_and_filter(
        period_start=d.JAN_1ST,
        period_end=d.JAN_3RD,
        calculation_id_by_grid_area=DEFAULT_CALCULATION_ID_BY_GRID_AREA,
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        select_columns=DEFAULT_SELECT_COLUMNS,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == 1
