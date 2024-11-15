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

from enum import Enum


class TaskType(Enum):
    """
    Databricks tasks that can be executed.
    """

    TimeSeriesHourly = 1
    TimeSeriesQuarterly = 2
    MeteringPointPeriods = 3
    ChargeLinks = 4
    ChargePricePoints = 5
    EnergyResults = 6
    WholesaleResults = 7
    MonthlyAmounts = 8
    Zip = 9
