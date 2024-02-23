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

import pyspark.sql.functions as f
from pyspark.sql import DataFrame

from package.common import assert_schema


def set_column(
    df: DataFrame,
    column_name: str,
    column_value: str | list,
) -> DataFrame:
    """Set the column value of all rows in the data frame."""
    if isinstance(column_value, list):
        return df.withColumn(column_name, f.array(*map(f.lit, column_value)))
    return df.withColumn(column_name, f.lit(column_value))


def assert_dataframes_equal(actual: DataFrame, expected: DataFrame) -> None:
    assert actual.subtract(expected).count() == 0
    assert expected.subtract(actual).count() == 0
    assert actual.subtract(expected).count() == 0


def assert_dataframes(
    actual: DataFrame,
    expected: DataFrame,
    ignore_nullability: bool = False,
    ignore_column_order: bool = False,
    ignore_decimal_scale: bool = False,
    ignore_decimal_precision: bool = False,
    ignore_schema: bool = False,
) -> None:

    if not ignore_schema:
        assert_schema(
            actual.schema,
            expected.schema,
            ignore_nullability,
            ignore_column_order,
            ignore_decimal_scale,
            ignore_decimal_precision,
        )
    assert_dataframes_equal(actual, expected)


def dataframes_show(
    actual: DataFrame,
    expected: DataFrame,
    print_schema: bool = False,
    show_dataframe: bool = True,
    save_expected_to_csv: bool = False,
    save_actual_to_csv: bool = False,
) -> None:

    if print_schema:
        print(actual.schema)
        print(expected.schema)

    if show_dataframe:
        print("ACTUAL: Count " + str(actual.count()))
        actual.show(1000, truncate=False)
        print("EXPECTED: Count " + str(expected.count()))
        expected.show(1000, truncate=False)

    if save_actual_to_csv:
        df = actual.select([f.col(c).cast("string") for c in actual.columns])
        df.coalesce(1).write.csv("actual.csv", header=True, mode="overwrite", sep=";")

    if save_expected_to_csv:
        df = expected.select([f.col(c).cast("string") for c in expected.columns])
        df.coalesce(1).write.csv("expected.csv", header=True, mode="overwrite", sep=";")
