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
from features.scenario_fixture import ScenarioFixture
from helpers.data_frame_utils import assert_dataframe_and_schema
from package.constants import WholesaleResultColumnNames
from .states.scenario_state import (
    get_expected,
)

"""
# Test Description

```gherkin
GIVEN two metering points (positive and negative grid loss)
  AND only one charge link 
  AND the charge link starts on February 27th
  AND the subscription price is 28.282828 DKK
WHEN calculating subscription amount per charge for February
THEN there is only result rows for 27th and 28th of february
  AND the subscription amount is 1.010101 DKK
```
"""


def test_execute__returns_expected(
    scenario_fixture: ScenarioFixture,
) -> None:
    # Arrange
    scenario_fixture.setup(get_expected)
    expected = scenario_fixture.expected.orderBy(WholesaleResultColumnNames.time)

    # Act
    results = scenario_fixture.execute()

    # Assert
    actual = results.wholesale_results.subscription_per_ga_co_es.orderBy(
        WholesaleResultColumnNames.time
    )

    assert_dataframe_and_schema(
        actual,
        expected,
        ignore_decimal_precision=True,
        ignore_nullability=True,
        columns_to_skip=[
            WholesaleResultColumnNames.calculation_result_id,
        ],
    )
