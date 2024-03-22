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

import yaml

from package.calculation.calculator_args import CalculatorArgs
from package.codelists import CalculationType
from package.constants import Colname


class ArgsName:
    calculation_id = "calculation_id"
    period_start = "period_start"
    period_end = "period_end"
    grid_areas = "grid_areas"


CSV_DATE_FORMAT = "%Y-%m-%d %H:%M:%S"


def create_calculation_args(test_path: str) -> CalculatorArgs:
    with open(test_path + "calculation_arguments.yml", "r") as file:
        calculation_args = yaml.safe_load(file)

    return CalculatorArgs(
        calculation_id=calculation_args[0][ArgsName.calculation_id],
        calculation_grid_areas=calculation_args[0][ArgsName.grid_areas],
        calculation_period_start_datetime=datetime.strptime(
            calculation_args[0][ArgsName.period_start], CSV_DATE_FORMAT
        ),
        calculation_period_end_datetime=datetime.strptime(
            calculation_args[0][ArgsName.period_end], CSV_DATE_FORMAT
        ),
        calculation_type=CalculationType(calculation_args[0][Colname.calculation_type]),
        calculation_execution_time_start=datetime.strptime(
            calculation_args[0][Colname.calculation_execution_time_start],
            CSV_DATE_FORMAT,
        ),
        time_zone="Europe/Copenhagen",
    )


def test_foo():
    pass
