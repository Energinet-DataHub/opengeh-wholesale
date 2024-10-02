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
    grid_area_codes = "grid_areas"
    is_internal_calculation = "is_internal_calculation"


CSV_DATE_FORMAT = "%Y-%m-%d %H:%M:%S"


def create_calculation_args(input_path: str) -> CalculatorArgs:
    with open(input_path + "calculation_arguments.yml", "r") as file:
        calculation_args = yaml.safe_load(file)[0]

    quarterly_resolution_transition_datetime = datetime(2023, 1, 31, 23, 0, 0)
    if "quarterly_resolution_transition_datetime" in calculation_args:
        quarterly_resolution_transition_datetime = datetime.strptime(
            calculation_args["quarterly_resolution_transition_datetime"],
            CSV_DATE_FORMAT,
        )
    time_zone = "Europe/Copenhagen"
    if "time_zone" in calculation_args:
        time_zone = calculation_args["time_zone"]

    return CalculatorArgs(
        calculation_id=calculation_args[ArgsName.calculation_id],
        calculation_grid_areas=calculation_args[ArgsName.grid_area_codes],
        calculation_period_start_datetime=datetime.strptime(
            calculation_args[ArgsName.period_start], CSV_DATE_FORMAT
        ),
        calculation_period_end_datetime=datetime.strptime(
            calculation_args[ArgsName.period_end], CSV_DATE_FORMAT
        ),
        calculation_type=CalculationType(calculation_args[Colname.calculation_type]),
        calculation_execution_time_start=datetime.strptime(
            calculation_args[Colname.calculation_execution_time_start],
            CSV_DATE_FORMAT,
        ),
        created_by_user_id=calculation_args[Colname.created_by_user_id],
        time_zone=time_zone,
        quarterly_resolution_transition_datetime=quarterly_resolution_transition_datetime,
        is_internal_calculation=calculation_args.get(
            ArgsName.is_internal_calculation, False
        ),
    )
