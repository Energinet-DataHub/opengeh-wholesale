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
    if actual == expected:
        return

    strict = not (
        ignore_nullability
        or ignore_column_order
        or ignore_decimal_precision
        or ignore_decimal_scale
    )
    if strict:
        if actual != expected:
            raise AssertionError(
                f"Schema mismatch. Expected {expected}, but got {actual}."
            )

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
        raise AssertionError(
            f"Expected column name '{expected.name}' to have nullable={expected.dataType}, but got nullable={actual.dataType}"
        )


def _assert_column_name(actual: StructField, expected: StructField) -> None:
    if actual.name != expected.name:
        raise AssertionError(
            f"Expected column name '{expected.name}', but found '{actual.name}'"
        )


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
        raise AssertionError(
            f"Expected column name '{expected.name}' to have type {expected.dataType}, but got type {actual.dataType}"
        )

    if (
        not ignore_decimal_precision
        and actual.dataType.precision != expected.dataType.precision
    ):
        raise AssertionError(
            f"Decimal precision error: Expected column name '{expected.name}' to have type {expected.dataType}, but got type {actual.dataType}"
        )

    if not ignore_decimal_scale and actual.dataType.scale != expected.dataType.scale:
        raise AssertionError(
            f"Decimal scale error: Expected column name '{expected.name}' to have type {expected.dataType}, but got type {actual.dataType}"
        )
