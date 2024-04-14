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
from pathlib import Path
from typing import Any

import pytest
from pyspark.sql import SparkSession

from features.utils.assertion import assert_output
from features.utils.expected_output import ExpectedOutput
from features.utils.scenario_executor import ScenarioExecutor
from package.calculation.calculation_results import CalculationResultsContainer


def get_output_names() -> list[str]:
    output_folder_path = Path(__file__).parent / "output"
    csv_files = list(output_folder_path.rglob("*.csv"))
    return [Path(file).stem for file in csv_files]


@pytest.fixture(scope="module")
def actual_and_expected(
    spark: SparkSession,
) -> tuple[CalculationResultsContainer, list[ExpectedOutput]]:
    """
    Provides the actual and expected output for a scenario test case.

    IMPORTANT: It is crucial that this fixture has scope=module, as the scenario executor
    is specific to a single scenario, which are each located in their own module.
    """

    scenario_path = str(Path(__file__).parent)
    scenario_executor = ScenarioExecutor(spark)
    return scenario_executor.execute(scenario_path)


# IMPORTANT:
# All test files should be identical. This makes changing them cumbersome.
# So in order to make it easier you can modify the utils/template.py file instead,
# and then run the power-shell script "Use-Template.ps1" to update all test_output.py files.
@pytest.mark.parametrize("output_name", get_output_names())
def test__equals_expected(
    actual_and_expected: Any,
    output_name: str,
) -> None:
    assert_output(actual_and_expected, output_name)
