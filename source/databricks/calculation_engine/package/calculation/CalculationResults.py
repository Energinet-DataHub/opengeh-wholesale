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

from package.calculation.energy.energy_results import EnergyResults


# TODO BJM: Can we remove the from the dataclass fields? (in another PR)
@dataclass
class EnergyResultsContainer:
    exchange_per_neighbour_ga: EnergyResults | None = None
    exchange_per_grid_area: EnergyResults | None = None
    temporary_production_per_ga: EnergyResults | None = None
    temporary_flex_consumption_per_ga: EnergyResults | None = None
    grid_loss: EnergyResults | None = None
    positive_grid_loss: EnergyResults | None = None
    negative_grid_loss: EnergyResults | None = None
    consumption_per_ga_and_brp: EnergyResults | None = None
    consumption_per_ga_and_brp_and_es: EnergyResults | None = None
    consumption_per_ga_and_es: EnergyResults | None = None
    consumption_per_ga: EnergyResults | None = None
    production_per_ga_and_brp_and_es: EnergyResults | None = None
    production_per_ga_and_brp: EnergyResults | None = None
    production_per_ga_and_es: EnergyResults | None = None
    production_per_ga: EnergyResults | None = None
    flex_consumption_per_ga: EnergyResults | None = None
    flex_consumption_per_ga_and_es: EnergyResults | None = None
    flex_consumption_per_ga_and_brp_and_es: EnergyResults | None = None
    flex_consumption_per_ga_and_brp: EnergyResults | None = None
    total_consumption: EnergyResults | None = None


@dataclass
class WholesaleResultsContainer:
    pass


@dataclass
class BasisDataContainer:
    metering_point_periods: DataFrame | None = None
    metering_point_time_series: DataFrame | None = None


@dataclass
class CalculationResultsContainer:
    energy_results: EnergyResultsContainer | None = None
    wholesale_results: WholesaleResultsContainer | None = None
    basis_data: BasisDataContainer = BasisDataContainer()
