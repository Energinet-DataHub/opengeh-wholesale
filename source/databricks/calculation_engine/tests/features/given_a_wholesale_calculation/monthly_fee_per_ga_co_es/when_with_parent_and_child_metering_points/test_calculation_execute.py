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
GIVEN a flex_consumption metering point for energy supplier 8100000000108 with two fee-1 links
  AND another flex_consumption metering points for energy supplier 8100000000100 with 10 fee-1 links
  AND a child consumption_from_grid with one fee-1 link
  AND the fee-1 price is 70 DDK
WHEN calculating fee
THEN there are 2 rows in the result
  AND the amount for fee for energy supplier 8100000000100 is 700 DDK
  AND the amount for fee for energy supplier 8100000000108 is 210 DDK
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
        results.wholesale_results.monthly_fee_per_ga_co_es,
        scenario_fixture.expected,
        ignore_decimal_precision=True,
        ignore_nullability=True,
        columns_to_skip=[
            WholesaleResultColumnNames.calculation_result_id,
        ],
    )
