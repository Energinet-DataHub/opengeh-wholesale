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

from pyspark.sql.types import DecimalType, StructField, StructType, ArrayType, DataType


def assert_contract(actual_schema: StructType, contract: StructType) -> None:
    """
    Asserts that the actual schema matches the contract of the data product data contract.
    Non-breaking changes are allowed, such as adding new columns or changing column ordering.
    """
    try:
        assert_schema(
            actual_schema,
            contract,
            ignore_extra_actual_columns=True,
            ignore_column_order=True,
            # Consider: Contract changes from nullable=False to nullable=True should be considered a breaking change
            ignore_nullability=True,
        )
    except AssertionError as e:
        raise AssertionError(
            f"""The data source does not comply with the contract.
            Were breaking changes made without a new major version?
            Is the contract correct? Details: {str(e)}"""
        )


def assert_schema(
    actual: StructType,
    expected: StructType,
    ignore_nullability: bool = False,
    ignore_column_order: bool = False,
    ignore_decimal_scale: bool = False,
    ignore_decimal_precision: bool = False,
    ignore_extra_actual_columns: bool = False,
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
        or ignore_extra_actual_columns
    )
    if strict:
        _raise(f"Expected {expected}, but got {actual}.")

    actual_fields = actual.fields
    expected_fields = expected.fields

    if ignore_extra_actual_columns:
        expected_field_names = set(field.name for field in expected_fields)
        actual_fields = [f for f in actual if f.name in expected_field_names]

    if ignore_column_order:
        actual_fields = sorted(actual_fields, key=lambda f: f.name)
        expected_fields = sorted(expected_fields, key=lambda f: f.name)

    if len(actual_fields) < len(expected_fields):
        _raise(
            f"""Actual schema has fewer fields than expected schema.
            Expected field names: {expected.fieldNames()}.
            Actual field names: {actual.fieldNames()}."""
        )

    if len(actual_fields) > len(expected_fields):
        _raise(
            f"""Actual schema has more fields than expected schema.
             Expected field names: {expected.fieldNames()}.
             Actual field names: {actual.fieldNames()}."""
        )

    for actual_field, expected_field in zip(actual_fields, expected_fields):
        _assert_column_name(actual_field, expected_field)
        _assert_field(
            actual_field,
            expected_field,
            ignore_decimal_precision,
            ignore_decimal_scale,
            ignore_nullability,
        )


def _assert_field(
    actual: StructField,
    expected: StructField,
    ignore_decimal_precision: bool,
    ignore_decimal_scale: bool,
    ignore_nullability: bool,
) -> None:
    if not ignore_nullability:
        _assert_struct_field_nullability(actual, expected)
    _assert_data_type(
        actual.dataType,
        expected.dataType,
        expected.name,
        ignore_decimal_precision,
        ignore_decimal_scale,
    )


def _assert_struct_field_nullability(
    actual: StructField, expected: StructField
) -> None:
    if actual.nullable != expected.nullable:
        _raise(
            f"Expected column name '{expected.name}' to have nullable={expected.nullable}, but got nullable={actual.nullable}"
        )

    _assert_data_type_nullability(actual.dataType, expected.dataType)


def _assert_data_type_nullability(actual: DataType, expected: DataType) -> None:
    """Recursively asserts that nullability of array type elements matches."""
    if not isinstance(actual, ArrayType) or not isinstance(expected, ArrayType):
        return

    if actual.containsNull != expected.containsNull:
        _raise(
            f"Expected array with element type '{expected.elementType}' to have nullable={expected.containsNull}, but got nullable={actual.containsNull}"
        )

    _assert_data_type_nullability(actual.elementType, expected.elementType)


def _assert_column_name(actual: StructField, expected: StructField) -> None:
    if actual.name != expected.name:
        _raise(f"Expected column name '{expected.name}', but found '{actual.name}'")


def _assert_data_type(
    actual: DataType,
    expected: DataType,
    column_name: str,
    ignore_decimal_precision: bool,
    ignore_decimal_scale: bool,
) -> None:
    if actual == expected:
        return

    if isinstance(actual, ArrayType) and isinstance(expected, ArrayType):
        _assert_data_type(
            actual.elementType,
            expected.elementType,
            column_name,
            ignore_decimal_precision,
            ignore_decimal_scale,
        )
        return

    if isinstance(actual, StructType) and isinstance(expected, StructType):
        for i, field in enumerate(actual):
            _assert_data_type(
                field.dataType,
                expected.fields[i].dataType,
                field.name,
                ignore_decimal_precision,
                ignore_decimal_scale,
            )
        return

    if not isinstance(actual, DecimalType) or not isinstance(expected, DecimalType):
        _raise(
            f"Expected column name '{column_name}' to have type {expected}, but got type {actual}"
        )

    if not ignore_decimal_precision and actual.precision != expected.precision:
        _raise(
            f"Decimal precision error: Expected column name '{column_name}' to have type {expected}, but got type {actual}"
        )

    if not ignore_decimal_scale and actual.scale != expected.scale:
        _raise(
            f"Decimal scale error: Expected column name '{column_name}' to have type {expected}, but got type {actual}"
        )


def _raise(error_message: str) -> None:
    raise AssertionError(f"Schema mismatch. {error_message}")
