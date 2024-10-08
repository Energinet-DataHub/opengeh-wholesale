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
from dataclasses import fields
from typing import Any

from pyspark.sql import DataFrame

from helpers.data_frame_utils import assert_dataframe_and_schema
from package.calculation.calculation_output import CalculationOutput
from package.databases.table_column_names import TableColumnNames
from testsession_configuration import FeatureTestsConfiguration
from .expected_output import ExpectedOutput


def assert_output(
    actual_and_expected: tuple[CalculationOutput, list[ExpectedOutput]],
    output_name: str,
    feature_tests_configuration: FeatureTestsConfiguration,
) -> None:
    actual_results, expected_results = actual_and_expected

    actual_result = _get_actual_for_output(actual_results, output_name)
    expected_result = _get_expected_for_output(expected_results, output_name)

    columns_to_skip = []
    if TableColumnNames.calculation_result_id in expected_result.columns:
        columns_to_skip.append(TableColumnNames.calculation_result_id)
    if "result_id" in expected_result.columns:
        columns_to_skip.append("result_id")

    # Sort actual_result and expected_result
    actual_result = actual_result.sort(actual_result.columns)
    expected_result = expected_result.sort(expected_result.columns)

    assert_dataframe_and_schema(
        actual_result,
        expected_result,
        feature_tests_configuration,
        ignore_decimal_precision=True,
        ignore_nullability=True,
        ignore_decimal_scale=True,
        columns_to_skip=columns_to_skip,
    )


def _get_expected_for_output(
    expected_results: list[ExpectedOutput], output_name: str
) -> DataFrame:
    for expected_result in expected_results:
        if expected_result.name == output_name:
            return expected_result.df

    raise Exception(f"Unknown expected result name: {output_name}")


def _get_actual_for_output(
    calculation_output: CalculationOutput,
    expected_result_name: str,
) -> DataFrame:
    if _has_field(calculation_output.energy_results_output, expected_result_name):
        return getattr(calculation_output.energy_results_output, expected_result_name)
    if _has_field(calculation_output.wholesale_results_output, expected_result_name):
        return getattr(
            calculation_output.wholesale_results_output, expected_result_name
        )

    if _has_field(calculation_output.basis_data_output, expected_result_name):
        return getattr(calculation_output.basis_data_output, expected_result_name)

    raise Exception(f"Unknown expected result name: {expected_result_name}")


def _has_field(container_class: Any, field_name: str) -> bool:
    """Check if the given dataclass has a field with the specified name."""

    if container_class is None:
        return False

    for field in fields(container_class):
        if field.name == field_name:
            return True
    return False
