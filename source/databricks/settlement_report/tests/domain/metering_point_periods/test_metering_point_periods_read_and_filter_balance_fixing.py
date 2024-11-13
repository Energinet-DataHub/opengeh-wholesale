from datetime import datetime
from unittest.mock import Mock

from pyspark.sql import SparkSession, DataFrame

import test_factories.default_test_data_spec as default_data
from settlement_report_job.domain.metering_point_periods.read_and_filter_balance_fixing import (
    read_and_filter,
)
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from test_factories import latest_calculations_factory, metering_point_periods_factory
from utils import Dates as d, DEFAULT_TIME_ZONE


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
    latest_calculations: DataFrame,
    charge_link_periods: DataFrame | None = None,
    charge_price_information_periods: DataFrame | None = None,
) -> Mock:
    mock_repository = Mock()
    mock_repository.read_metering_point_periods.return_value = metering_point_period
    mock_repository.read_latest_calculations.return_value = latest_calculations

    if charge_link_periods:
        mock_repository.read_charge_link_periods.return_value = charge_link_periods

    if charge_price_information_periods:
        mock_repository.read_charge_price_information_periods.return_value = (
            charge_price_information_periods
        )

    return mock_repository


def test_read_and_filter__when_duplicate_metering_point_periods__returns_one_period_per_duplicate(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_periods = metering_point_periods_factory.create(
        spark,
        [
            default_data.create_metering_point_periods_row(
                from_date=d.JAN_1ST, to_date=d.JAN_2ND
            ),
            default_data.create_metering_point_periods_row(
                from_date=d.JAN_1ST, to_date=d.JAN_2ND
            ),
        ],
    )
    latest_calculations = latest_calculations_factory.create(
        spark,
        [
            default_data.create_latest_calculations_per_day_row(
                start_of_day=d.JAN_1ST,
            ),
        ],
    )
    mock_repository = _get_repository_mock(
        metering_point_periods, latest_calculations=latest_calculations
    )

    # Act
    actual = read_and_filter(
        period_start=d.JAN_1ST,
        period_end=d.JAN_2ND,
        grid_area_codes=default_data.DEFAULT_GRID_AREA_CODE,
        energy_supplier_ids=None,
        select_columns=DEFAULT_SELECT_COLUMNS,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == 1


def test_read_and_filter__when_metering_periods_with_gap__returns_separate_periods(
    spark: SparkSession,
) -> None:
    # Arrange
    latest_calculations = latest_calculations_factory.create(
        spark,
        [
            default_data.create_latest_calculations_per_day_row(
                start_of_day=d.JAN_1ST,
            ),
            default_data.create_latest_calculations_per_day_row(
                start_of_day=d.JAN_2ND,
            ),
            default_data.create_latest_calculations_per_day_row(
                start_of_day=d.JAN_3RD,
            ),
        ],
    )
    metering_point_periods = metering_point_periods_factory.create(
        spark,
        [
            default_data.create_metering_point_periods_row(
                from_date=d.JAN_1ST,
                to_date=d.JAN_2ND,
            ),
            default_data.create_metering_point_periods_row(
                from_date=d.JAN_3RD,
                to_date=d.JAN_4TH,
            ),
        ],
    )
    mock_repository = _get_repository_mock(metering_point_periods, latest_calculations)

    # Act
    actual = read_and_filter(
        period_start=d.JAN_1ST,
        period_end=d.JAN_4TH,
        grid_area_codes=default_data.DEFAULT_GRID_AREA_CODE,
        energy_supplier_ids=None,
        select_columns=DEFAULT_SELECT_COLUMNS,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == 2
    actual = actual.orderBy(DataProductColumnNames.from_date)
    assert actual.collect()[0][DataProductColumnNames.from_date] == d.JAN_1ST
    assert actual.collect()[0][DataProductColumnNames.to_date] == d.JAN_2ND
    assert actual.collect()[1][DataProductColumnNames.from_date] == d.JAN_3RD
    assert actual.collect()[1][DataProductColumnNames.to_date] == d.JAN_4TH


def test_read_and_filter__when_period_exceeds_selection_period__returns_period_that_ends_on_the_selection_end_date(
    spark: SparkSession,
) -> None:
    # Arrange
    latest_calculations = latest_calculations_factory.create(
        spark,
        [
            default_data.create_latest_calculations_per_day_row(
                start_of_day=d.JAN_1ST,
            ),
            default_data.create_latest_calculations_per_day_row(
                start_of_day=d.JAN_2ND,
            ),
            default_data.create_latest_calculations_per_day_row(
                start_of_day=d.JAN_3RD,
            ),
            default_data.create_latest_calculations_per_day_row(
                start_of_day=d.JAN_4TH,
            ),
        ],
    )
    metering_point_periods = metering_point_periods_factory.create(
        spark,
        [
            default_data.create_metering_point_periods_row(
                from_date=d.JAN_1ST,
                to_date=d.JAN_4TH,
            ),
        ],
    )
    mock_repository = _get_repository_mock(metering_point_periods, latest_calculations)

    # Act
    actual = read_and_filter(
        period_start=d.JAN_2ND,
        period_end=d.JAN_3RD,
        grid_area_codes=default_data.DEFAULT_GRID_AREA_CODE,
        energy_supplier_ids=None,
        select_columns=DEFAULT_SELECT_COLUMNS,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == 1
    actual = actual.orderBy(DataProductColumnNames.from_date)
    assert actual.collect()[0][DataProductColumnNames.from_date] == d.JAN_2ND
    assert actual.collect()[0][DataProductColumnNames.to_date] == d.JAN_3RD


def test_read_and_filter__when_calculation_overlap_in_time__returns_latest(
    spark: SparkSession,
) -> None:
    # Arrange
    calculation_id_1 = "11111111-1111-1111-1111-11111111"
    calculation_id_2 = "22222222-2222-2222-2222-22222222"
    latest_calculations = latest_calculations_factory.create(
        spark,
        [
            default_data.create_latest_calculations_per_day_row(
                calculation_id=calculation_id_1,
                start_of_day=d.JAN_1ST,
            ),
            default_data.create_latest_calculations_per_day_row(
                calculation_id=calculation_id_2,
                start_of_day=d.JAN_2ND,
            ),
        ],
    )

    metering_point_periods = metering_point_periods_factory.create(
        spark,
        [
            default_data.create_metering_point_periods_row(
                calculation_id=calculation_id_1,
                metering_point_id="1",
                from_date=d.JAN_1ST,
                to_date=d.JAN_3RD,
            ),
            default_data.create_metering_point_periods_row(
                calculation_id=calculation_id_2,
                metering_point_id="2",
                from_date=d.JAN_1ST,
                to_date=d.JAN_3RD,
            ),
        ],
    )
    mock_repository = _get_repository_mock(metering_point_periods, latest_calculations)

    # Act
    actual = read_and_filter(
        period_start=d.JAN_1ST,
        period_end=d.JAN_3RD,
        grid_area_codes=default_data.DEFAULT_GRID_AREA_CODE,
        energy_supplier_ids=None,
        select_columns=DEFAULT_SELECT_COLUMNS,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == 2
    assert (
        actual.orderBy(DataProductColumnNames.from_date).collect()[0][
            DataProductColumnNames.metering_point_id
        ]
        == "1"
    )
    assert (
        actual.orderBy(DataProductColumnNames.from_date).collect()[1][
            DataProductColumnNames.metering_point_id
        ]
        == "2"
    )


def test_read_and_filter__when_metering_point_period_is_shorter_in_newer_calculation__returns_the_shorter_period(
    spark: SparkSession,
) -> None:
    # Arrange
    calculation_id_1 = "11111111-1111-1111-1111-11111111"
    calculation_id_2 = "22222222-2222-2222-2222-22222222"
    latest_calculations = latest_calculations_factory.create(
        spark,
        [
            default_data.create_latest_calculations_per_day_row(
                calculation_id=calculation_id_2,
                start_of_day=d.JAN_1ST,
            ),
            default_data.create_latest_calculations_per_day_row(
                calculation_id=calculation_id_2,
                start_of_day=d.JAN_2ND,
            ),
        ],
    )

    metering_point_periods = metering_point_periods_factory.create(
        spark,
        [
            default_data.create_metering_point_periods_row(
                calculation_id=calculation_id_1,
                from_date=d.JAN_1ST,
                to_date=d.JAN_3RD,
            ),
            default_data.create_metering_point_periods_row(
                calculation_id=calculation_id_2,
                from_date=d.JAN_2ND,
                to_date=d.JAN_3RD,
            ),
        ],
    )
    mock_repository = _get_repository_mock(metering_point_periods, latest_calculations)

    # Act
    actual = read_and_filter(
        period_start=d.JAN_1ST,
        period_end=d.JAN_3RD,
        grid_area_codes=default_data.DEFAULT_GRID_AREA_CODE,
        energy_supplier_ids=None,
        select_columns=DEFAULT_SELECT_COLUMNS,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == 1
    assert actual.collect()[0][DataProductColumnNames.from_date] == d.JAN_2ND
    assert actual.collect()[0][DataProductColumnNames.to_date] == d.JAN_3RD


def test_read_and_filter__when_daylight_saving_time_returns_expected(
    spark: SparkSession,
) -> None:
    # Arrange
    from_date = datetime(2024, 3, 30, 23)
    to_date = datetime(2024, 4, 1, 22)
    latest_calculations = latest_calculations_factory.create(
        spark,
        [
            default_data.create_latest_calculations_per_day_row(
                start_of_day=datetime(2024, 3, 30, 23),
            ),
            default_data.create_latest_calculations_per_day_row(
                start_of_day=datetime(2024, 3, 31, 22),
            ),
        ],
    )

    metering_point_periods = metering_point_periods_factory.create(
        spark,
        [
            default_data.create_metering_point_periods_row(
                from_date=from_date,
                to_date=to_date,
            ),
        ],
    )
    mock_repository = _get_repository_mock(metering_point_periods, latest_calculations)

    # Act
    actual = read_and_filter(
        period_start=from_date,
        period_end=to_date,
        grid_area_codes=default_data.DEFAULT_GRID_AREA_CODE,
        energy_supplier_ids=None,
        select_columns=DEFAULT_SELECT_COLUMNS,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == 1
    assert actual.collect()[0][DataProductColumnNames.from_date] == from_date
    assert actual.collect()[0][DataProductColumnNames.to_date] == to_date
