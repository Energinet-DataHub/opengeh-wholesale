import uuid
from datetime import datetime, timedelta
from functools import reduce
from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession
import test_factories.default_test_data_spec as default_data
import test_factories.metering_point_time_series_factory as time_series_factory
import test_factories.charge_link_periods_factory as charge_links_factory
import test_factories.charge_price_information_periods_factory as charge_price_information_periods


from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.DataProductValues.metering_point_resolution import (
    MeteringPointResolutionDataProductValue,
)
from settlement_report_job.domain.time_series.read_and_filter import (
    read_and_filter_for_wholesale,
)
from settlement_report_job.infrastructure.column_names import DataProductColumnNames

DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
SYSTEM_OPERATOR_ID = "3333333333333"
NOT_SYSTEM_OPERATOR_ID = "4444444444444"


@pytest.mark.parametrize(
    "resolution",
    [
        MeteringPointResolutionDataProductValue.HOUR,
        MeteringPointResolutionDataProductValue.QUARTER,
    ],
)
def test_read_and_filter_for_wholesale__when_input_has_both_resolution_types__returns_only_data_with_expected_resolution(
    spark: SparkSession,
    resolution: MeteringPointResolutionDataProductValue,
) -> None:
    # Arrange
    hourly_metering_point_id = "1111111111111"
    quarterly_metering_point_id = "1515151515115"
    expected_metering_point_id = (
        hourly_metering_point_id
        if resolution == MeteringPointResolutionDataProductValue.HOUR
        else quarterly_metering_point_id
    )
    spec_hour = default_data.create_time_series_data_spec(
        metering_point_id=hourly_metering_point_id,
        resolution=MeteringPointResolutionDataProductValue.HOUR,
    )
    spec_quarter = default_data.create_time_series_data_spec(
        metering_point_id=quarterly_metering_point_id,
        resolution=MeteringPointResolutionDataProductValue.QUARTER,
    )
    df = time_series_factory.create(spark, spec_hour).union(
        time_series_factory.create(spark, spec_quarter)
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
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        metering_point_resolution=resolution,
        repository=mock_repository,
    )

    # Assert
    assert actual_df.count() == 1
    assert (
        actual_df.collect()[0][DataProductColumnNames.metering_point_id]
        == expected_metering_point_id
    )


def test_read_and_filter_for_wholesale__returns_only_days_within_selected_period(
    spark: SparkSession,
) -> None:
    # Arrange
    data_from_date = datetime(2024, 1, 1, 23)
    data_to_date = datetime(2024, 1, 31, 23)
    number_of_days_in_period = 2
    number_of_hours_in_period = number_of_days_in_period * 24
    period_start = datetime(2024, 1, 10, 23)
    period_end = period_start + timedelta(days=number_of_days_in_period)

    df = time_series_factory.create(
        spark,
        default_data.create_time_series_data_spec(
            from_date=data_from_date, to_date=data_to_date
        ),
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    actual_df = read_and_filter_for_wholesale(
        period_start=period_start,
        period_end=period_end,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(
                default_data.DEFAULT_CALCULATION_ID
            )
        },
        energy_supplier_ids=None,
        metering_point_resolution=MeteringPointResolutionDataProductValue.HOUR,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        repository=mock_repository,
    )

    # Assert

    assert actual_df.count() == number_of_hours_in_period
    assert (
        actual_df.agg({DataProductColumnNames.observation_time: "max"}).collect()[0][0]
        == period_start
    )
    assert (
        actual_df.agg({DataProductColumnNames.observation_time: "min"}).collect()[0][0]
        == period_end
    )


def test_read_and_filter_for_wholesale__returns_only_selected_grid_area(
    spark: SparkSession,
) -> None:
    # Arrange
    selected_grid_area_code = "805"
    not_selected_grid_area_code = "806"
    df = time_series_factory.create(
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
    assert actual_df.count() == 1
    assert (
        actual_df.collect()[0][DataProductColumnNames.grid_area_code]
        == selected_grid_area_code
    )


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
        default_data.create_charge_price_information_periods_data_spec(
            charge_owner_id=SYSTEM_OPERATOR_ID
        ),
    )
    charge_link_periods_df = charge_links_factory.create(
        spark,
        default_data.create_charge_link_periods_data_spec(
            charge_owner_id=SYSTEM_OPERATOR_ID
        ),
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
