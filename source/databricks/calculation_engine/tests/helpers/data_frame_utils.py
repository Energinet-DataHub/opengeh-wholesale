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
from pyspark.testing import assertDataFrameEqual

from package.common import assert_schema
from tests.testsession_configuration import FeatureTestsConfiguration


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
    actual_excess = actual.subtract(expected)
    expected_excess = expected.subtract(actual)

    # do the count once here to avoid materializing multiple times
    actual_excess_count = actual_excess.count()
    expected_excess_count = expected_excess.count()

    if actual_excess_count > 0:
        print("Actual excess:")
        actual_excess.show(3000, False)

    if expected_excess_count > 0:
        print("Expected excess:")
        expected_excess.show(3000, False)

    assert (
        actual_excess_count == 0 and expected_excess_count == 0
    ), "Dataframes data are not equal"


def _assert_no_duplicates(df: DataFrame) -> None:
    original_count = df.count()
    distinct_count = df.dropDuplicates().count()
    assert original_count == distinct_count, "The DataFrame contains duplicate rows"


def _show_duplicates(df: DataFrame) -> DataFrame:
    duplicates = (
        df.groupby(df.columns)
        .count()
        .where(f.col("count") > 1)
        .withColumnRenamed("count", "duplicate_count")
    )
    return duplicates


def assert_dataframe_and_schema(
    actual: DataFrame,
    expected: DataFrame,
    feature_tests_configuration: FeatureTestsConfiguration,
    ignore_nullability: bool = False,
    ignore_column_order: bool = False,
    ignore_decimal_scale: bool = False,
    ignore_decimal_precision: bool = False,
    columns_to_skip: list[str] = None,
) -> None:
    assert actual is not None, "Actual data frame is None"
    assert expected is not None, "Expected data frame is None"

    if feature_tests_configuration.show_actual_and_expected_count:
        print("\n")
        print(f"Number of rows in actual: {actual.count()}")
        print(f"Number of rows in expected: {expected.count()}")

    if columns_to_skip is not None and len(columns_to_skip) > 0:
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

    if feature_tests_configuration.show_actual_and_expected:
        print("ACTUAL:")
        actual.show(3000, False)
        print("EXPECTED:")
        expected.show(3000, False)

    if feature_tests_configuration.assert_no_duplicate_rows:
        try:
            _assert_no_duplicates(actual)
        except AssertionError:

            if (
                not feature_tests_configuration.show_columns_when_actual_and_expected_are_equal
            ):
                actual, expected = drop_columns_if_the_same(actual, expected)

            print("DUPLICATED ROWS IN ACTUAL:")
            _show_duplicates(actual).show(3000, False)
            raise

        try:
            _assert_no_duplicates(expected)
        except AssertionError:

            if (
                not feature_tests_configuration.show_columns_when_actual_and_expected_are_equal
            ):
                actual, expected = drop_columns_if_the_same(actual, expected)

            print("DUPLICATED ROWS IN EXPECTED:")
            _show_duplicates(expected).show(3000, False)
            raise

    try:
        assertDataFrameEqual(actual, expected)
    except AssertionError:

        if (
            not feature_tests_configuration.show_columns_when_actual_and_expected_are_equal
        ):
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
