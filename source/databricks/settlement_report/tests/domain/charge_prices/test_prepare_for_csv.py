import uuid
from datetime import datetime
from decimal import Decimal
from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import monotonically_increasing_id
import pyspark.sql.functions as F
from pyspark.sql.types import DecimalType

import tests.test_factories.default_test_data_spec as default_data
from settlement_report_job.domain.charge_prices.prepare_for_csv import prepare_for_csv
from settlement_report_job.domain.charge_prices.read_and_filter import read_and_filter
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.wholesale.data_values import (
    ChargeResolutionDataProductValue,
)
import tests.test_factories.charge_link_periods_factory as charge_links_factory
import test_factories.metering_point_periods_factory as metering_point_periods_factory
import test_factories.charge_prices_factory as charge_prices_factory
import test_factories.charge_price_information_periods_factory as charge_price_information_periods_factory

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


@pytest.mark.parametrize(
    "resolution",
    [
        ChargeResolutionDataProductValue.HOUR,
        ChargeResolutionDataProductValue.DAY,
        ChargeResolutionDataProductValue.MONTH,
    ],
)
def test_prepare_for_csv(
    spark: SparkSession, resolution: ChargeResolutionDataProductValue
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
        default_data.create_charge_price_information_periods_row(resolution=resolution),
    )

    mock_repository = _get_repository_mock(
        metering_point_periods,
        charge_link_periods,
        charge_prices,
        charge_price_information_periods,
    )

    charge_prices = read_and_filter(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area=DEFAULT_CALCULATION_ID_BY_GRID_AREA,
        energy_supplier_ids=ENERGY_SUPPLIER_IDS,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        repository=mock_repository,
    )
    charge_prices.show()

    # Act
    result_df = prepare_for_csv(
        charge_prices=charge_prices,
    )
    result_df.show()
    # Assert
    assert result_df.count() == 1
