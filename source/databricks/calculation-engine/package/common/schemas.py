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

from pyspark.sql.types import DecimalType, StructField, StructType


def assert_schema(
    actual: StructType,
    expected: StructType,
    ignore_nullability: bool = False,
    ignore_column_order: bool = False,
    ignore_decimal_scale: bool = False,
    ignore_decimal_precision: bool = False,
) -> None:
    """
    When actual schema does not match the expected schema,
    raises an AssertionError with an error message starting with 'Schema mismatch'.

    The function provides options to provide a more lenient comparison for either
    special cases or to allow a stepwise implementation of more strict checks.
    """
    if actual == expected:
        return

    strict = not (
        ignore_nullability
        or ignore_column_order
        or ignore_decimal_precision
        or ignore_decimal_scale
    )
    if strict:
        _raise(f"Expected {expected}, but got {actual}.")

    actual_fields = actual.fields
    expected_fields = expected.fields

    if ignore_column_order:
        actual_fields = sorted(actual_fields, key=lambda f: f.name)
        expected_fields = sorted(expected_fields, key=lambda f: f.name)

    for actual, expected in zip(actual_fields, expected_fields):
        _assert_column_name(actual, expected)
        _assert_column_nullability(actual, expected, ignore_nullability)
        _assert_column_datatype(
            actual, expected, ignore_decimal_precision, ignore_decimal_scale
        )


def _assert_column_nullability(
    actual: StructField, expected: StructField, ignore_nullability: bool
) -> None:
    if not ignore_nullability and actual.nullable != expected.nullable:
        _raise(
            f"Expected column name '{expected.name}' to have nullable={expected.dataType}, but got nullable={actual.dataType}"
        )


def _assert_column_name(actual: StructField, expected: StructField) -> None:
    if actual.name != expected.name:
        _raise(f"Expected column name '{expected.name}', but found '{actual.name}'")


def _assert_column_datatype(
    actual: StructField,
    expected: StructField,
    ignore_decimal_precision: bool,
    ignore_decimal_scale: bool,
) -> None:
    if actual.dataType == expected.dataType:
        return

    if not isinstance(actual.dataType, DecimalType) or not isinstance(
        expected.dataType, DecimalType
    ):
        _raise(
            f"Expected column name '{expected.name}' to have type {expected.dataType}, but got type {actual.dataType}"
        )

    if (
        not ignore_decimal_precision
        and actual.dataType.precision != expected.dataType.precision
    ):
        _raise(
            f"Decimal precision error: Expected column name '{expected.name}' to have type {expected.dataType}, but got type {actual.dataType}"
        )

    if not ignore_decimal_scale and actual.dataType.scale != expected.dataType.scale:
        _raise(
            f"Decimal scale error: Expected column name '{expected.name}' to have type {expected.dataType}, but got type {actual.dataType}"
        )


def _raise(error_message: str) -> None:
    raise AssertionError(f"Schema mismatch. {error_message}")
