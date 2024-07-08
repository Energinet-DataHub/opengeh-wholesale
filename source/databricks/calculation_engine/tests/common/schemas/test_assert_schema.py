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

import pyspark.sql.types as t
import pytest

from package.common import assert_schema

reference_schema = t.StructType(
    [
        t.StructField("foo", t.StringType(), False),
        t.StructField("bar", t.IntegerType(), False),
        t.StructField("array", t.ArrayType(t.StringType(), True), True),
    ]
)
schema_with_missing_column = t.StructType(
    [
        t.StructField("bar", t.IntegerType(), False),
        t.StructField("array", t.ArrayType(t.StringType(), True), True),
    ]
)
schema_with_other_column_order = t.StructType(
    [
        t.StructField("bar", t.IntegerType(), False),
        t.StructField("foo", t.StringType(), False),
        t.StructField("array", t.ArrayType(t.StringType(), True), True),
    ]
)
schema_with_other_nullability = t.StructType(
    [
        t.StructField("foo", t.StringType(), False),
        # Other nullability
        t.StructField("bar", t.IntegerType(), True),
        # Other nullability in deeper levels
        t.StructField("array", t.ArrayType(t.StringType(), False), False),
    ]
)
schema_with_other_column_order_and_nullability = t.StructType(
    [
        t.StructField("bar", t.IntegerType(), False),
        t.StructField("foo", t.StringType(), True),
        t.StructField("array", t.ArrayType(t.StringType(), True), True),
    ]
)
schema_with_other_datatype = t.StructType(
    [
        t.StructField("foo", t.StringType(), False),
        t.StructField("bar", t.DecimalType(), False),
        t.StructField("array", t.ArrayType(t.StringType(), True), True),
    ]
)
schema_with_more_columns = t.StructType(
    [
        t.StructField("foo", t.StringType(), False),
        t.StructField("bar", t.IntegerType(), False),
        t.StructField("array", t.ArrayType(t.StringType(), True), True),
        t.StructField("another-field", t.StringType(), True),
    ]
)
schema_with_more_columns_and_different_column_order = t.StructType(
    [
        t.StructField("bar", t.IntegerType(), False),
        t.StructField("another-field", t.StringType(), True),
        t.StructField("foo", t.StringType(), False),
        t.StructField("array", t.ArrayType(t.StringType(), True), True),
    ]
)


@pytest.mark.parametrize(
    "actual, expected, ignore_column_order, ignore_nullability",
    [
        (reference_schema, reference_schema, False, False),
        (reference_schema, reference_schema, False, True),
        (reference_schema, reference_schema, True, False),
        (reference_schema, reference_schema, True, True),
        (reference_schema, schema_with_other_column_order, True, False),
        (reference_schema, schema_with_other_column_order, True, True),
        (reference_schema, schema_with_other_nullability, False, True),
        (reference_schema, schema_with_other_nullability, True, True),
        (reference_schema, schema_with_other_column_order_and_nullability, True, True),
    ],
)
def test__accepts_matching_schema(
    actual: t.StructType,
    expected: t.StructType,
    ignore_column_order: bool,
    ignore_nullability: bool,
) -> None:
    # Assert no error is raised
    assert_schema(
        actual,
        expected,
        ignore_column_order=ignore_column_order,
        ignore_nullability=ignore_nullability,
    )


@pytest.mark.parametrize(
    "actual, expected, ignore_column_order, ignore_nullability",
    [
        (schema_with_missing_column, reference_schema, True, True),
        (reference_schema, schema_with_other_column_order, False, False),
        (reference_schema, schema_with_other_column_order, False, True),
        (reference_schema, schema_with_other_nullability, False, False),
        (reference_schema, schema_with_other_nullability, True, False),
        (
            reference_schema,
            schema_with_other_column_order_and_nullability,
            False,
            False,
        ),
        (reference_schema, schema_with_other_column_order_and_nullability, False, True),
        (reference_schema, schema_with_other_column_order_and_nullability, True, False),
    ],
)
def test__when_schema_does_not_match__raises(
    actual: t.StructType,
    expected: t.StructType,
    ignore_column_order: bool,
    ignore_nullability: bool,
) -> None:
    with pytest.raises(AssertionError):
        assert_schema(
            actual,
            expected,
            ignore_column_order=ignore_column_order,
            ignore_nullability=ignore_nullability,
        )


