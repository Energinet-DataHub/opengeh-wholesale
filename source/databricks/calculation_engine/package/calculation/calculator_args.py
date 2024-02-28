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

from dataclasses import dataclass
from datetime import datetime

from package.codelists.calculation_type import (
    CalculationType,
)


@dataclass
class CalculatorArgs:
    calculation_id: str
    calculation_grid_areas: list[str]
    calculation_period_start_datetime: datetime
    calculation_period_end_datetime: datetime
    calculation_type: CalculationType
    calculation_execution_time_start: datetime
    time_zone: str
