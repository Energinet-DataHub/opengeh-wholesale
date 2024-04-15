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
from package.calculation.calculation_results import CalculationResultsContainer
from package.constants.result_column_names import ResultColumnNames
from .expected_output import ExpectedOutput


def assert_output(
    actual_and_expected: tuple[CalculationResultsContainer, list[ExpectedOutput]],
    output_name: str,
) -> None:
    actual_results, expected_results = actual_and_expected

    actual_result = _get_actual_for_output(actual_results, output_name)
    expected_result = _get_expected_for_output(expected_results, output_name)

    columns_to_skip = (
        [ResultColumnNames.calculation_result_id]
        if ResultColumnNames.calculation_result_id in expected_result.columns
        else []
    )

    assert_dataframe_and_schema(
        actual_result,
        expected_result,
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
    calculation_results_container: CalculationResultsContainer,
    expected_result_name: str,
) -> DataFrame:
    if _has_field(calculation_results_container.energy_results, expected_result_name):
        return getattr(
            calculation_results_container.energy_results, expected_result_name
        )
    if _has_field(
        calculation_results_container.wholesale_results, expected_result_name
    ):
        return getattr(
            calculation_results_container.wholesale_results, expected_result_name
        )
    if _has_field(
        calculation_results_container.total_monthly_amounts, expected_result_name
    ):
        return getattr(
            calculation_results_container.total_monthly_amounts, expected_result_name
        )

    if _has_field(calculation_results_container.basis_data, expected_result_name):
        return getattr(calculation_results_container.basis_data, expected_result_name)

    raise Exception(f"Unknown expected result name: {expected_result_name}")


def _has_field(container_class: Any, field_name: str) -> bool:
    """Check if the given dataclass has a field with the specified name."""

    if container_class is None:
        return False

    for field in fields(container_class):
        if field.name == field_name:
            return True
    return False
