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

import pytest
from pyspark.sql import DataFrame

from helpers.data_frame_utils import assert_dataframe_and_schema


def get_test_cases() -> list[tuple]:
    tests_base_path = str(Path(__file__).parent.parent)
    test_cases = []
    print("Path: " + tests_base_path)
    scenario_abs_paths = _get_scenario_paths(tests_base_path)
    print("Scenario paths: " + repr(scenario_abs_paths))
    for scenario_path in scenario_abs_paths:
        print(f"Scenario path: {scenario_path}")

        scenario_fixture = ScenarioFixture(SPARK, scenario_path)

        output_paths = _get_scenario_output_paths(scenario_path)
        print("Output paths: " + repr(output_paths))
        for output_path in output_paths:
            test_cases.append(
                (
                    str(
                        Path(f"{scenario_path}/{output_path}").relative_to(
                            tests_base_path
                        )
                    ),
                    scenario_fixture,
                )
            )
    print("Test cases: " + repr(test_cases))
    return test_cases


# builder generates a list of tuples based on existing files in folders "basis_data", "energy_results", and "wholesale_results"
# Example of a tuple:
#   (<actual flex consumption per ga dataframe>, <expected flex consumption per ga dataframe>, "flex_consumption_per_ga")
# The builder will fail if an unexpected file is found in the folders
class Base:
    def _get_scenario_folder_path(self) -> str:
        # Retrieves the file path of the class of the current instance
        return str(Path(inspect.getfile(self.__class__)).parent)

    @pytest.mark.parametrize("actual, expected, test_name", get_test_cases())
    def test_then_actual_equals_expected(
        self, actual: DataFrame, expected: DataFrame, test_name: str
    ) -> None:
        for csv in csvs:
            assert_dataframe_and_schema(
                actual,
                expected,
            )
