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
# Testing hourly tariff per grid area, charge owner and energy supplier with grid loss

    Calculation period: February 2023
    Grid area: 804

    charge_code charge_type charge_owner_id metering_point_id TYPE from_date to_date Energy supplier MP type
    40000 tariff 5790001330552 571313180400100657 E17 31-01-2023 23:00 8100000000115 Grid_loss
    40000 tariff 5790001330552 571313180480500149 E18 31-01-2023 23:00 8100000000108 System_CMP

    Positive_grid_los 1-20 February
    MP kWh MP id
    Production E18 90 571313180400012004
    Consumption E17 75 571313180400140417

    Calculated grid loss 15 Per time Pris: 0.756998 Amount: 11.35497

    Negative_grid_loss 20-28 February
    MP kWh MP id
    Production E18 80 571313180400012004
    Consumption E17 90 571313180400140417

    Calculated grid loss -10 Per time Pris0.756998 Amount: 7.56998

```gherkin
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
        results.wholesale_results.hourly_tariff_per_ga_co_es,
        scenario_fixture.expected,
        ignore_decimal_precision=True,
        ignore_nullability=True,
        columns_to_skip=[
            WholesaleResultColumnNames.calculation_result_id,
        ],
    )
