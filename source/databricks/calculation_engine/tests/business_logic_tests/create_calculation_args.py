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
from datetime import datetime

import yaml

from package.calculation.calculator_args import CalculatorArgs
from package.codelists import CalculationType
from package.constants import Colname


class ArgsName:
    period_start = "period_start"
    period_end = "period_end"
    grid_areas = "grid_areas"


def create_calculation_args(test_path: str) -> CalculatorArgs:
    with open(test_path + "calculation_arguments.yml", "r") as file:
        calculation_args = yaml.safe_load(file)

    date_format = "%Y-%m-%d %H:%M:%S"

    return CalculatorArgs(
        calculation_id=str(uuid.uuid4()),
        calculation_grid_areas=calculation_args[0][ArgsName.grid_areas],
        calculation_period_start_datetime=datetime.strptime(
            calculation_args[0][ArgsName.period_start], date_format
        ),
        calculation_period_end_datetime=datetime.strptime(
            calculation_args[0][ArgsName.period_end], date_format
        ),
        calculation_type=CalculationType(calculation_args[0][Colname.calculation_type]),
        calculation_execution_time_start=datetime.strptime(
            calculation_args[0][Colname.calculation_execution_time_start],
            date_format,
        ),
        time_zone="Europe/Copenhagen",
    )
