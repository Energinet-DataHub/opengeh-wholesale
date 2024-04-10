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

from features.scenario_fixture import ScenarioFixture
from features.utils.scenario_fixture2 import ScenarioFixture2, ExpectedResult
from package.calculation.calculation_results import CalculationResultsContainer


@pytest.fixture(scope="session")
def scenario_fixture(
    spark: SparkSession,
) -> ScenarioFixture:
    return ScenarioFixture(spark)


@pytest.fixture(scope="session")
def scenario_fixture2(
    spark: SparkSession,
) -> ScenarioFixture2:
    return ScenarioFixture2(spark)


@pytest.fixture(scope="module")
def actual_and_expected(
    request: FixtureRequest,
    scenario_fixture2: ScenarioFixture2,
) -> tuple[CalculationResultsContainer, list[ExpectedResult]]:
    scenario_path = str(Path(request.module.__file__).parent)
    return scenario_fixture2.execute(scenario_path)
