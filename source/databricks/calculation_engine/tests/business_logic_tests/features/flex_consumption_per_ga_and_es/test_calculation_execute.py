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

from business_logic_tests.features.flex_consumption_per_ga_and_es.states.state import (
    get_result,
)
from business_logic_tests.scenario_factory import ScenarioFixture
from helpers.data_frame_utils import (
    assert_dataframes,
)


def test_execute__returns_expected(
    scenario_fixture: ScenarioFixture,
) -> None:
    # Arrange
    scenario_fixture.setup(get_result)

    # Act
    results = scenario_fixture.execute()

    # Assert
    assert_dataframes(
        results.energy_results.flex_consumption_per_ga_and_es.df,
        scenario_fixture.expected,
        ignore_schema=True,
        ignore_nullability=True,
        ignore_decimal_precision=True,
    )
