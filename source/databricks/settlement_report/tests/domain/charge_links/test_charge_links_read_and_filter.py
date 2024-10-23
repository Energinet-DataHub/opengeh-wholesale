import uuid
from datetime import datetime
from functools import reduce
from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession, functions as F
import test_factories.default_test_data_spec as default_data
import test_factories.charge_link_periods_factory as charge_links_factory
import test_factories.metering_point_periods_factory as metering_point_periods_factory
import test_factories.charge_price_information_periods_factory as charge_price_information_periods
from settlement_report_job.domain.charge_links.read_and_filter import read_and_filter

from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.wholesale.column_names import DataProductColumnNames


DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
SYSTEM_OPERATOR_ID = "3333333333333"
NOT_SYSTEM_OPERATOR_ID = "4444444444444"
DEFAULT_TIME_ZONE = "Europe/Copenhagen"

JAN_1ST = datetime(2023, 12, 31, 23)
JAN_2ND = datetime(2024, 1, 1, 23)
JAN_3RD = datetime(2024, 1, 2, 23)
JAN_4TH = datetime(2024, 1, 3, 23)
JAN_5TH = datetime(2024, 1, 4, 23)


@pytest.mark.parametrize(
    "charge_link_from_date,charge_link_to_date,is_included",
    [
        pytest.param(
            JAN_1ST,
            JAN_2ND,
            False,
            id="charge link period stops before selected period",
        ),
        pytest.param(
            JAN_1ST,
            JAN_3RD,
            True,
            id="charge link starts before and ends within selected period",
        ),
        pytest.param(
            JAN_3RD,
            JAN_4TH,
            True,
            id="charge link period is within selected period",
        ),
        pytest.param(
            JAN_3RD,
            JAN_5TH,
            True,
            id="charge link starts within but stops after selected period",
        ),
        pytest.param(
            JAN_4TH, JAN_5TH, False, id="charge link starts after selected period"
        ),
    ],
)
def test_read_and_filter__returns_link_periods_that_overlaps_with_selected_period(
    spark: SparkSession,
    charge_link_from_date: datetime,
    charge_link_to_date: datetime,
    is_included: bool,
) -> None:
    # Arrange
    period_start = JAN_2ND
    period_end = JAN_4TH

    metering_point_periods = metering_point_periods_factory.create(
        spark,
        default_data.create_metering_point_periods_row(
            from_date=charge_link_from_date, to_date=charge_link_to_date
        ),
    )

    charge_link_periods = charge_links_factory.create(
        spark,
        default_data.create_charge_link_periods_row(
            from_date=charge_link_from_date, to_date=charge_link_to_date
        ),
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_periods.return_value = charge_link_periods
    mock_repository.read_charge_link_periods.return_value = metering_point_periods

    # Act
    actual_df = read_and_filter(
        period_start=period_start,
        period_end=period_end,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(
                default_data.DEFAULT_CALCULATION_ID
            )
        },
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
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
    charge_link_periods = charge_links_factory.create(
        spark,
        default_data.create_charge_link_periods_row(
            metering_point_id=selected_metering_point,
        ),
    ).union(
        charge_links_factory.create(
            spark,
            default_data.create_charge_link_periods_row(
                metering_point_id=not_selected_metering_point,
            ),
        )
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = (
        metering_point_periods
    )
    mock_repository.read_charge_link_periods.return_value = charge_link_periods

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
    charge_link_periods = charge_links_factory.create(
        spark,
        default_data.create_charge_link_periods_row(
            calculation_id=selected_calculation_id,
            metering_point_id=expected_metering_point_id,
        ),
    ).union(
        charge_links_factory.create(
            spark,
            default_data.create_charge_link_periods_row(
                calculation_id=not_selected_calculation_id,
                metering_point_id=other_metering_point_id,
            ),
        )
    )
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
    mock_repository = Mock()
    mock_repository.read_charge_link_periods.return_value = charge_link_periods
    mock_repository.read_metering_point_periods.return_value = metering_point_periods

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
    df = reduce(
        lambda df1, df2: df1.union(df2),
        [
            time_series_factory.create(
                spark,
                default_data.create_time_series_data_spec(
                    energy_supplier_id=energy_supplier_id,
                ),
            )
            for energy_supplier_id in ENERGY_SUPPLIERS_ABC
        ],
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    actual_df = read_and_filter(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(
                default_data.DEFAULT_CALCULATION_ID
            )
        },
        energy_supplier_ids=selected_energy_supplier_ids,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        repository=mock_repository,
    )

    # Assert
    assert set(
        row[DataProductColumnNames.energy_supplier_id] for row in actual_df.collect()
    ) == set(expected_energy_supplier_ids)


@pytest.mark.parametrize(
    "charge_owner_id,return_rows",
    [
        (SYSTEM_OPERATOR_ID, True),
        (NOT_SYSTEM_OPERATOR_ID, False),
    ],
)
def test_read_and_filter_for_wholesale__when_system_operator__returns_only_time_series_with_system_operator_as_charge_owner(
    spark: SparkSession,
    charge_owner_id: str,
    return_rows: bool,
) -> None:
    # Arrange
    time_series_df = time_series_factory.create(
        spark,
        default_data.create_time_series_data_spec(),
    )
    charge_price_information_period_df = charge_price_information_periods.create(
        spark,
        default_data.create_charge_price_information_periods_row(
            charge_owner_id=SYSTEM_OPERATOR_ID
        ),
    )
    charge_link_periods_df = charge_links_factory.create(
        spark,
        default_data.create_charge_link_periods_row(charge_owner_id=SYSTEM_OPERATOR_ID),
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = time_series_df
    mock_repository.read_charge_price_information_periods.return_value = (
        charge_price_information_period_df
    )
    mock_repository.read_charge_link_periods.return_value = charge_link_periods_df

    # Act
    actual = read_and_filter(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(
                default_data.DEFAULT_CALCULATION_ID
            )
        },
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.SYSTEM_OPERATOR,
        requesting_actor_id=charge_owner_id,
        repository=mock_repository,
    )

    # Assert
    assert (actual.count() > 0) == return_rows
