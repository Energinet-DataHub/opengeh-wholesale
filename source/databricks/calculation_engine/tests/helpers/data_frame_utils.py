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


def assert_dataframe_and_schema(
    actual: DataFrame,
    expected: DataFrame,
    ignore_nullability: bool = False,
    ignore_column_order: bool = False,
    ignore_decimal_scale: bool = False,
    ignore_decimal_precision: bool = False,
    columns_to_skip: list[str] | None = None,
    drop_columns_when_actual_and_expected_are_equal: bool = False,
) -> None:
    assert actual is not None, "Actual data frame is None"
    assert expected is not None, "Expected data frame is None"

    if columns_to_skip is not None and len(columns_to_skip) > 0:
        # Assert that the expected value is "IGNORED" to ensure that the skip is explicit in the expected value
        _assert_skipped_columns(expected, columns_to_skip)

        actual = actual.drop(*columns_to_skip)
        expected = expected.drop(*columns_to_skip)

    try:
        assert_schema(
            actual.schema,
            expected.schema,
            ignore_nullability,
            ignore_column_order,
            ignore_decimal_scale,
            ignore_decimal_precision,
        )
    except AssertionError:
        print("SCHEMA MISMATCH:")
        print("ACTUAL SCHEMA:")
        actual.printSchema()
        print("EXPECTED SCHEMA:")
        expected.printSchema()
        raise

    try:
        assert_dataframes_equal(actual, expected)
    except AssertionError:

        if drop_columns_when_actual_and_expected_are_equal:
            actual, expected = drop_columns_if_the_same(actual, expected)

        print("DATA MISMATCH:")
        print("IN ACTUAL BUT NOT IN EXPECTED:")
        actual.subtract(expected).show(3000, False)
        print("IN EXPECTED BUT NOT IN ACTUAL:")
        expected.subtract(actual).show(3000, False)
        raise


def drop_columns_if_the_same(df1: DataFrame, df2: DataFrame) -> (DataFrame, DataFrame):
    column_names = df1.columns
    for column_name in column_names:
        df1_column = df1.select(column_name).collect()
        df2_column = df2.select(column_name).collect()

        if df1_column == df2_column:
            df1 = df1.drop(column_name)
            df2 = df2.drop(column_name)

    return df1, df2


def _assert_skipped_columns(df: DataFrame, column_names: list[str]) -> None:
    # Construct a filter that checks if any column is not 'IGNORED'
    condition = " OR ".join([f"{col_name} != 'IGNORED'" for col_name in column_names])

    non_ignored_df = df.filter(condition)
    count = non_ignored_df.count()

    if count != 0:
        print(
            "ROWS WITH SKIPPED COLUMNS, BUT BAD EXPECTED VALUE (should be 'IGNORED'):"
        )
        non_ignored_df.show(3000, False)

    assert (
        count == 0
    ), f"There are {count} rows where columns are not 'IGNORED' in all the skipped columns: {column_names}."
