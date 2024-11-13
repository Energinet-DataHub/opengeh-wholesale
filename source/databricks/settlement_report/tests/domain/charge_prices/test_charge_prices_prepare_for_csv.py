import uuid
from datetime import datetime, timedelta
from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession, DataFrame, functions as F

import test_factories.default_test_data_spec as default_data
from settlement_report_job.domain.charge_prices.prepare_for_csv import prepare_for_csv


import test_factories.charge_prices_factory as charge_prices_factory
from settlement_report_job.domain.utils.csv_column_names import CsvColumnNames
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from settlement_report_job.infrastructure.wholesale.data_values import (
    ChargeResolutionDataProductValue,
)

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
        ChargeResolutionDataProductValue.DAY,
        ChargeResolutionDataProductValue.MONTH,
    ],
)
def test_when_resolution_is_day_or_month_return_only_value_in_energy_price_1(
    spark: SparkSession,
    resolution: ChargeResolutionDataProductValue,
) -> None:
    # Arrange
    filtered_charge_prices = (
        charge_prices_factory.create(
            spark,
            default_data.create_charge_prices_row(),
        )
        .withColumn(
            DataProductColumnNames.grid_area_code,
            F.lit(default_data.DEFAULT_GRID_AREA_CODE),
        )
        .withColumn(DataProductColumnNames.is_tax, F.lit(False))
        .withColumn(
            DataProductColumnNames.resolution,
            F.lit(resolution.value),
        )
    )

    # Act
    result_df = prepare_for_csv(
        filtered_charge_prices=filtered_charge_prices, time_zone=DEFAULT_TIME_ZONE
    )

    # Assert
    assert result_df.count() == 1
    result = result_df.collect()[0]
    assert result["ENERGYPRICE1"] == default_data.DEFAULT_CHARGE_PRICE
    for i in range(2, 26):
        assert result[f"ENERGYPRICE{i}"] is None


def test_when_resolution_is_hour_return_one_row_with_value_in_every_energy_price_except_25(
    spark: SparkSession,
) -> None:
    # Arrange
    hours_in_day = [JAN_1ST + timedelta(hours=i) for i in range(24)]
    charge_price_rows = []
    for i in range(24):
        charge_price_rows.append(
            default_data.create_charge_prices_row(
                charge_time=hours_in_day[i],
                charge_price=default_data.DEFAULT_CHARGE_PRICE + i,
            )
        )

    filtered_charge_prices = (
        charge_prices_factory.create(
            spark,
            charge_price_rows,
        )
        .withColumn(
            DataProductColumnNames.grid_area_code,
            F.lit(default_data.DEFAULT_GRID_AREA_CODE),
        )
        .withColumn(DataProductColumnNames.is_tax, F.lit(False))
        .withColumn(
            DataProductColumnNames.resolution,
            F.lit(ChargeResolutionDataProductValue.HOUR.value),
        )
    )

    # Act
    result_df = prepare_for_csv(
        filtered_charge_prices=filtered_charge_prices, time_zone=DEFAULT_TIME_ZONE
    )

    # Assert
    assert result_df.count() == 1
    result = result_df.collect()[0]
    for i in range(1, 25):
        assert (
            result[f"{CsvColumnNames.energy_price}{i}"]
            == default_data.DEFAULT_CHARGE_PRICE + i - 1
        )
    assert result[f"{CsvColumnNames.energy_price}25"] is None


@pytest.mark.parametrize(
    "is_tax, expected_tax_indicator",
    [
        (True, 1),
        (False, 0),
    ],
)
def test_tax_indicator_is_converted_correctly(
    spark: SparkSession,
    is_tax: bool,
    expected_tax_indicator: int,
) -> None:
    # Arrange
    filtered_charge_prices = (
        charge_prices_factory.create(
            spark,
            default_data.create_charge_prices_row(),
        )
        .withColumn(
            DataProductColumnNames.grid_area_code,
            F.lit(default_data.DEFAULT_GRID_AREA_CODE),
        )
        .withColumn(DataProductColumnNames.is_tax, F.lit(is_tax))
        .withColumn(
            DataProductColumnNames.resolution,
            F.lit(ChargeResolutionDataProductValue.DAY.value),
        )
    )

    # Act
    result_df = prepare_for_csv(
        filtered_charge_prices=filtered_charge_prices, time_zone=DEFAULT_TIME_ZONE
    )

    # Assert
    assert result_df.collect()[0][CsvColumnNames.is_tax] == expected_tax_indicator
