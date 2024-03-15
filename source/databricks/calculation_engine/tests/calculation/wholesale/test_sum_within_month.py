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
from package.calculation.wholesale.sum_within_month import sum_within_month
from package.codelists import (
    ChargeQuality,
    ChargeType,
)
from package.constants import Colname
import tests.calculation.wholesale.wholesale_results_factory as wholesale_results_factory


PERIOD_START_DATETIME = datetime(2019, 12, 31, 23)


def test__sum_within_month__sums_amount_per_month(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 1, 1),
            total_amount=Decimal("2"),
        ),
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 1, 0),
            total_amount=Decimal("2"),
        ),
    ]
    df = wholesale_results_factory.create(spark, rows)
    # Act
    actual = sum_within_month(
        df,
        PERIOD_START_DATETIME,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("4.000000")
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
        PERIOD_START_DATETIME,
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
        PERIOD_START_DATETIME,
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
        PERIOD_START_DATETIME,
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
        PERIOD_START_DATETIME,
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
        PERIOD_START_DATETIME,
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
        PERIOD_START_DATETIME,
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
