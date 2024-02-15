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

from datetime import datetime

import pytest
from azure.identity import ClientSecretCredential

from package.calculation.calculator_args import CalculatorArgs
from package.codelists.calculation_type import (
    CalculationType,
)


@pytest.fixture(scope="session")
def any_calculator_args() -> CalculatorArgs:
    return CalculatorArgs(
        data_storage_account_name="foo",
        data_storage_account_credentials=ClientSecretCredential("foo", "foo", "foo"),
        wholesale_container_path="foo",
        calculation_input_path="foo",
        time_series_points_table_name=None,
        metering_point_periods_table_name=None,
        grid_loss_metering_points_table_name=None,
        calculation_id="0b15a420-9fc8-409a-a169-fbd49479d718",
        calculation_type=CalculationType.BALANCE_FIXING,
        calculation_grid_areas=["805", "806"],
        calculation_period_start_datetime=datetime(2018, 1, 1, 23, 0, 0),
        calculation_period_end_datetime=datetime(2018, 1, 3, 23, 0, 0),
        calculation_execution_time_start=datetime(2018, 1, 5, 23, 0, 0),
        time_zone="Europe/Copenhagen",
    )
