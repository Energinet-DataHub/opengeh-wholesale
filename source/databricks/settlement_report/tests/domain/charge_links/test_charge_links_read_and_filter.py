import uuid
from datetime import datetime, timedelta
from functools import reduce
from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession, functions as F
import test_factories.default_test_data_spec as default_data
import test_factories.charge_link_periods_factory as charge_links_factory
import test_factories.metering_point_periods_factory as metering_point_periods_factory
import test_factories.charge_price_information_periods_factory as charge_price_information_periods
from settlement_report_job.domain.charge_links.read_and_filter import read_and_filter
from settlement_report_job.wholesale.data_values import (
    CalculationTypeDataProductValue,
)

from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.wholesale.column_names import DataProductColumnNames
from test_factories import latest_calculations_factory


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
            JAN_1ST, JAN_2ND, False, id="charge link period stops before period"
        ),
        pytest.param(
            JAN_1ST,
            JAN_3RD,
            True,
            id="charge link starts before and ends within period",
        ),
        pytest.param(
            JAN_3RD,
            JAN_4TH,
            True,
            id="charge link period is within period",
        ),
        pytest.param(
            JAN_3RD,
            JAN_5TH,
            True,
            id="charge link starts within but stops after period",
        ),
        pytest.param(JAN_4TH, JAN_5TH, False, id="charge link starts after period"),
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
    df = charge.create(
        spark,
        default_data.create_time_series_data_spec(
            grid_area_code=selected_grid_area_code,
        ),
    ).union(
        time_series_factory.create(
            spark,
            default_data.create_time_series_data_spec(
                grid_area_code=not_selected_grid_area_code,
            ),
        )
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    actual_df = read_and_filter_for_wholesale(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area={
            selected_grid_area_code: uuid.UUID(default_data.DEFAULT_CALCULATION_ID)
        },
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        metering_point_resolution=MeteringPointResolutionDataProductValue.HOUR,
        repository=mock_repository,
    )

    # Assert
    actual_grid_area_codes = (
        actual_df.select(DataProductColumnNames.grid_area_code).distinct().collect()
    )
    assert len(actual_grid_area_codes) == 1
    assert actual_grid_area_codes[0][0] == selected_grid_area_code


def test_read_and_filter_for_wholesale__returns_only_metering_points_from_selected_calculation_id(
    spark: SparkSession,
) -> None:
    # Arrange
    selected_calculation_id = "11111111-9fc8-409a-a169-fbd49479d718"
    not_selected_calculation_id = "22222222-9fc8-409a-a169-fbd49479d718"
    expected_metering_point_id = "123456789012345678901234567"
    other_metering_point_id = "765432109876543210987654321"
    df = time_series_factory.create(
        spark,
        default_data.create_time_series_data_spec(
            calculation_id=selected_calculation_id,
            metering_point_id=expected_metering_point_id,
        ),
    ).union(
        time_series_factory.create(
            spark,
            default_data.create_time_series_data_spec(
                calculation_id=not_selected_calculation_id,
                metering_point_id=other_metering_point_id,
            ),
        )
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    actual_df = read_and_filter_for_wholesale(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(selected_calculation_id)
        },
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        metering_point_resolution=MeteringPointResolutionDataProductValue.HOUR,
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
def test_read_and_filter_for_wholesale__returns_data_for_expected_energy_suppliers(
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
    actual_df = read_and_filter_for_wholesale(
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
        metering_point_resolution=MeteringPointResolutionDataProductValue.HOUR,
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
    actual = read_and_filter_for_wholesale(
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
        metering_point_resolution=MeteringPointResolutionDataProductValue.HOUR,
        repository=mock_repository,
    )

    # Assert
    assert (actual.count() > 0) == return_rows


def test_read_and_filter_for_balance_fixing__returns_only_time_series_from_latest_calculations(
    spark: SparkSession,
) -> None:
    # Arrange
    not_latest_calculation_id = "11111111-9fc8-409a-a169-fbd49479d718"
    latest_calculation_id = "22222222-9fc8-409a-a169-fbd49479d718"
    time_series_df = reduce(
        lambda df1, df2: df1.union(df2),
        [
            time_series_factory.create(
                spark,
                default_data.create_time_series_data_spec(
                    calculation_id=calculation_id
                ),
            )
            for calculation_id in [latest_calculation_id, not_latest_calculation_id]
        ],
    )
    latest_calculations = latest_calculations_factory.create(
        spark,
        default_data.create_latest_calculations_per_day_row(
            calculation_id=latest_calculation_id,
            calculation_type=CalculationTypeDataProductValue.BALANCE_FIXING,
        ),
    )

    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = time_series_df
    mock_repository.read_latest_calculations.return_value = latest_calculations

    # Act
    actual_df = read_and_filter_for_balance_fixing(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        grid_area_codes=[default_data.DEFAULT_GRID_AREA_CODE],
        energy_supplier_ids=None,
        metering_point_resolution=default_data.DEFAULT_RESOLUTION,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    actual_calculation_ids = (
        actual_df.select(DataProductColumnNames.calculation_id).distinct().collect()
    )
    assert len(actual_calculation_ids) == 1
    assert (
        actual_calculation_ids[0][DataProductColumnNames.calculation_id]
        == latest_calculation_id
    )


def test_read_and_filter_for_balance_fixing__returns_only_balance_fixing_results(
    spark: SparkSession,
) -> None:
    # Arrange
    calculation_id_and_type = {
        "11111111-9fc8-409a-a169-fbd49479d718": CalculationTypeDataProductValue.AGGREGATION,
        "22222222-9fc8-409a-a169-fbd49479d718": CalculationTypeDataProductValue.BALANCE_FIXING,
        "33333333-9fc8-409a-a169-fbd49479d718": CalculationTypeDataProductValue.WHOLESALE_FIXING,
        "44444444-9fc8-409a-a169-fbd49479d718": CalculationTypeDataProductValue.FIRST_CORRECTION_SETTLEMENT,
        "55555555-9fc8-409a-a169-fbd49479d718": CalculationTypeDataProductValue.SECOND_CORRECTION_SETTLEMENT,
        "66666666-9fc8-409a-a169-fbd49479d718": CalculationTypeDataProductValue.THIRD_CORRECTION_SETTLEMENT,
    }
    time_series = reduce(
        lambda df1, df2: df1.union(df2),
        [
            time_series_factory.create(
                spark,
                default_data.create_time_series_data_spec(
                    calculation_id=calc_id, calculation_type=calc_type
                ),
            )
            for calc_id, calc_type in calculation_id_and_type.items()
        ],
    )

    latest_calculations = reduce(
        lambda df1, df2: df1.union(df2),
        [
            latest_calculations_factory.create(
                spark,
                default_data.create_latest_calculations_per_day_row(
                    calculation_id=calc_id, calculation_type=calc_type
                ),
            )
            for calc_id, calc_type in calculation_id_and_type.items()
        ],
    )

    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = time_series
    mock_repository.read_latest_calculations.return_value = latest_calculations

    # Act
    actual_df = read_and_filter_for_balance_fixing(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        grid_area_codes=[default_data.DEFAULT_GRID_AREA_CODE],
        energy_supplier_ids=None,
        metering_point_resolution=default_data.DEFAULT_RESOLUTION,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    actual_calculation_ids = (
        actual_df.select(DataProductColumnNames.calculation_id).distinct().collect()
    )
    assert len(actual_calculation_ids) == 1
    assert (
        actual_calculation_ids[0][DataProductColumnNames.calculation_id]
        == "22222222-9fc8-409a-a169-fbd49479d718"
    )


def test_read_and_filter_for_balance_fixing__when_two_calculations_with_time_overlap__returns_only_latest_calculation_data(
    spark: SparkSession,
) -> None:
    # Arrange
    day_1 = DEFAULT_FROM_DATE
    day_2 = day_1 + timedelta(days=1)
    day_3 = day_1 + timedelta(days=2)
    day_4 = day_1 + timedelta(days=3)  # exclusive
    calculation_id_1 = "11111111-9fc8-409a-a169-fbd49479d718"
    calculation_id_2 = "22222222-9fc8-409a-a169-fbd49479d718"
    calc_type = CalculationTypeDataProductValue.BALANCE_FIXING

    time_series = reduce(
        lambda df1, df2: df1.union(df2),
        [
            time_series_factory.create(
                spark,
                default_data.create_time_series_data_spec(
                    calculation_id=calc_id,
                    calculation_type=calc_type,
                    from_date=from_date,
                    to_date=to_date,
                ),
            )
            for calc_id, from_date, to_date in [
                (calculation_id_1, day_1, day_3),
                (calculation_id_2, day_2, day_4),
            ]
        ],
    )

    latest_calculations = latest_calculations_factory.create(
        spark,
        [
            default_data.create_latest_calculations_per_day_row(
                calculation_id=calc_id,
                calculation_type=calc_type,
                start_of_day=start_of_day,
            )
            for calc_id, start_of_day in [
                (calculation_id_1, day_1),
                (calculation_id_1, day_2),
                (calculation_id_2, day_3),
            ]
        ],
    )

    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = time_series
    mock_repository.read_latest_calculations.return_value = latest_calculations

    # Act
    actual_df = read_and_filter_for_balance_fixing(
        period_start=day_1,
        period_end=day_4,
        grid_area_codes=[default_data.DEFAULT_GRID_AREA_CODE],
        energy_supplier_ids=None,
        metering_point_resolution=default_data.DEFAULT_RESOLUTION,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert

    for day, expected_calculation_id in zip(
        [day_1, day_2, day_3], [calculation_id_1, calculation_id_1, calculation_id_2]
    ):
        actual_calculation_ids = (
            actual_df.where(
                (F.col(DataProductColumnNames.observation_time) >= day)
                & (
                    F.col(DataProductColumnNames.observation_time)
                    < day + timedelta(days=1)
                )
            )
            .select(DataProductColumnNames.calculation_id)
            .distinct()
            .collect()
        )
        assert len(actual_calculation_ids) == 1
        assert actual_calculation_ids[0][0] == expected_calculation_id


def test_read_and_filter_for_balance_fixing__latest_calculation_for_grid_area(
    spark: SparkSession,
) -> None:
    # Arrange
    day_1 = DEFAULT_FROM_DATE
    day_2 = day_1 + timedelta(days=1)  # exclusive
    grid_area_1 = "805"
    grid_area_2 = "806"
    calculation_id_1 = "11111111-9fc8-409a-a169-fbd49479d718"
    calculation_id_2 = "22222222-9fc8-409a-a169-fbd49479d718"
    calc_type = CalculationTypeDataProductValue.BALANCE_FIXING

    time_series = reduce(
        lambda df1, df2: df1.union(df2),
        [
            time_series_factory.create(
                spark,
                default_data.create_time_series_data_spec(
                    calculation_id=calc_id,
                    calculation_type=calc_type,
                    grid_area_code=grid_area,
                    from_date=day_1,
                    to_date=day_2,
                ),
            )
            for calc_id, grid_area in [
                (calculation_id_1, grid_area_1),
                (calculation_id_1, grid_area_2),
                (calculation_id_2, grid_area_2),
            ]
        ],
    )

    latest_calculations = latest_calculations_factory.create(
        spark,
        [
            default_data.create_latest_calculations_per_day_row(
                calculation_id=calculation_id_1,
                calculation_type=calc_type,
                grid_area_code=grid_area_1,
                start_of_day=day_1,
            ),
            default_data.create_latest_calculations_per_day_row(
                calculation_id=calculation_id_2,
                calculation_type=calc_type,
                grid_area_code=grid_area_2,
                start_of_day=day_1,
            ),
        ],
    )

    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = time_series
    mock_repository.read_latest_calculations.return_value = latest_calculations

    # Act
    actual_df = read_and_filter_for_balance_fixing(
        period_start=day_1,
        period_end=day_2,
        grid_area_codes=[grid_area_1, grid_area_2],
        energy_supplier_ids=None,
        metering_point_resolution=default_data.DEFAULT_RESOLUTION,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert all(
        row[DataProductColumnNames.calculation_id] == calculation_id_1
        for row in actual_df.where(
            F.col(DataProductColumnNames.grid_area_code) == grid_area_1
        )
        .select(DataProductColumnNames.calculation_id)
        .distinct()
        .collect()
    )

    assert all(
        row[DataProductColumnNames.calculation_id] == calculation_id_2
        for row in actual_df.where(
            F.col(DataProductColumnNames.grid_area_code) == grid_area_2
        )
        .select(DataProductColumnNames.calculation_id)
        .distinct()
        .collect()
    )