def test__when_lenient_and_other_datatype__raises_assertion_error() -> None:
    """Lenient refers to being as loose as possible in the check."""
    with pytest.raises(AssertionError):
        assert_schema(
            reference_schema,
            schema_with_other_datatype,
            ignore_nullability=True,
            ignore_column_order=True,
            ignore_decimal_scale=True,
            ignore_decimal_precision=True,
            ignore_extra_actual_columns=True,
        )


@pytest.mark.parametrize(
    "expected_decimal, ignore_precision, ignore_scale",
    [
        (t.DecimalType(17, 2), False, False),
        (t.DecimalType(17, 3), False, False),
        (t.DecimalType(18, 2), False, False),
        (t.DecimalType(17, 3), False, True),
        (t.DecimalType(18, 2), True, False),
    ],
)
def test__when_invalid_decimal_type__raises_assertion_error(
    expected_decimal: t.DecimalType,
    ignore_precision: bool,
    ignore_scale: bool,
) -> None:
    actual = t.StructType([t.StructField("d", t.DecimalType(18, 3), False)])
    expected = t.StructType([t.StructField("d", expected_decimal, False)])

    with pytest.raises(AssertionError):
        assert_schema(
            actual,
            expected,
            ignore_decimal_scale=ignore_scale,
            ignore_decimal_precision=ignore_precision,
        )


@pytest.mark.parametrize(
    "expected_decimal, ignore_precision, ignore_scale",
    [
        (t.DecimalType(17, 2), True, True),
        (t.DecimalType(18, 3), False, False),
        (t.DecimalType(17, 3), True, False),
        (t.DecimalType(18, 2), False, True),
    ],
)
def test__when_decimal_type_should_be_accepted__does_not_raise(
    expected_decimal: t.DecimalType,
    ignore_precision: bool,
    ignore_scale: bool,
) -> None:
    actual = t.StructType([t.StructField("d", t.DecimalType(18, 3), False)])
    expected = t.StructType([t.StructField("d", expected_decimal, False)])

    # Act and assert (implicitly asserts that no error is raised)
    assert_schema(
        actual,
        expected,
        ignore_decimal_scale=ignore_scale,
        ignore_decimal_precision=ignore_precision,
    )


def test__when_more_actual_columns_should_be_rejected__raises_assertion_error() -> None:
    with pytest.raises(AssertionError):
        assert_schema(
            schema_with_more_columns,
            reference_schema,
            ignore_extra_actual_columns=False,
        )


def test__when_different_column_order_and_more_actual_columns_should_be_rejected__does_not_raise() -> (
    None
):
    """
    Test name is leaving out the fact that column ordering is ignored as well.
    Otherwise, the name is too long.
    """
    assert_schema(
        schema_with_more_columns_and_different_column_order,
        reference_schema,
        ignore_column_order=True,
        ignore_extra_actual_columns=True,
    )


def test__when_more_actual_columns_should_be_accepted__does_not_raise() -> None:
    assert_schema(
        schema_with_more_columns,
        reference_schema,
        ignore_extra_actual_columns=True,
    )


def test__when_more_actual_columns_should_be_rejected_without_ignore_extra_columns__raises_assertion_error() -> (
    None
):
    """
    Test name is leaving out the fact that column ordering is ignored as well.
    Otherwise, the strict assertion will fail first.
    """
    with pytest.raises(AssertionError):
        assert_schema(
            schema_with_more_columns,
            reference_schema,
            ignore_column_order=True,
            ignore_extra_actual_columns=False,
        )
