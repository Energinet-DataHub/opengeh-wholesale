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
class EnergyResultsOutput:
    """
    Contains all energy results from a calculation.
    """

    exchange_per_neighbor: DataFrame | None = None
    exchange: DataFrame | None = None
    temporary_production: DataFrame | None = None
    temporary_flex_consumption: DataFrame | None = None
    grid_loss: DataFrame | None = None
    positive_grid_loss: DataFrame | None = None
    negative_grid_loss: DataFrame | None = None
    non_profiled_consumption_per_brp: DataFrame | None = None
    non_profiled_consumption_per_es: DataFrame | None = None
    non_profiled_consumption: DataFrame | None = None
    production_per_es: DataFrame | None = None
    production_per_brp: DataFrame | None = None
    production: DataFrame | None = None
    flex_consumption: DataFrame | None = None
    flex_consumption_per_es: DataFrame | None = None
    flex_consumption_per_brp: DataFrame | None = None
    total_consumption: DataFrame | None = None


@dataclass
class WholesaleResultsOutput:
    """
    Contains all wholesale results from a calculation.
    """

    hourly_tariff_per_co_es: DataFrame | None = None
    monthly_tariff_from_hourly_per_co_es: DataFrame | None = None
    monthly_tariff_from_hourly_per_co_es_as_monthly_amount: DataFrame | None = None
    daily_tariff_per_co_es: DataFrame | None = None
    monthly_tariff_from_daily_per_co_es: DataFrame | None = None
    monthly_tariff_from_daily_per_co_es_as_monthly_amount: DataFrame | None = None
    subscription_per_co_es: DataFrame | None = None
    monthly_subscription_per_co_es: DataFrame | None = None
    monthly_subscription_per_co_es_as_monthly_amount: DataFrame | None = None
    fee_per_co_es: DataFrame | None = None
    monthly_fee_per_co_es: DataFrame | None = None
    monthly_fee_per_co_es_as_monthly_amount: DataFrame | None = None
    total_monthly_amounts_per_co_es: DataFrame | None = None
    total_monthly_amounts_per_es: DataFrame | None = None


@dataclass
class BasisDataOutput:
    """
    Contains all the foundation data used in a calculation.
    """

    metering_point_periods: DataFrame
    """Data frame where the columns uses the column names of the storage model."""
    time_series_points: DataFrame
    charge_price_information_periods: DataFrame | None
    charge_price_points: DataFrame | None
    charge_link_periods: DataFrame | None
    grid_loss_metering_points: DataFrame


@dataclass
class CalculationOutput:
    """
    Contains the output of a calculation.
    The output consists of energy and wholesale results and basis data.

    The usage of `cast(x, None)` is a workaround to prevent the type checker from complaining.
    It's a consequence of the current code design where props are set after creation of the container.
    """

    energy_results_output: EnergyResultsOutput = cast(EnergyResultsOutput, None)
    wholesale_results_output: WholesaleResultsOutput | None = None
    basis_data_output: BasisDataOutput = cast(BasisDataOutput, None)
