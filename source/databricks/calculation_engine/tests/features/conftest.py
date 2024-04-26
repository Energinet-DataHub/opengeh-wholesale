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

import pytest
from _pytest.fixtures import FixtureRequest
from pyspark.sql import SparkSession

from features.utils.expected_output import ExpectedOutput
from features.utils.scenario_executor import ScenarioExecutor
from features.utils.views.dataframe_container import DataframeWrapper
from features.utils.views.view_scenario_executor import ViewScenarioExecutor
from package.calculation.calculation_results import CalculationResultsContainer


@pytest.fixture(scope="module")
def actual_and_expected(
    request: FixtureRequest,
    spark: SparkSession,
) -> tuple[CalculationResultsContainer, list[ExpectedOutput]]:
    """
    Provides the actual and expected output for a scenario test case.

    IMPORTANT: It is crucial that this fixture has scope=module, as the scenario executor
    is specific to a single scenario, which are each located in their own module.
    """

    scenario_path = str(Path(request.module.__file__).parent)
    scenario_executor = ScenarioExecutor(spark)
    return scenario_executor.execute(scenario_path)


@pytest.fixture(scope="module")
def actual_and_expected_views(
    migrations_executed: None,
    request: FixtureRequest,
    spark: SparkSession,
) -> tuple[list[DataframeWrapper], list[DataframeWrapper]]:
    """
    Provides the actual and expected output for a scenario test case.

    IMPORTANT: It is crucial that this fixture has scope=module, as the scenario executor
    is specific to a single scenario, which are each located in their own module.
    """

    scenario_path = str(Path(request.module.__file__).parent)
    executor = ViewScenarioExecutor(spark)
    return executor.execute(scenario_path)
