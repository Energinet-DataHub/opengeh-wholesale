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
            total_amount=Decimal("2"),
        ),
        wholesale_results_factory.create_row(
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


def test__sum_within_month__qualities_always_set_to_none(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            qualities=[ChargeQuality.CALCULATED.value],
        ),
        wholesale_results_factory.create_row(
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
    assert actual.collect()[0][Colname.qualities] is None
    assert actual.count() == 1


def test__sum_within_month__charge_time_is_always_period_start_datetime(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            charge_time=datetime(2020, 1, 3, 0),
        ),
    ]
    df = wholesale_results_factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        df,
        PERIOD_START_DATETIME,
    ).df

    # Assert
    assert actual.collect()[0][Colname.charge_time] == PERIOD_START_DATETIME


def test__sum_within_month__total_quantity_always_set_to_none(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            total_quantity=Decimal("1"),
        ),
        wholesale_results_factory.create_row(
            total_quantity=Decimal("1"),
        ),
    ]
    df = wholesale_results_factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        df,
        PERIOD_START_DATETIME,
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_quantity] is None
    assert actual.count() == 1


def test__sum_within_month__sets_charge_price_to_none(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            charge_price=Decimal("1"),
        ),
        wholesale_results_factory.create_row(
            charge_price=Decimal("1"),
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


def test__sum_within_month__can_sum_when_one_value_is_none(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        wholesale_results_factory.create_row(
            total_amount=None,
        ),
        wholesale_results_factory.create_row(
            total_amount=Decimal("6.000000"),
        ),
    ]
    df = wholesale_results_factory.create(spark, rows)

    # Act
    actual = sum_within_month(
        df,
        datetime(2019, 12, 31, 23),
    ).df

    # Assert
    assert actual.collect()[0][Colname.total_amount] == Decimal("6.000000")
    assert actual.count() == 1
