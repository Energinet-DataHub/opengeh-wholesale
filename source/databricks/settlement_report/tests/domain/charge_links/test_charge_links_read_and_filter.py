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
from settlement_report_job.domain.charge_links.read_and_filter import (
    read_and_filter,
    merge_connected_periods,
)

from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.wholesale.column_names import DataProductColumnNames


DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
SYSTEM_OPERATOR_ID = "3333333333333"
GRID_ACCESS_PROVIDER_ID = "4444444444444"
OTHER_ID = "9999999999999"
DEFAULT_TIME_ZONE = "Europe/Copenhagen"

JAN_1ST = datetime(2023, 12, 31, 23)
JAN_2ND = datetime(2024, 1, 1, 23)
JAN_3RD = datetime(2024, 1, 2, 23)
JAN_4TH = datetime(2024, 1, 3, 23)
JAN_5TH = datetime(2024, 1, 4, 23)
JAN_6TH = datetime(2024, 1, 5, 23)
JAN_7TH = datetime(2024, 1, 6, 23)
JAN_8TH = datetime(2024, 1, 7, 23)
JAN_9TH = datetime(2024, 1, 8, 23)


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
def test_read_and_filter__returns_charge_links_that_overlap_with_selected_period(
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
    mock_repository.read_metering_point_periods.return_value = metering_point_periods
    mock_repository.read_charge_link_periods.return_value = charge_link_periods

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
    charge_link_periods = reduce(
        lambda df1, df2: df1.union(df2),
        [
            charge_links_factory.create(
                spark,
                default_data.create_charge_link_periods_row(
                    metering_point_id=metering_point_id,
                ),
            )
            for metering_point_id in METERING_POINT_ID_ABC
        ],
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_periods.return_value = metering_point_periods
    mock_repository.read_charge_link_periods.return_value = charge_link_periods

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
def test_read_and_filter__when_system_operator__returns_expected_charge_links(
    spark: SparkSession,
    charge_owner_id: str,
    is_tax: bool,
    return_rows: bool,
) -> None:
    # Arrange
    metering_point_period = metering_point_periods_factory.create(
        spark,
        default_data.create_metering_point_periods_row(),
    )
    charge_price_information_period_df = charge_price_information_periods.create(
        spark,
        default_data.create_charge_price_information_periods_row(
            charge_owner_id=charge_owner_id,
            is_tax=is_tax,
        ),
    )
    charge_link_periods_df = charge_links_factory.create(
        spark,
        default_data.create_charge_link_periods_row(charge_owner_id=charge_owner_id),
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_periods.return_value = metering_point_period
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
        requesting_actor_id=SYSTEM_OPERATOR_ID,
        repository=mock_repository,
    )

    # Assert
    assert (actual.count() > 0) == return_rows


@pytest.mark.parametrize(
    "charge_owner_id,is_tax,return_rows",
    [
        pytest.param(
            GRID_ACCESS_PROVIDER_ID,
            True,
            True,
            id="grid access provider with tax: include",
        ),
        pytest.param(
            GRID_ACCESS_PROVIDER_ID,
            False,
            False,
            id="grid access provider without tax: include",
        ),
        pytest.param(
            OTHER_ID, False, False, id="other charge owner without tax: exclude"
        ),
        pytest.param(OTHER_ID, True, False, id="other charge owner with tax: include"),
    ],
)
def test_read_and_filter__when_grid_access_provider__returns_expected_charge_links(
    spark: SparkSession,
    charge_owner_id: str,
    is_tax: bool,
    return_rows: bool,
) -> None:
    # Arrange
    metering_point_period = metering_point_periods_factory.create(
        spark,
        default_data.create_metering_point_periods_row(),
    )
    charge_price_information_period_df = charge_price_information_periods.create(
        spark,
        default_data.create_charge_price_information_periods_row(
            charge_owner_id=charge_owner_id,
            is_tax=is_tax,
        ),
    )
    charge_link_periods_df = charge_links_factory.create(
        spark,
        default_data.create_charge_link_periods_row(charge_owner_id=charge_owner_id),
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_periods.return_value = metering_point_period
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
        requesting_actor_market_role=MarketRole.GRID_ACCESS_PROVIDER,
        requesting_actor_id=GRID_ACCESS_PROVIDER_ID,
        repository=mock_repository,
    )

    # Assert
    assert (actual.count() > 0) == return_rows


def test_read_and_filter__when_multiple_metering_point_periods_match_link_period__return_one_row(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_period = metering_point_periods_factory.create(
        spark,
        [
            default_data.create_metering_point_periods_row(
                balance_responsible_party_id="1", from_date=JAN_1ST, to_date=JAN_2ND
            ),
            default_data.create_metering_point_periods_row(
                balance_responsible_party_id="2", from_date=JAN_2ND, to_date=JAN_3RD
            ),
        ],
    )
    charge_link_period = charge_links_factory.create(
        spark,
        default_data.create_charge_link_periods_row(from_date=JAN_1ST, to_date=JAN_3RD),
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_periods.return_value = metering_point_period
    mock_repository.read_charge_link_periods.return_value = charge_link_period

    # Act
    actual = read_and_filter(
        period_start=JAN_1ST,
        period_end=JAN_3RD,
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
    actual.show()
    assert actual.count() == 1
    assert actual.select(DataProductColumnNames.from_date).collect()[0][0] == JAN_1ST
    assert actual.select(DataProductColumnNames.to_date).collect()[0][0] == JAN_3RD


def test_merge_connecting_periods(spark: SparkSession) -> None:
    # Arrange

    # create a dataframe with two periods that are connected
    df = spark.createDataFrame(
        [
            ("1", JAN_1ST, JAN_2ND),
            ("1", JAN_2ND, JAN_3RD),
            ("1", JAN_4TH, JAN_5TH),
            ("1", JAN_5TH, JAN_6TH),
            ("1", JAN_7TH, JAN_8TH),
            ("1", JAN_8TH, JAN_9TH),
        ],
        [
            DataProductColumnNames.charge_key,
            DataProductColumnNames.from_date,
            DataProductColumnNames.to_date,
        ],
    ).orderBy(F.rand())

    # Act
    actual = merge_connected_periods(df, [DataProductColumnNames.charge_key])

    # Assert
    actual.show()
    actual = actual.orderBy(DataProductColumnNames.from_date)
    assert actual.count() == 2
    assert actual.collect()[0][DataProductColumnNames.from_date] == JAN_1ST
    assert actual.collect()[0][DataProductColumnNames.to_date] == JAN_3RD
    assert actual.collect()[1][DataProductColumnNames.from_date] == JAN_4TH
    assert actual.collect()[1][DataProductColumnNames.to_date] == JAN_6TH
