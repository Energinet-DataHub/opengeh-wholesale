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
import logging
from pathlib import Path

import pytest
from pyspark.sql import DataFrame, SparkSession

from helpers.data_frame_utils import assert_dataframe_and_schema
from package.constants import EnergyResultColumnNames

LOGGER = logging.getLogger(__name__)


def _get_scenario_paths(base_path: str) -> list[str]:
    """
    Get the path of all scenarios.
    A scenario is defined as a directory containing a 'TESTS.md' file.
    """
    base = Path(base_path)
    tests_md_files = list(base.rglob("TESTS.md"))
    return [str(file.parent) for file in tests_md_files]


def _get_scenario_output_paths(scenario_path: str) -> list[str]:
    base = Path(f"{scenario_path}/output")
    csv_files = list(base.rglob("*.csv"))
    return [str(file.relative_to(scenario_path)) for file in csv_files]


class ScenarioFixture:
    def __init__(self, spark: SparkSession, scenario_path: str):
        self.spark = spark
        self.scenario_path = scenario_path

    def actual(self, result_name: str) -> DataFrame:
        pass  # TODO BJM

    def expected(self, result_name: str) -> DataFrame:
        pass  # TODO BJM


def _get_test_cases() -> list[tuple]:
    tests_base_path = str(Path(__file__).parent)
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


@pytest.mark.parametrize("test_name, scenario_fixture", _get_test_cases())
def test__actual_equals_expected(
    spark: SparkSession,
    test_name: str,
    scenario_fixture: ScenarioFixture,
) -> None:
    """The "test_name" parameter is used to identify the test case in the test report."""

    return
    # assert_dataframe_and_schema(
    #     actual,
    #     expected,
    #     ignore_decimal_precision=True,
    #     ignore_nullability=True,
    #     columns_to_skip=[
    #         EnergyResultColumnNames.calculation_result_id,
    #     ],
    # )
