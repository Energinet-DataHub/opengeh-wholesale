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
from typing import Tuple

from telemetry_logging import use_span, logging_configuration

import package.calculation.energy.aggregators.exchange_aggregators as exchange_aggr
import package.calculation.energy.aggregators.grid_loss_aggregators as grid_loss_aggr
import package.calculation.energy.aggregators.grouping_aggregators as grouping_aggr
import package.calculation.energy.aggregators.metering_point_time_series_aggregators as mp_aggr
import package.databases.wholesale_results_internal.energy_storage_model_factory as factory
from package.calculation.calculation_output import EnergyResultsOutput
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.energy.data_structures.energy_results import (
    EnergyResults,
)
from package.calculation.energy.resolution_transition_factory import (
    get_energy_result_resolution_adjusted_metering_point_time_series,
)
from package.calculation.preparation.data_structures.grid_loss_metering_point_periods import (
    GridLossMeteringPointPeriods,
)
from package.calculation.preparation.data_structures.metering_point_time_series import (
    MeteringPointTimeSeries,
)
from package.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from package.codelists import (
    CalculationType,
    MeteringPointType,
    TimeSeriesType,
)


@use_span("calculation.execute.energy")
def execute(
    args: CalculatorArgs,
    prepared_metering_point_time_series: PreparedMeteringPointTimeSeries,
    grid_loss_metering_point_periods: GridLossMeteringPointPeriods,
) -> Tuple[EnergyResultsOutput, EnergyResults, EnergyResults]:
    with logging_configuration.start_span("metering_point_time_series"):
        metering_point_time_series = (
            get_energy_result_resolution_adjusted_metering_point_time_series(
                args, prepared_metering_point_time_series
            )
        )
        metering_point_time_series.cache_internal()

    return _calculate(
        args,
        metering_point_time_series,
        grid_loss_metering_point_periods,
    )


def _calculate(
    args: CalculatorArgs,
    metering_point_time_series: MeteringPointTimeSeries,
    grid_loss_metering_point_periods: GridLossMeteringPointPeriods,
) -> Tuple[EnergyResultsOutput, EnergyResults, EnergyResults]:
    energy_results_output = EnergyResultsOutput()

    # cache of net exchange per grid area did not improve performance (01/12/2023)
    exchange = _calculate_exchange(
        args,
        metering_point_time_series,
        energy_results_output,
    )

    temporary_production_per_es = _calculate_temporary_production_per_es(
        args, metering_point_time_series, energy_results_output
    )

    temporary_flex_consumption_per_es = _calculate_temporary_flex_consumption_per_es(
        args, metering_point_time_series, energy_results_output
    )

    non_profiled_consumption_per_es = _calculate_non_profiled_consumption_per_es(
        metering_point_time_series
    )
    non_profiled_consumption_per_es.cache_internal()

    positive_grid_loss, negative_grid_loss = _calculate_grid_loss(
        args,
        exchange,
        temporary_production_per_es,
        temporary_flex_consumption_per_es,
        non_profiled_consumption_per_es,
        grid_loss_metering_point_periods,
        energy_results_output,
    )

    production_per_es = _calculate_adjust_production_per_es(
        temporary_production_per_es,
        negative_grid_loss,
        grid_loss_metering_point_periods,
    )

    flex_consumption_per_es = _calculate_adjust_flex_consumption_per_es(
        temporary_flex_consumption_per_es,
        positive_grid_loss,
        grid_loss_metering_point_periods,
    )

    _calculate_non_profiled_consumption(
        args,
        non_profiled_consumption_per_es,
        energy_results_output,
    )
    production = _calculate_production(
        args,
        production_per_es,
        energy_results_output,
    )
    _calculate_flex_consumption(
        args,
        flex_consumption_per_es,
        energy_results_output,
    )

    _calculate_total_consumption(args, production, exchange, energy_results_output)

    return energy_results_output, positive_grid_loss, negative_grid_loss


