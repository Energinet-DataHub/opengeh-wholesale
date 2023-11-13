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

from package.codelists import (
    MeteringPointType,
    SettlementMethod,
)

from package.calculation.energy.energy_results import EnergyResults
from .transformations.grouping_aggregators import (
    aggregate_per_ga_and_brp_and_es,
    aggregate_per_ga_and_es,
    aggregate_per_ga_and_brp,
    aggregate_per_ga,
)
from ..preparation.quarterly_metering_point_time_series import (
    QuarterlyMeteringPointTimeSeries,
)


def aggregate_non_profiled_consumption_ga_brp_es(
    quarterly_metering_point_time_series: QuarterlyMeteringPointTimeSeries,
) -> EnergyResults:
    return aggregate_per_ga_and_brp_and_es(
        quarterly_metering_point_time_series,
        MeteringPointType.CONSUMPTION,
        SettlementMethod.NON_PROFILED,
    )


def aggregate_flex_consumption_ga_brp_es(
    quarterly_metering_point_time_series: QuarterlyMeteringPointTimeSeries,
) -> EnergyResults:
    return aggregate_per_ga_and_brp_and_es(
        quarterly_metering_point_time_series,
        MeteringPointType.CONSUMPTION,
        SettlementMethod.FLEX,
    )


def aggregate_production_ga_brp_es(
    quarterly_metering_point_time_series: QuarterlyMeteringPointTimeSeries,
) -> EnergyResults:
    return aggregate_per_ga_and_brp_and_es(
        quarterly_metering_point_time_series, MeteringPointType.PRODUCTION, None
    )


def aggregate_production_ga_es(production: EnergyResults) -> EnergyResults:
    return aggregate_per_ga_and_es(
        production,
        MeteringPointType.PRODUCTION,
    )


def aggregate_non_profiled_consumption_ga_es(
    non_profiled_consumption: EnergyResults,
) -> EnergyResults:
    return aggregate_per_ga_and_es(
        non_profiled_consumption,
        MeteringPointType.CONSUMPTION,
    )


def aggregate_flex_consumption_ga_es(flex_consumption: EnergyResults) -> EnergyResults:
    return aggregate_per_ga_and_es(
        flex_consumption,
        MeteringPointType.CONSUMPTION,
    )


def aggregate_production_ga_brp(production: EnergyResults) -> EnergyResults:
    return aggregate_per_ga_and_brp(production, MeteringPointType.PRODUCTION)


def aggregate_non_profiled_consumption_ga_brp(
    non_profiled_consumption: EnergyResults,
) -> EnergyResults:
    return aggregate_per_ga_and_brp(
        non_profiled_consumption,
        MeteringPointType.CONSUMPTION,
    )


def aggregate_flex_consumption_ga_brp(flex_consumption: EnergyResults) -> EnergyResults:
    return aggregate_per_ga_and_brp(
        flex_consumption,
        MeteringPointType.CONSUMPTION,
    )


def aggregate_production_ga(production: EnergyResults) -> EnergyResults:
    return aggregate_per_ga(
        production,
        MeteringPointType.PRODUCTION,
    )


def aggregate_non_profiled_consumption_ga(consumption: EnergyResults) -> EnergyResults:
    return aggregate_per_ga(
        consumption,
        MeteringPointType.CONSUMPTION,
    )


def aggregate_flex_consumption_ga(
    flex_consumption: EnergyResults,
) -> EnergyResults:
    return aggregate_per_ga(
        flex_consumption,
        MeteringPointType.CONSUMPTION,
    )
