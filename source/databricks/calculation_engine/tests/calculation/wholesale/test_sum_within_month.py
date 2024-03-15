# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from datetime import datetime
from decimal import Decimal

from pyspark.sql import SparkSession
from pyspark.sql import functions as f

from package.calculation.wholesale.subscription_calculators import calculate
from package.calculation.wholesale.sum_within_month import sum_within_month
from package.codelists import (
    ChargeQuality,
    ChargeType,
    MeteringPointType,
    SettlementMethod,
    QuantityQuality,
)
from package.constants import Colname
import tests.calculation.wholesale.wholesale_results_factory as wholesale_results_factory


class DefaultValues:
    GRID_AREA = "543"
    CHARGE_CODE = "4000"
    CHARGE_OWNER = "001"
    CHARGE_TIME_HOUR_0 = datetime(2019, 12, 31, 23)
    CHARGE_PRICE = Decimal("2.000005")
    CHARGE_QUANTITY = 1
    ENERGY_SUPPLIER_ID = "1234567890123"
    METERING_POINT_ID = "123456789012345678901234567"
    METERING_POINT_TYPE = MeteringPointType.CONSUMPTION
    SETTLEMENT_METHOD = SettlementMethod.FLEX
    QUANTITY = Decimal("1.005")
    QUALITY = QuantityQuality.CALCULATED
    CALCULATION_PERIOD_START = datetime(2020, 1, 31, 23, 0)
    CALCULATION_PERIOD_END = datetime(2020, 2, 29, 23, 0)
    DAYS_IN_MONTH = 29
    CALCULATION_MONTH = 2
    TIME_ZONE = "Europe/Copenhagen"


def test__sum_within_month__tariff__sums_amount_per_month(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 1, 1),
            charge_price=Decimal("2.000005"),
            total_amount=Decimal("2.010005"),
            qualities=[ChargeQuality.CALCULATED.value],
        ),
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 1, 0),
            charge_price=Decimal("2.000005"),
            total_amount=Decimal("2.010005"),
            qualities=[ChargeQuality.CALCULATED.value],
        ),
    ]
    df = wholesale_results_factory.create(spark, rows)
    # Act
    actual = sum_within_month(
        df,
        datetime(2019, 12, 31, 23),
        ChargeType.TARIFF,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("4.020010")
    assert actual.count() == 1


def test__sum_within_month__subscription__sums_amount_per_month(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 31, 23),
            charge_price=Decimal("2.000005"),
            total_amount=Decimal("2.010005"),
            charge_type=ChargeType.SUBSCRIPTION.value,
        ),
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 2, 15, 23),
            charge_price=Decimal("2.000005"),
            total_amount=Decimal("2.010005"),
            charge_type=ChargeType.SUBSCRIPTION.value,
        ),
    ]
    df = wholesale_results_factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        df,
        DefaultValues.CALCULATION_PERIOD_START,
        ChargeType.SUBSCRIPTION,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("4.020010")
    assert actual.count() == 1


def test__sum_within_month__sums_across_metering_point_types(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 1, 1),
            charge_price=Decimal("2.000005"),
            total_amount=Decimal("2.010005"),
            qualities=[ChargeQuality.CALCULATED.value],
            metering_point_type=MeteringPointType.PRODUCTION,
        ),
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 1, 0),
            charge_price=Decimal("2.000005"),
            total_amount=Decimal("2.010005"),
            qualities=[ChargeQuality.CALCULATED.value],
            metering_point_type=MeteringPointType.CONSUMPTION,
        ),
    ]
    df = wholesale_results_factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        df,
        datetime(2019, 12, 31, 23),
        ChargeType.TARIFF,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("4.020010")
    assert actual.count() == 1


def test__sum_within_month__tariff__joins_qualities(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 1, 1),
            charge_price=Decimal("2.000005"),
            total_amount=Decimal("2.010005"),
            qualities=[ChargeQuality.CALCULATED.value],
        ),
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 1, 0),
            charge_price=Decimal("2.000005"),
            total_amount=Decimal("2.010005"),
            qualities=[ChargeQuality.ESTIMATED.value],
        ),
    ]
    df = wholesale_results_factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        df,
        datetime(2019, 12, 31, 23),
        ChargeType.TARIFF,
    ).df

    # Assert
    assert actual.collect()[0][Colname.qualities] == ["calculated", "estimated"]
    assert actual.collect()[0][Colname.total_amount] == Decimal("4.020010")
    assert actual.count() == 1


