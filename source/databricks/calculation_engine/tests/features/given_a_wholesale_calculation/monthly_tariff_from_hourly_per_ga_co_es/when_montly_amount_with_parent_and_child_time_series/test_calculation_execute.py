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
GIVEN two grid loss metering points (positive and negative grid loss)
  AND one parent metering point (consumption)  
  AND one child metering point (consumption from grid)
  AND the charge link quantity of 10 for child metering point starting on February 2nd
  AND the charge link quantity of 20 for for parent metering point starting February 2nd
  AND the charge price is 0.756998 DKK per hour
WHEN calculating monthly amount for hourly tariff for February
THEN the amount is 15261.07968 DKK
```

```text
The amount calculated like this:
charge price * quantity * 24 hour * days in month = amount for metering point
Child metering point: 0.756998 * 10 * 24 * 28 = 5087.02656
Parent metering point: 0.756998 * 20 * 24 * 28 = 10174.05312
5087.02656 + 10174.05312 = 15261.07968
```
"""


def test_execute__returns_expected(
    scenario_fixture: ScenarioFixture,
) -> None:
    # Arrange
    scenario_fixture.setup(get_expected)

    # Act
    results = scenario_fixture.execute()

    # Assert
    assert_dataframe_and_schema(
        results.wholesale_results.monthly_tariff_from_hourly_per_ga_co_es,
        scenario_fixture.expected,
        ignore_decimal_precision=True,
        ignore_nullability=True,
        columns_to_skip=[
            WholesaleResultColumnNames.calculation_result_id,
        ],
    )
