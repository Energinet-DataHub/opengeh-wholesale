import uuid
from unittest.mock import Mock

from pyspark.sql import SparkSession, DataFrame
import test_factories.default_test_data_spec as default_data
import test_factories.metering_point_periods_factory as metering_point_periods_factory
from settlement_report_job.domain.metering_point_periods.read_and_filter import (
    read_and_filter_balance_fixing,
)
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.wholesale.column_names import DataProductColumnNames
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


def test_read_and_filter_balance_fixing__when_duplicate_metering_point_periods__returns_one_period_per_duplicate(
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
    mock_repository = _get_repository_mock(metering_point_periods)

    # Act
    actual = read_and_filter_balance_fixing(
        period_start=d.JAN_1ST,
        period_end=d.JAN_3RD,
        grid_area_codes=default_data.DEFAULT_GRID_AREA_CODE,
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == 1


def test_read_and_filter_balance_fixing__when_overlapping_metering_period__returns_merged_periods(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_periods = metering_point_periods_factory.create(
        spark,
        [
            default_data.create_metering_point_periods_row(
                calculation_id="11111111-9fc8-409a-a169-fbd49479d718",
                from_date=d.JAN_1ST,
                to_date=d.JAN_3RD,
            ),
            default_data.create_metering_point_periods_row(
                calculation_id="22222222-9fc8-409a-a169-fbd49479d718",
                from_date=d.JAN_2ND,
                to_date=d.JAN_4TH,
            ),
        ],
    )
    mock_repository = _get_repository_mock(metering_point_periods)

    # Act
    actual = read_and_filter_balance_fixing(
        period_start=d.JAN_1ST,
        period_end=d.JAN_4TH,
        grid_area_codes=default_data.DEFAULT_GRID_AREA_CODE,
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == 1
    assert actual.collect()[0][DataProductColumnNames.from_date] == d.JAN_1ST
    assert actual.collect()[0][DataProductColumnNames.to_date] == d.JAN_4TH


def test_read_and_filter_balance_fixing__when_metering_periods_with_gap__returns_separate_periods(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_periods = metering_point_periods_factory.create(
        spark,
        [
            default_data.create_metering_point_periods_row(
                calculation_id="11111111-9fc8-409a-a169-fbd49479d718",
                from_date=d.JAN_1ST,
                to_date=d.JAN_2ND,
            ),
            default_data.create_metering_point_periods_row(
                calculation_id="22222222-9fc8-409a-a169-fbd49479d718",
                from_date=d.JAN_3RD,
                to_date=d.JAN_4TH,
            ),
        ],
    )
    mock_repository = _get_repository_mock(metering_point_periods)

    # Act
    actual = read_and_filter_balance_fixing(
        period_start=d.JAN_1ST,
        period_end=d.JAN_4TH,
        grid_area_codes=default_data.DEFAULT_GRID_AREA_CODE,
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == 2
    actual = actual.orderBy(DataProductColumnNames.from_date)
    assert actual.collect()[0][DataProductColumnNames.from_date] == d.JAN_1ST
    assert actual.collect()[0][DataProductColumnNames.to_date] == d.JAN_2ND
    assert actual.collect()[1][DataProductColumnNames.from_date] == d.JAN_3RD
    assert actual.collect()[1][DataProductColumnNames.to_date] == d.JAN_4TH


def test_read_and_filter_balance_fixing__when_period_exceeds_selection_period__returns_period_that_ends_on_the_selection_end_date(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_periods = metering_point_periods_factory.create(
        spark,
        [
            default_data.create_metering_point_periods_row(
                from_date=d.JAN_1ST,
                to_date=d.JAN_5TH,
            ),
        ],
    )
    mock_repository = _get_repository_mock(metering_point_periods)

    # Act
    actual = read_and_filter_balance_fixing(
        period_start=d.JAN_2ND,
        period_end=d.JAN_4TH,
        grid_area_codes=default_data.DEFAULT_GRID_AREA_CODE,
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == 1
    actual = actual.orderBy(DataProductColumnNames.from_date)
    assert actual.collect()[0][DataProductColumnNames.from_date] == d.JAN_2ND
    assert actual.collect()[0][DataProductColumnNames.to_date] == d.JAN_4TH
