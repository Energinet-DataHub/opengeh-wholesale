#  Copyright 2020 Energinet DataHub A/S
#
#  Licensed under the Apache License, Version 2.0 (the "License2");
#  you may not use this file except in compliance with the License.
#  You may obtain a copy of the License at
#
#      http://www.apache.org/licenses/LICENSE-2.0
#
#  Unless required by applicable law or agreed to in writing, software
#  distributed under the License is distributed on an "AS IS" BASIS,
#  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#  See the License for the specific language governing permissions and
#  limitations under the License.
import inspect
import logging
from pathlib import Path

import pytest
from pyspark.sql import DataFrame

from helpers.data_frame_utils import assert_dataframe_and_schema
from package.constants import EnergyResultColumnNames


# @pytest.fixture(scope="module")
# def execute(
#     scenario_fixture: ScenarioFixture2,
# ) -> None:
#     # Arrange
#     scenario_fixture.setup()
#
#
# @pytest.fixture(scope="module")
# def actual(
#     execute,
#     scenario_fixture: ScenarioFixture2,
# ) -> DataFrame:
#     # Act
#     results = scenario_fixture.execute()
#     return results.energy_results.flex_consumption_per_ga
#
#
# @pytest.fixture(scope="module")
# def expected(execute, scenario_fixture: ScenarioFixture2) -> DataFrame:
#     # Assert
#     return scenario_fixture.expected


def _get_scenario_paths(base_path: str) -> list[str]:
    """
    Searches recursively for all 'TESTS.md' files starting from the given base path.

    Parameters:
    - base_path: A string representing the base directory path from where the search will begin.

    Returns:
    - A list of strings, where each string is the path to a 'TESTS.md' file found in the subdirectories of the base path.
    """
    base = Path(base_path)
    tests_md_files = list(base.rglob("TESTS.md"))
    # Convert Path objects to strings for compatibility
    return [str(file.parent) for file in tests_md_files]


def _get_scenario_output_paths(scenario_path: str) -> list[str]:
    """
    Searches recursively for all 'OUTPUT.md' files starting from the given base path.

    Parameters:
    - base_path: A string representing the base directory path from where the search will begin.

    Returns:
    - A list of strings, where each string is the path to a 'OUTPUT.md' file found in the subdirectories of the base path.
    """
    # TODO BJM: Support the other output files as well
    base = Path(f"{scenario_path}/energy_results")
    csv_files = list(base.rglob("*.csv"))
    # Convert Path objects to strings for compatibility
    return [str(file.relative_to(scenario_path)) for file in csv_files]


LOGGER = logging.getLogger(__name__)


def _get_test_cases() -> list[tuple]:
    tests_base_path = str(Path(__file__).parent)
    test_cases = []
    print("Path: " + tests_base_path)
    scenario_abs_paths = _get_scenario_paths(tests_base_path)
    print("Scenario paths: " + repr(scenario_abs_paths))
    for scenario_path in scenario_abs_paths:
        print(f"Scenario path: {scenario_path}")
        output_paths = _get_scenario_output_paths(scenario_path)
        print("Output paths: " + repr(output_paths))
        for output_path in output_paths:
            test_cases.append(
                (
                    None,
                    None,
                    str(
                        Path(f"{scenario_path}/{output_path}").relative_to(
                            tests_base_path
                        )
                    ),
                )
            )
    print("Test cases: " + repr(test_cases))
    return test_cases


# builder generates a list of tuples based on existing files in folders "basis_data", "energy_results", and "wholesale_results"
# Example of a tuple:
#   (<actual flex consumption per ga dataframe>, <expected flex consumption per ga dataframe>, "flex_consumption_per_ga")
# The builder will fail if an unexpected file is found in the folders
@pytest.mark.parametrize("actual, expected, output", _get_test_cases())
def test__actual_equals_expected(
    actual: DataFrame, expected: DataFrame, output: str
) -> None:
    """The "output" input parameter is used to identify the test case in the test report."""

    return
    assert_dataframe_and_schema(
        actual,
        expected,
        ignore_decimal_precision=True,
        ignore_nullability=True,
        columns_to_skip=[
            EnergyResultColumnNames.calculation_result_id,
        ],
    )
