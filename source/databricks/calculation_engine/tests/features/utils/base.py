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
from pathlib import Path

from pyspark.sql import DataFrame

from features.utils.scenario_fixture2 import ScenarioFixture2, ExpectedResult
from helpers.data_frame_utils import assert_dataframe_and_schema
from package.calculation.calculation_results import CalculationResultsContainer
from package.constants import EnergyResultColumnNames


class Base:
    def _get_scenario_folder_path(self) -> str:
        # Retrieves the file path of the class of the current instance
        return str(Path(inspect.getfile(self.__class__)).parent)

    def test_then_actual_equals_expected(
        self,
        scenario_fixture2: ScenarioFixture2,
    ) -> None:
        scenario_fixture = scenario_fixture2
        # Arrange
        scenario_folder_path = self._get_scenario_folder_path()
        scenario_fixture.setup(scenario_folder_path)

        # Act
        actual_results = scenario_fixture.execute()
        expected_results = scenario_fixture.expected

        # Assert
        for expected_result in expected_results:
            actual_result = get_actual_for_expected_result(
                actual_results, expected_result
            )

            actual_result.show()
            expected_result.df.show()

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
    if expected_result.name == "flex_consumption_per_ga":
        return calculation_results_container.energy_results.flex_consumption_per_ga
    if expected_result.name == "flex_consumption_per_ga_es":
        return (
            calculation_results_container.energy_results.flex_consumption_per_ga_and_es
        )
    raise Exception(f"Unknown expected result name: {expected_result.name}")
