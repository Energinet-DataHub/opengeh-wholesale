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
from pyspark.sql import DataFrame


@dataclass
class EnergyResults:
    pass


@dataclass
class WholesaleResults:
    pass


@dataclass
class BasisData:
    metering_point_periods: DataFrame = None
    metering_point_time_series: DataFrame = None


@dataclass
class CalculationResults:
    energy_results: EnergyResults = EnergyResults()
    wholesale_results: WholesaleResults = WholesaleResults()
    basis_data: BasisData = BasisData()