@use_span("calculate_exchange")
def _calculate_exchange(
    args: CalculatorArgs,
    metering_point_time_series: MeteringPointTimeSeries,
    energy_results_output: EnergyResultsOutput,
) -> EnergyResults:
    exchange_per_neighbor = exchange_aggr.aggregate_exchange_per_neighbor(
        metering_point_time_series, args.calculation_grid_areas
    )

    # exchange_per_neighbor is a result for eSett.
    # And eSett is only interested in the calculation types aggregation and balance fixing.
    if _is_aggregation_or_balance_fixing(args.calculation_type):
        energy_results_output.exchange_per_neighbor = factory.create(
            args,
            exchange_per_neighbor,
            TimeSeriesType.EXCHANGE_PER_NEIGHBOR,
        )

    exchange = exchange_aggr.aggregate_exchange(exchange_per_neighbor)

    energy_results_output.exchange = factory.create(
        args,
        exchange,
        TimeSeriesType.EXCHANGE,
    )

    return exchange


@use_span("calculate_non_profiled_consumption_per_es")
def _calculate_non_profiled_consumption_per_es(
    metering_point_time_series: MeteringPointTimeSeries,
) -> EnergyResults:
    # Non-profiled consumption per balance responsible party and energy supplier
    non_profiled_consumption_per_es = mp_aggr.aggregate_non_profiled_consumption_per_es(
        metering_point_time_series
    )

    return non_profiled_consumption_per_es


@use_span("calculate_temporary_production_per_es")
def _calculate_temporary_production_per_es(
    args: CalculatorArgs,
    metering_point_time_series: MeteringPointTimeSeries,
    energy_results_output: EnergyResultsOutput,
) -> EnergyResults:
    temporary_production_per_es = mp_aggr.aggregate_production_per_es(
        metering_point_time_series
    )
    temporary_production_per_es.cache_internal()
    # temp production per grid area - used as control result for grid loss
    temporary_production = grouping_aggr.aggregate(temporary_production_per_es)

    energy_results_output.temporary_production = factory.create(
        args,
        temporary_production,
        TimeSeriesType.TEMP_PRODUCTION,
    )

    return temporary_production_per_es


@use_span("calculate_temporary_flex_consumption_per_es")
def _calculate_temporary_flex_consumption_per_es(
    args: CalculatorArgs,
    metering_point_time_series: MeteringPointTimeSeries,
    energy_results_output: EnergyResultsOutput,
) -> EnergyResults:
    temporary_flex_consumption_per_es = mp_aggr.aggregate_flex_consumption_per_es(
        metering_point_time_series
    )
    temporary_flex_consumption_per_es.cache_internal()
    # temp flex consumption per grid area - used as control result for grid loss
    temporary_flex_consumption = grouping_aggr.aggregate(
        temporary_flex_consumption_per_es
    )

    energy_results_output.temporary_flex_consumption = factory.create(
        args,
        temporary_flex_consumption,
        TimeSeriesType.TEMP_FLEX_CONSUMPTION,
    )

    return temporary_flex_consumption_per_es


@use_span("calculate_grid_loss")
def _calculate_grid_loss(
    args: CalculatorArgs,
    exchange: EnergyResults,
    temporary_production_per_es: EnergyResults,
    temporary_flex_consumption_per_es: EnergyResults,
    non_profiled_consumption_per_es: EnergyResults,
    grid_loss_metering_point_periods: GridLossMeteringPointPeriods,
    energy_results_output: EnergyResultsOutput,
) -> tuple[EnergyResults, EnergyResults]:
    grid_loss = grid_loss_aggr.calculate_grid_loss(
        exchange,
        non_profiled_consumption_per_es,
        temporary_flex_consumption_per_es,
        temporary_production_per_es,
    )
    grid_loss.cache_internal()

    energy_results_output.grid_loss = factory.create(
        args, grid_loss, TimeSeriesType.GRID_LOSS
    )

    positive_grid_loss = grid_loss_aggr.calculate_positive_grid_loss(
        grid_loss, grid_loss_metering_point_periods
    )

    energy_results_output.positive_grid_loss = factory.create(
        args,
        positive_grid_loss,
        TimeSeriesType.POSITIVE_GRID_LOSS,
    )

    negative_grid_loss = grid_loss_aggr.calculate_negative_grid_loss(
        grid_loss, grid_loss_metering_point_periods
    )

    energy_results_output.negative_grid_loss = factory.create(
        args,
        negative_grid_loss,
        TimeSeriesType.NEGATIVE_GRID_LOSS,
    )

    return positive_grid_loss, negative_grid_loss


