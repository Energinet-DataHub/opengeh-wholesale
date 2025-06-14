from decimal import Decimal

from pyspark.sql import Row, SparkSession

from geh_wholesale.calculation.energy.aggregators.transformations import (
    aggregate_quantity_and_quality,
)
from geh_wholesale.constants import Colname


class TestWhenValidInput:
    def test_returns_single_row_for_group(self, spark: SparkSession):
        # Arrange
        rows = [
            Row(**{Colname.quantity: 1, Colname.quality: "foo", "group": "some-group"}),
            Row(**{Colname.quantity: 2, Colname.quality: "foo", "group": "some-group"}),
        ]
        df = spark.createDataFrame(data=rows)

        # Act
        actual = aggregate_quantity_and_quality(df, ["group"])

        # assert
        actual_rows = actual.collect()
        assert len(actual_rows) == 1

    def test_returns_a_row_for_each_group(self, spark: SparkSession):
        # Arrange
        rows = [
            Row(**{Colname.quantity: 1, Colname.quality: "foo", "group": "some-group"}),
            Row(
                **{
                    Colname.quantity: 2,
                    Colname.quality: "foo",
                    "group": "another-group",
                }
            ),
        ]
        df = spark.createDataFrame(data=rows)

        # Act
        actual = aggregate_quantity_and_quality(df, ["group"])

        # assert
        actual_rows = actual.collect()
        assert len(actual_rows) == 2

    def test_returns_sum_of_quantity_in_group(self, spark: SparkSession):
        # Arrange
        rows = [
            Row(
                **{
                    Colname.quantity: Decimal("1.111"),
                    Colname.quality: "foo",
                    "group": "some-group",
                }
            ),
            Row(
                **{
                    Colname.quantity: Decimal("2.222"),
                    Colname.quality: "foo",
                    "group": "some-group",
                }
            ),
        ]
        df = spark.createDataFrame(data=rows)

        # Act
        actual = aggregate_quantity_and_quality(df, ["group"])

        # assert
        actual_row = actual.collect()[0]
        assert actual_row[Colname.quantity] == Decimal("3.333")

    def test_returns_sum_of_quantity_in_each_group(self, spark: SparkSession):
        # Arrange
        rows = [
            Row(
                **{
                    Colname.quantity: Decimal("1.1"),
                    Colname.quality: "foo",
                    "group": "one-group",
                }
            ),
            Row(
                **{
                    Colname.quantity: Decimal("2.2"),
                    Colname.quality: "foo",
                    "group": "one-group",
                }
            ),
            Row(
                **{
                    Colname.quantity: Decimal("3.0"),
                    Colname.quality: "foo",
                    "group": "another-group",
                }
            ),
            Row(
                **{
                    Colname.quantity: Decimal("4.0"),
                    Colname.quality: "foo",
                    "group": "another-group",
                }
            ),
        ]
        df = spark.createDataFrame(data=rows)

        # Act
        actual = aggregate_quantity_and_quality(df, ["group"])

        # assert
        actual_rows = actual.collect()
        assert actual_rows[0][Colname.quantity] == Decimal("3.3")
        assert actual_rows[1][Colname.quantity] == Decimal("7.0")

    def test_returns_distinct_qualities_in_group(self, spark: SparkSession):
        # Arrange
        group = "group"
        rows = [
            Row(**{Colname.quantity: 1, Colname.quality: "foo", group: "the-group"}),
            Row(**{Colname.quantity: 2, Colname.quality: "bar", group: "the-group"}),
            Row(**{Colname.quantity: 3, Colname.quality: "bar", group: "the-group"}),
            Row(
                **{
                    Colname.quantity: 1,
                    Colname.quality: "baz",
                    group: "other-group",
                }
            ),
        ]
        df = spark.createDataFrame(data=rows)

        # Act
        actual = aggregate_quantity_and_quality(df, [group])

        # assert
        actual_rows = actual.collect()
        assert sorted(actual_rows[0][Colname.qualities]) == ["bar", "foo"]
        assert actual_rows[1][Colname.qualities] == ["baz"]
