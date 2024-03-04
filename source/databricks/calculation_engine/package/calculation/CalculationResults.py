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
from typing import cast

from pyspark.sql import DataFrame


@dataclass
class EnergyResultsContainer:
    net_exchange_per_neighbour_ga: DataFrame | None = None
    net_exchange_per_ga: DataFrame | None = None
    temporary_production_per_ga: DataFrame | None = None
    temporary_flex_consumption_per_ga: DataFrame | None = None
    grid_loss: DataFrame | None = None
    positive_grid_loss: DataFrame | None = None
    negative_grid_loss: DataFrame | None = None
    consumption_per_ga_and_brp: DataFrame | None = None
    consumption_per_ga_and_brp_and_es: DataFrame | None = None
    consumption_per_ga_and_es: DataFrame | None = None
    consumption_per_ga: DataFrame | None = None
    production_per_ga_and_brp_and_es: DataFrame | None = None
    production_per_ga_and_brp: DataFrame | None = None
    production_per_ga_and_es: DataFrame | None = None
    production_per_ga: DataFrame | None = None
    flex_consumption_per_ga: DataFrame | None = None
    flex_consumption_per_ga_and_es: DataFrame | None = None
    flex_consumption_per_ga_and_brp_and_es: DataFrame | None = None
    flex_consumption_per_ga_and_brp: DataFrame | None = None
    total_consumption: DataFrame | None = None


@dataclass
class WholesaleResultsContainer:
    tariff_amount_per_charge_from_hourly: DataFrame | None = None
    tariff_monthly_amount_per_charge_from_hourly: DataFrame | None = None
    tariff_amount_per_charge_from_daily: DataFrame | None = None
    tariff_monthly_amount_per_charge_from_daily: DataFrame | None = None


@dataclass
class BasisDataContainer:
    master_basis_data_per_es_per_ga: DataFrame | None = None
    master_basis_data_per_total_ga: DataFrame | None = None
    time_series_quarter_basis_data_per_total_ga: DataFrame | None = None
    time_series_quarter_basis_data_per_es_per_ga: DataFrame | None = None
    time_series_hour_basis_data: DataFrame | None = None
    time_series_hour_basis_data_per_es_per_ga: DataFrame | None = None


@dataclass
class CalculationResultsContainer:
    """
    The usage of `cast(x, None)` is a workaround to prevent the type checker from complaining.
    It's a consequence of the current code design where props are set after creation of the container.
    """

    energy_results: EnergyResultsContainer = cast(EnergyResultsContainer, None)
    wholesale_results: WholesaleResultsContainer | None = None
    basis_data: BasisDataContainer = cast(BasisDataContainer, None)
