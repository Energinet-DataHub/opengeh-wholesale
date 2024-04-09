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
import inspect
from dataclasses import fields
from pathlib import Path
from typing import Any

from pyspark.sql import DataFrame

from features.utils.scenario_fixture2 import ScenarioFixture2, ExpectedResult
from helpers.data_frame_utils import assert_dataframe_and_schema
from package.calculation.calculation_results import CalculationResultsContainer
from package.constants import EnergyResultColumnNames


class Base:
    def _get_scenario_folder_path(self) -> str:
        """Retrieves the file path of the (most derived) class of the current object instance"""
        path_of_test_subclass = inspect.getfile(self.__class__)
        return str(Path(path_of_test_subclass).parent)

    def test_then_actual_equals_expected(
        self,
        scenario_fixture2: ScenarioFixture2,
    ) -> None:
        # Arrange
        scenario_folder_path = self._get_scenario_folder_path()

        # Act
        actual_results, expected_results = scenario_fixture2.execute(
            scenario_folder_path
        )

        # Assert
        for expected_result in expected_results:
            actual_result = get_actual_for_expected_result(
                actual_results, expected_result
            )

            assert_dataframe_and_schema(
                actual_result,
                expected_result.df,
                ignore_decimal_precision=True,
                ignore_nullability=True,
                columns_to_skip=[
                    EnergyResultColumnNames.calculation_result_id,
                ],
            )


def get_actual_for_expected_result(
    calculation_results_container: CalculationResultsContainer,
    expected_result: ExpectedResult,
) -> DataFrame:
    if has_field(calculation_results_container.energy_results, expected_result.name):
        return getattr(
            calculation_results_container.energy_results, expected_result.name
        )
    if has_field(calculation_results_container.wholesale_results, expected_result.name):
        return getattr(
            calculation_results_container.wholesale_results, expected_result.name
        )
    if has_field(calculation_results_container.basis_data, expected_result.name):
        return getattr(calculation_results_container.basis_data, expected_result.name)

    raise Exception(f"Unknown expected result name: {expected_result.name}")


def has_field(container_class: Any, field_name: str) -> bool:
    """Check if the given dataclass has a field with the specified name."""

    for field in fields(container_class):
        if field.name == field_name:
            return True
    return False