def test__sum_within_month__subscription__sets_qualities_to_none(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 31, 23),
            charge_price=Decimal("2.000005"),
            total_amount=Decimal("2.010005"),
            charge_type=ChargeType.SUBSCRIPTION.value,
        ),
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 2, 15, 23),
            charge_price=Decimal("2.000005"),
            total_amount=Decimal("2.010005"),
            charge_type=ChargeType.SUBSCRIPTION.value,
        ),
    ]
    df = wholesale_results_factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        df,
        datetime(2019, 12, 31, 23),
        ChargeType.SUBSCRIPTION,
    ).df

    # Assert
    assert actual.collect()[0][Colname.qualities] is None
    assert actual.count() == 1


def test__sum_within_month__groups_by_local_time_months(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 1, 1),
            charge_price=Decimal("2.000005"),
            total_amount=Decimal("2.010005"),
            qualities=[ChargeQuality.CALCULATED.value],
        ),
        wholesale_results_factory.create_row(
            charge_time=datetime(2019, 12, 31, 23),
            charge_price=Decimal("2.000005"),
            total_amount=Decimal("2.010005"),
            qualities=[ChargeQuality.CALCULATED.value],
        ),
    ]
    df = wholesale_results_factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        df,
        datetime(2019, 12, 31, 23),
        ChargeType.TARIFF,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("4.020010")
    assert actual.collect()[0][Colname.charge_time] == datetime(2019, 12, 31, 23)
    assert actual.count() == 1


def test__sum_within_month__charge_time_always_start_of_month(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 3, 0),
            charge_price=Decimal("2.000005"),
            total_amount=Decimal("2.010005"),
            qualities=[ChargeQuality.CALCULATED.value],
        ),
    ]
    df = wholesale_results_factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        df,
        datetime(2019, 12, 31, 23),
        ChargeType.TARIFF,
    ).df

    # Assert
    assert actual.collect()[0][Colname.charge_time] == datetime(2019, 12, 31, 23)


def test__sum_within_month__sums_quantity_per_month(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 1, 1),
            charge_price=Decimal("2.000005"),
            total_amount=Decimal("2.010005"),
            qualities=[ChargeQuality.CALCULATED.value],
            total_quantity=Decimal("1.111"),
        ),
        wholesale_results_factory.create_row(
            charge_time=datetime(2019, 12, 31, 23),
            charge_price=Decimal("2.000005"),
            total_amount=Decimal("2.010005"),
            qualities=[ChargeQuality.CALCULATED.value],
            total_quantity=Decimal("1.111"),
        ),
    ]
    df = wholesale_results_factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        df,
        datetime(2019, 12, 31, 23),
        ChargeType.TARIFF,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_quantity] == Decimal("2.222")
    assert actual.count() == 1


def test__sum_within_month__sets_charge_price_to_none(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 1, 0),
            charge_price=Decimal("1.111111"),
            total_amount=Decimal("2.010005"),
            qualities=[ChargeQuality.CALCULATED.value],
        ),
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 1, 1),
            charge_price=Decimal("1.111111"),
            total_amount=Decimal("2.010005"),
            qualities=[ChargeQuality.CALCULATED.value],
        ),
    ]
    df = wholesale_results_factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        df,
        datetime(2019, 12, 31, 23),
        ChargeType.TARIFF,
    ).df

    # Assert
    assert actual.collect()[0][Colname.charge_price] is None
    assert actual.count() == 1


def test__sum_within_month__when_all_charge_prices_are_none__sums_charge_price_and_total_amount_per_month_to_none(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 1, 0),
            charge_price=None,
            total_amount=None,
            qualities=[ChargeQuality.CALCULATED.value],
        ),
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 1, 1),
            charge_price=None,
            total_amount=None,
            qualities=[ChargeQuality.CALCULATED.value],
        ),
    ]
    df = wholesale_results_factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        df,
        datetime(2019, 12, 31, 23),
        ChargeType.TARIFF,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] is None
    assert actual.collect()[0][Colname.charge_price] is None
    assert actual.count() == 1


def test__sum_within_month__when_one_tariff_has_charge_price_none__sums_charge_price_and_total_amount_per_month_to_expected_value(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 1, 1),
            charge_price=None,
            total_amount=None,
            qualities=[ChargeQuality.CALCULATED.value],
            total_quantity=Decimal("1.111"),
        ),
        wholesale_results_factory.create_row(
            charge_time=datetime(2019, 12, 31, 23),
            charge_price=Decimal("2.000000"),
            total_amount=Decimal("6.000000"),
            qualities=[ChargeQuality.CALCULATED.value],
            total_quantity=Decimal("3.000000"),
        ),
    ]
    df = wholesale_results_factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        df,
        datetime(2019, 12, 31, 23),
        ChargeType.TARIFF,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("6.000000")
    assert actual.count() == 1
