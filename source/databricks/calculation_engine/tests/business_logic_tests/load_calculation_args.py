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
import uuid

import pandas as pd
import yaml

from package.calculation.calculator_args import CalculatorArgs
from package.codelists import CalculationType
from package.constants import Colname


def load_calculation_args(test_path: str) -> CalculatorArgs:
    with open(test_path + "calculation_arguments.yml", "r") as file:
        calculation_args = yaml.safe_load(file)

    date_format = "%Y-%m-%d %H:%M:%S"

    return CalculatorArgs(
        calculation_id=str(uuid.uuid4()),
        calculation_grid_areas=calculation_args[0]["grid_areas"],  # TODO const?
        calculation_period_start_datetime=pd.to_datetime(
            calculation_args[0]["period_start"], format=date_format
        ),
        calculation_period_end_datetime=pd.to_datetime(
            calculation_args[0]["period_end"], format=date_format
        ),
        calculation_type=CalculationType(calculation_args[0][Colname.calculation_type]),
        calculation_execution_time_start=pd.to_datetime(
            calculation_args[0][Colname.calculation_execution_time_start],
            format=date_format,
        ),
        time_zone="Europe/Copenhagen",
    )
