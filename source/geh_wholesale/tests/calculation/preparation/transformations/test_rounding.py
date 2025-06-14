from datetime import datetime
from decimal import Decimal

from pyspark.sql import SparkSession

from geh_wholesale.calculation.preparation.transformations.rounding import (
    round_quantity,
)
from geh_wholesale.constants import Colname


def test_special_quantity_rounding(spark: SparkSession) -> None:
    # Arrange
    rows = [
        {
            Colname.observation_time: datetime(2022, 1, 1, 1, 0),
            Colname.quantity: Decimal("222.03075"),
        },
        {
            Colname.observation_time: datetime(2022, 1, 1, 1, 15),
            Colname.quantity: Decimal("222.03075"),
        },
        {
            Colname.observation_time: datetime(2022, 1, 1, 1, 30),
            Colname.quantity: Decimal("222.03075"),
        },
        {
            Colname.observation_time: datetime(2022, 1, 1, 1, 45),
            Colname.quantity: Decimal("222.03075"),
        },
    ]
    df = spark.createDataFrame(rows)

    # Act
    actual = round_quantity(df)

    # Assert
    assert actual.collect()[0][Colname.quantity] == Decimal("222.031")
    assert actual.collect()[1][Colname.quantity] == Decimal("222.031")
    assert actual.collect()[2][Colname.quantity] == Decimal("222.030")
    assert actual.collect()[3][Colname.quantity] == Decimal("222.031")


def test_special_quantity_rounding_when_two_energy_supplier_at_the_same_time(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        {
            Colname.observation_time: datetime(2022, 1, 1, 1, 0),
            Colname.quantity: Decimal("222.03075"),
            Colname.energy_supplier_id: "1",
        },
        {
            Colname.observation_time: datetime(2022, 1, 1, 1, 15),
            Colname.quantity: Decimal("222.03075"),
            Colname.energy_supplier_id: "1",
        },
        {
            Colname.observation_time: datetime(2022, 1, 1, 1, 30),
            Colname.quantity: Decimal("222.03075"),
            Colname.energy_supplier_id: "1",
        },
        {
            Colname.observation_time: datetime(2022, 1, 1, 1, 45),
            Colname.quantity: Decimal("222.03075"),
            Colname.energy_supplier_id: "1",
        },
        {
            Colname.observation_time: datetime(2022, 1, 1, 1, 0),
            Colname.quantity: Decimal("25.00125"),
            Colname.energy_supplier_id: "2",
        },
        {
            Colname.observation_time: datetime(2022, 1, 1, 1, 15),
            Colname.quantity: Decimal("25.00125"),
            Colname.energy_supplier_id: "2",
        },
        {
            Colname.observation_time: datetime(2022, 1, 1, 1, 30),
            Colname.quantity: Decimal("25.00125"),
            Colname.energy_supplier_id: "2",
        },
        {
            Colname.observation_time: datetime(2022, 1, 1, 1, 45),
            Colname.quantity: Decimal("25.00125"),
            Colname.energy_supplier_id: "2",
        },
    ]
    df = spark.createDataFrame(rows)

    # Act
    actual = round_quantity(df)
    actual = actual.orderBy(
        Colname.energy_supplier_id,
        Colname.observation_time,
    )

    # Assert
    # Energy supplier 1
    assert actual.collect()[0][Colname.quantity] == Decimal("222.031")
    assert actual.collect()[1][Colname.quantity] == Decimal("222.031")
    assert actual.collect()[2][Colname.quantity] == Decimal("222.030")
    assert actual.collect()[3][Colname.quantity] == Decimal("222.031")
    # Energy supplier 2
    assert actual.collect()[4][Colname.quantity] == Decimal("25.001")
    assert actual.collect()[5][Colname.quantity] == Decimal("25.002")
    assert actual.collect()[6][Colname.quantity] == Decimal("25.001")
    assert actual.collect()[7][Colname.quantity] == Decimal("25.001")
