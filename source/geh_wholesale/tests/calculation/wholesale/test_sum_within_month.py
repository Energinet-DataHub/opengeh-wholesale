from datetime import datetime
from decimal import Decimal

from pyspark.sql import SparkSession

import tests.calculation.wholesale.factories.wholesale_results_factory as wholesale_results_factory
from geh_wholesale.calculation.wholesale.sum_within_month import sum_within_month
from geh_wholesale.constants import Colname

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