@use_span("calculate_adjust_production_per_es")
def _calculate_adjust_production_per_es(
    temporary_production_per_es: EnergyResults,
    negative_grid_loss: EnergyResults,
    grid_loss_metering_point_periods: GridLossMeteringPointPeriods,
) -> EnergyResults:
    production_per_es = grid_loss_aggr.apply_grid_loss_adjustment(
        temporary_production_per_es,
        negative_grid_loss,
        grid_loss_metering_point_periods,
        MeteringPointType.PRODUCTION,
    )

    return production_per_es


@use_span("calculate_adjust_flex_consumption_per_es")
def _calculate_adjust_flex_consumption_per_es(
    temporary_flex_consumption_per_es: EnergyResults,
    positive_grid_loss: EnergyResults,
    grid_loss_metering_point_periods: GridLossMeteringPointPeriods,
) -> EnergyResults:
    flex_consumption_per_es = grid_loss_aggr.apply_grid_loss_adjustment(
        temporary_flex_consumption_per_es,
        positive_grid_loss,
        grid_loss_metering_point_periods,
        MeteringPointType.CONSUMPTION,
    )

    return flex_consumption_per_es


@use_span("calculate_production")
def _calculate_production(
    args: CalculatorArgs,
    production_per_es: EnergyResults,
    energy_results_output: EnergyResultsOutput,
) -> EnergyResults:
    # production per energy supplier
    energy_results_output.production_per_es = factory.create(
        args,
        production_per_es,
        TimeSeriesType.PRODUCTION,
    )

    if _is_aggregation_or_balance_fixing(args.calculation_type):
        # production per balance responsible
        energy_results_output.production_per_brp = factory.create(
            args,
            grouping_aggr.aggregate_per_brp(production_per_es),
            TimeSeriesType.PRODUCTION,
        )

    # production per grid area
    aggregate = grouping_aggr.aggregate(production_per_es)
    energy_results_output.production = factory.create(
        args,
        aggregate,
        TimeSeriesType.PRODUCTION,
    )

    return aggregate


@use_span("calculate_flex_consumption")
def _calculate_flex_consumption(
    args: CalculatorArgs,
    flex_consumption_per_es: EnergyResults,
    energy_results_output: EnergyResultsOutput,
) -> None:
    # flex consumption per grid area
    energy_results_output.flex_consumption = factory.create(
        args,
        grouping_aggr.aggregate(flex_consumption_per_es),
        TimeSeriesType.FLEX_CONSUMPTION,
    )

    # flex consumption per energy supplier
    energy_results_output.flex_consumption_per_es = factory.create(
        args,
        flex_consumption_per_es,
        TimeSeriesType.FLEX_CONSUMPTION,
    )

    if _is_aggregation_or_balance_fixing(args.calculation_type):
        # flex consumption per balance responsible
        energy_results_output.flex_consumption_per_brp = factory.create(
            args,
            grouping_aggr.aggregate_per_brp(flex_consumption_per_es),
            TimeSeriesType.FLEX_CONSUMPTION,
        )


@use_span("calculate_non_profiled_consumption")
def _calculate_non_profiled_consumption(
    args: CalculatorArgs,
    non_profiled_consumption_per_es: EnergyResults,
    energy_results_output: EnergyResultsOutput,
) -> None:
    # Non-profiled consumption per energy supplier
    energy_results_output.non_profiled_consumption_per_es = factory.create(
        args,
        non_profiled_consumption_per_es,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
    )

    if _is_aggregation_or_balance_fixing(args.calculation_type):
        # Non-profiled consumption per balance responsible
        energy_results_output.non_profiled_consumption_per_brp = factory.create(
            args,
            grouping_aggr.aggregate_per_brp(non_profiled_consumption_per_es),
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
        )

    # Non-profiled consumption per grid area
    energy_results_output.non_profiled_consumption = factory.create(
        args,
        grouping_aggr.aggregate(non_profiled_consumption_per_es),
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
    )


@use_span("calculate_total_consumption")
def _calculate_total_consumption(
    args: CalculatorArgs,
    production: EnergyResults,
    exchange: EnergyResults,
    energy_results_output: EnergyResultsOutput,
) -> None:
    energy_results_output.total_consumption = factory.create(
        args,
        grid_loss_aggr.calculate_total_consumption(production, exchange),
        TimeSeriesType.TOTAL_CONSUMPTION,
    )


def _is_aggregation_or_balance_fixing(calculation_type: CalculationType) -> bool:
    return (
        calculation_type == CalculationType.AGGREGATION
        or calculation_type == CalculationType.BALANCE_FIXING
    )
