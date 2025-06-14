import pyspark.sql.functions as f
from geh_common.testing.dataframes.assert_schemas import assert_schema
from pyspark.sql import DataFrame

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
    actual_excess = actual.exceptAll(expected)
    expected_excess = expected.exceptAll(actual)

    # do the count once here to avoid materializing multiple times
    actual_excess_count = actual_excess.count()
    expected_excess_count = expected_excess.count()

    if actual_excess_count > 0:
        print("Actual excess:")  # noqa: T201
        actual_excess.show(3000, False)

    if expected_excess_count > 0:
        print("Expected excess:")  # noqa: T201
        expected_excess.show(3000, False)

    assert actual_excess_count == 0 and expected_excess_count == 0, "Dataframes data are not equal"  # noqa: PT018


def _assert_no_duplicates(df: DataFrame) -> None:
    original_count = df.count()
    distinct_count = df.dropDuplicates().count()
    assert original_count == distinct_count, "The DataFrame contains duplicate rows"


def _show_duplicates(df: DataFrame) -> DataFrame:
    duplicates = df.groupby(df.columns).count().where(f.col("count") > 1).withColumnRenamed("count", "duplicate_count")
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
        print("\n")  # noqa: T201
        print(f"Number of rows in actual: {actual.count()}")  # noqa: T201
        print(f"Number of rows in expected: {expected.count()}")  # noqa: T201

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
        print("SCHEMA MISMATCH:")  # noqa: T201
        print("ACTUAL SCHEMA:")  # noqa: T201
        actual.printSchema()
        print("EXPECTED SCHEMA:")  # noqa: T201
        expected.printSchema()
        raise

    if feature_tests_configuration.show_actual_and_expected:
        print("ACTUAL:")  # noqa: T201
        actual.show(3000, False)
        print("EXPECTED:")  # noqa: T201
        expected.show(3000, False)

    if feature_tests_configuration.assert_no_duplicate_rows:
        try:
            _assert_no_duplicates(actual)
        except AssertionError:
            if not feature_tests_configuration.show_columns_when_actual_and_expected_are_equal:
                actual, expected = drop_columns_if_the_same(actual, expected)

            print("DUPLICATED ROWS IN ACTUAL:")  # noqa: T201
            _show_duplicates(actual).show(3000, False)
            raise

        try:
            _assert_no_duplicates(expected)
        except AssertionError:
            if not feature_tests_configuration.show_columns_when_actual_and_expected_are_equal:
                actual, expected = drop_columns_if_the_same(actual, expected)

            print("DUPLICATED ROWS IN EXPECTED:")  # noqa: T201
            _show_duplicates(expected).show(3000, False)
            raise

    try:
        assert_dataframes_equal(actual, expected)
    except AssertionError:
        if not feature_tests_configuration.show_columns_when_actual_and_expected_are_equal:
            actual, expected = drop_columns_if_the_same(actual, expected)

        print("DATA MISMATCH:")  # noqa: T201
        print("IN ACTUAL BUT NOT IN EXPECTED:")  # noqa: T201
        actual.subtract(expected).show(3000, False)
        print("IN EXPECTED BUT NOT IN ACTUAL:")  # noqa: T201
        expected.subtract(actual).show(3000, False)
        raise


def drop_columns_if_the_same(df1: DataFrame, df2: DataFrame) -> tuple[DataFrame, DataFrame]:
    column_names = df1.columns
    for column_name in column_names:
        df1_column = df1.select(column_name).collect()
        df2_column = df2.select(column_name).collect()

        if df1_column == df2_column:
            df1 = df1.drop(column_name)
            df2 = df2.drop(column_name)

    return df1, df2
