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
from decimal import Decimal

from pyspark.sql import Row, SparkSession

from package.calculation.energy.aggregators.transformations import (
    aggregate_sum_quantity_and_qualities,
)
from package.constants import Colname


class TestWhenValidInput:
    def test_returns_single_row_for_group(self, spark: SparkSession):
        # Arrange
        rows = [
            Row(
                **{
                    Colname.sum_quantity: 1,
                    Colname.qualities: ["foo"],
                    "group": "some-group",
                }
            ),
            Row(
                **{
                    Colname.sum_quantity: 2,
                    Colname.qualities: ["foo"],
                    "group": "some-group",
                }
            ),
        ]
        df = spark.createDataFrame(data=rows)

        # Act
        actual = aggregate_sum_quantity_and_qualities(df, ["group"])

        # assert
        actual_rows = actual.collect()
        assert len(actual_rows) == 1

    def test_returns_a_row_for_each_group(self, spark: SparkSession):
        # Arrange
        rows = [
            Row(
                **{
                    Colname.sum_quantity: 1,
                    Colname.qualities: ["foo"],
                    "group": "some-group",
                }
            ),
            Row(
                **{
                    Colname.sum_quantity: 2,
                    Colname.qualities: ["foo"],
                    "group": "another-group",
                }
            ),
        ]
        df = spark.createDataFrame(data=rows)

        # Act
        actual = aggregate_sum_quantity_and_qualities(df, ["group"])

        # assert
        actual_rows = actual.collect()
        assert len(actual_rows) == 2

    def test_returns_sum_of_quantity_in_group(self, spark: SparkSession):
        # Arrange
        rows = [
            Row(
                **{
                    Colname.sum_quantity: Decimal("1.111"),
                    Colname.qualities: ["foo"],
                    "group": "some-group",
                }
            ),
            Row(
                **{
                    Colname.sum_quantity: Decimal("2.222"),
                    Colname.qualities: ["foo"],
                    "group": "some-group",
                }
            ),
        ]
        df = spark.createDataFrame(data=rows)

        # Act
        actual = aggregate_sum_quantity_and_qualities(df, ["group"])

        # assert
        actual_row = actual.collect()[0]
        assert actual_row[Colname.sum_quantity] == Decimal("3.333")

    def test_returns_sum_of_quantity_in_each_group(self, spark: SparkSession):
        # Arrange
        rows = [
            Row(
                **{
                    Colname.sum_quantity: Decimal("1.1"),
                    Colname.qualities: ["foo"],
                    "group": "some-group",
                }
            ),
            Row(
                **{
                    Colname.sum_quantity: Decimal("2.2"),
                    Colname.qualities: ["foo"],
                    "group": "some-group",
                }
            ),
            Row(
                **{
                    Colname.sum_quantity: Decimal("3.0"),
                    Colname.qualities: ["foo"],
                    "group": "another-group",
                }
            ),
            Row(
                **{
                    Colname.sum_quantity: Decimal("4.0"),
                    Colname.qualities: ["foo"],
                    "group": "another-group",
                }
            ),
        ]
        df = spark.createDataFrame(data=rows)

        # Act
        actual = aggregate_sum_quantity_and_qualities(df, ["group"])

        # assert
        actual_rows = actual.collect()
        assert actual_rows[0][Colname.sum_quantity] == Decimal("3.3")
        assert actual_rows[1][Colname.sum_quantity] == Decimal("7.0")

    def test_returns_distinct_qualities_in_group(self, spark: SparkSession):
        # Arrange
        group = "group"
        rows = [
            Row(
                **{
                    Colname.sum_quantity: 1,
                    Colname.qualities: ["foo", "bar"],
                    group: "the-group",
                }
            ),
            Row(
                **{
                    Colname.sum_quantity: 2,
                    Colname.qualities: ["baz"],
                    group: "the-group",
                }
            ),
            Row(
                **{
                    Colname.sum_quantity: 3,
                    Colname.qualities: [],
                    group: "the-group",
                }
            ),
            Row(
                **{Colname.sum_quantity: 4, Colname.qualities: None, group: "the-group"}
            ),
            Row(
                **{
                    Colname.sum_quantity: 5,
                    Colname.qualities: None,
                    group: "other-group",
                }
            ),
        ]
        df = spark.createDataFrame(data=rows)

        # Act
        actual = aggregate_sum_quantity_and_qualities(df, [group])

        # assert
        actual_rows = actual.collect()
        assert sorted(actual_rows[0][Colname.qualities]) == ["bar", "baz", "foo"]
        assert actual_rows[1][Colname.qualities] == []
