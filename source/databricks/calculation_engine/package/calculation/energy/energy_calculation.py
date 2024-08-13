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

import package.calculation.energy.aggregators.exchange_aggregators as exchange_aggr
import package.calculation.energy.aggregators.grid_loss_aggregators as grid_loss_aggr
import package.calculation.energy.aggregators.grouping_aggregators as grouping_aggr
import package.calculation.energy.aggregators.metering_point_time_series_aggregators as mp_aggr
import package.databases.wholesale_results_internal.energy_storage_model_factory as factory
from package.calculation.calculation_output import EnergyResults
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.energy.data_structures.energy_results import EnergyResults
from package.calculation.energy.resolution_transition_factory import (
    get_energy_result_resolution_adjusted_metering_point_time_series,
)
from package.calculation.preparation.data_structures.grid_loss_responsible import (
    GridLossResponsible,
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
    AggregationLevel,
    TimeSeriesType,
)
from package.infrastructure import logging_configuration


@logging_configuration.use_span("calculation.execute.energy")
def execute(
    args: CalculatorArgs,
    prepared_metering_point_time_series: PreparedMeteringPointTimeSeries,
    grid_loss_responsible_df: GridLossResponsible,
) -> Tuple[EnergyResults, EnergyResults, EnergyResults]:
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
        grid_loss_responsible_df,
    )


def _calculate(
    args: CalculatorArgs,
    metering_point_time_series: MeteringPointTimeSeries,
    grid_loss_responsible_df: GridLossResponsible,
) -> Tuple[EnergyResults, EnergyResults, EnergyResults]:
    results = EnergyResults()

    # cache of net exchange per grid area did not improve performance (01/12/2023)
    exchange = _calculate_exchange(
        args,
        metering_point_time_series,
        results,
    )

    temporary_production_per_es = _calculate_temporary_production_per_es(
        args, metering_point_time_series, results
    )

    temporary_flex_consumption_per_es = _calculate_temporary_flex_consumption_per_es(
        args, metering_point_time_series, results
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
        grid_loss_responsible_df,
        results,
    )

    production_per_es = _calculate_adjust_production_per_es(
        temporary_production_per_es,
        negative_grid_loss,
        grid_loss_responsible_df,
    )

    flex_consumption_per_es = _calculate_adjust_flex_consumption_per_es(
        temporary_flex_consumption_per_es,
        positive_grid_loss,
        grid_loss_responsible_df,
    )

    _calculate_non_profiled_consumption(
        args,
        non_profiled_consumption_per_es,
        results,
    )
    production = _calculate_production(
        args,
        production_per_es,
        results,
    )
    _calculate_flex_consumption(
        args,
        flex_consumption_per_es,
        results,
    )

    _calculate_total_consumption(args, production, exchange, results)

    return results, positive_grid_loss, negative_grid_loss


@logging_configuration.use_span("calculate_exchange")
def _calculate_exchange(
    args: CalculatorArgs,
    metering_point_time_series: MeteringPointTimeSeries,
    results: EnergyResults,
) -> EnergyResults:
    exchange_per_neighbor = exchange_aggr.aggregate_exchange_per_neighbor(
        metering_point_time_series, args.calculation_grid_areas
    )
    if _is_aggregation_or_balance_fixing(args.calculation_type):
        results.exchange_per_neighbor = factory.create(
            args,
            exchange_per_neighbor,
            TimeSeriesType.EXCHANGE_PER_NEIGHBOR,
            AggregationLevel.GRID_AREA,
        )

    exchange = exchange_aggr.aggregate_exchange(exchange_per_neighbor)

    results.exchange = factory.create(
        args,
        exchange,
        TimeSeriesType.EXCHANGE,
        AggregationLevel.GRID_AREA,
    )

    return exchange


@logging_configuration.use_span("calculate_non_profiled_consumption_per_es")
def _calculate_non_profiled_consumption_per_es(
    metering_point_time_series: MeteringPointTimeSeries,
) -> EnergyResults:
    # Non-profiled consumption per balance responsible party and energy supplier
    non_profiled_consumption_per_es = mp_aggr.aggregate_non_profiled_consumption_per_es(
        metering_point_time_series
    )

    return non_profiled_consumption_per_es


@logging_configuration.use_span("calculate_temporary_production_per_es")
def _calculate_temporary_production_per_es(
    args: CalculatorArgs,
    metering_point_time_series: MeteringPointTimeSeries,
    results: EnergyResults,
) -> EnergyResults:
    temporary_production_per_es = mp_aggr.aggregate_production_per_es(
        metering_point_time_series
    )
    temporary_production_per_es.cache_internal()
    # temp production per grid area - used as control result for grid loss
    temporary_production = grouping_aggr.aggregate(temporary_production_per_es)

    results.temporary_production = factory.create(
        args,
        temporary_production,
        TimeSeriesType.TEMP_PRODUCTION,
        AggregationLevel.GRID_AREA,
    )

    return temporary_production_per_es


@logging_configuration.use_span("calculate_temporary_flex_consumption_per_es")
def _calculate_temporary_flex_consumption_per_es(
    args: CalculatorArgs,
    metering_point_time_series: MeteringPointTimeSeries,
    results: EnergyResults,
) -> EnergyResults:
    temporary_flex_consumption_per_es = mp_aggr.aggregate_flex_consumption_per_es(
        metering_point_time_series
    )
    temporary_flex_consumption_per_es.cache_internal()
    # temp flex consumption per grid area - used as control result for grid loss
    temporary_flex_consumption = grouping_aggr.aggregate(
        temporary_flex_consumption_per_es
    )

    results.temporary_flex_consumption = factory.create(
        args,
        temporary_flex_consumption,
        TimeSeriesType.TEMP_FLEX_CONSUMPTION,
        AggregationLevel.GRID_AREA,
    )

    return temporary_flex_consumption_per_es


@logging_configuration.use_span("calculate_grid_loss")
def _calculate_grid_loss(
    args: CalculatorArgs,
    exchange: EnergyResults,
    temporary_production_per_es: EnergyResults,
    temporary_flex_consumption_per_es: EnergyResults,
    non_profiled_consumption_per_es: EnergyResults,
    grid_loss_responsible_df: GridLossResponsible,
    results: EnergyResults,
) -> tuple[EnergyResults, EnergyResults]:
    grid_loss = grid_loss_aggr.calculate_grid_loss(
        exchange,
        non_profiled_consumption_per_es,
        temporary_flex_consumption_per_es,
        temporary_production_per_es,
    )
    grid_loss.cache_internal()

    results.grid_loss = factory.create(
        args, grid_loss, TimeSeriesType.GRID_LOSS, AggregationLevel.GRID_AREA
    )

    positive_grid_loss = grid_loss_aggr.calculate_positive_grid_loss(
        grid_loss, grid_loss_responsible_df
    )

    results.positive_grid_loss = factory.create(
        args,
        positive_grid_loss,
        TimeSeriesType.POSITIVE_GRID_LOSS,
        AggregationLevel.GRID_AREA,
    )

    negative_grid_loss = grid_loss_aggr.calculate_negative_grid_loss(
        grid_loss, grid_loss_responsible_df
    )

    results.negative_grid_loss = factory.create(
        args,
        negative_grid_loss,
        TimeSeriesType.NEGATIVE_GRID_LOSS,
        AggregationLevel.GRID_AREA,
    )

    return positive_grid_loss, negative_grid_loss


@logging_configuration.use_span("calculate_adjust_production_per_es")
def _calculate_adjust_production_per_es(
    temporary_production_per_es: EnergyResults,
    negative_grid_loss: EnergyResults,
    grid_loss_responsible_df: GridLossResponsible,
) -> EnergyResults:
    production_per_es = grid_loss_aggr.apply_grid_loss_adjustment(
        temporary_production_per_es,
        negative_grid_loss,
        grid_loss_responsible_df,
        MeteringPointType.PRODUCTION,
    )

    return production_per_es


@logging_configuration.use_span("calculate_adjust_flex_consumption_per_es")
def _calculate_adjust_flex_consumption_per_es(
    temporary_flex_consumption_per_es: EnergyResults,
    positive_grid_loss: EnergyResults,
    grid_loss_responsible_df: GridLossResponsible,
) -> EnergyResults:
    flex_consumption_per_es = grid_loss_aggr.apply_grid_loss_adjustment(
        temporary_flex_consumption_per_es,
        positive_grid_loss,
        grid_loss_responsible_df,
        MeteringPointType.CONSUMPTION,
    )

    return flex_consumption_per_es


@logging_configuration.use_span("calculate_production")
def _calculate_production(
    args: CalculatorArgs,
    production_per_es: EnergyResults,
    results: EnergyResults,
) -> EnergyResults:
    # production per energy supplier
    results.production_per_es = factory.create(
        args,
        production_per_es,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.ENERGY_SUPPLIER,
    )

    if _is_aggregation_or_balance_fixing(args.calculation_type):
        # production per balance responsible
        results.production_per_brp = factory.create(
            args,
            grouping_aggr.aggregate_per_brp(production_per_es),
            TimeSeriesType.PRODUCTION,
            AggregationLevel.BALANCE_RESPONSIBLE_PARTY,
        )

    # production per grid area
    aggregate = grouping_aggr.aggregate(production_per_es)
    results.production = factory.create(
        args,
        aggregate,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.GRID_AREA,
    )

    return aggregate


@logging_configuration.use_span("calculate_flex_consumption")
def _calculate_flex_consumption(
    args: CalculatorArgs,
    flex_consumption_per_es: EnergyResults,
    results: EnergyResults,
) -> None:
    # flex consumption per grid area
    results.flex_consumption = factory.create(
        args,
        grouping_aggr.aggregate(flex_consumption_per_es),
        TimeSeriesType.FLEX_CONSUMPTION,
        AggregationLevel.GRID_AREA,
    )

    # flex consumption per energy supplier
    results.flex_consumption_per_es = factory.create(
        args,
        flex_consumption_per_es,
        TimeSeriesType.FLEX_CONSUMPTION,
        AggregationLevel.ENERGY_SUPPLIER,
    )

    if _is_aggregation_or_balance_fixing(args.calculation_type):
        # flex consumption per balance responsible
        results.flex_consumption_per_brp = factory.create(
            args,
            grouping_aggr.aggregate_per_brp(flex_consumption_per_es),
            TimeSeriesType.FLEX_CONSUMPTION,
            AggregationLevel.BALANCE_RESPONSIBLE_PARTY,
        )


@logging_configuration.use_span("calculate_non_profiled_consumption")
def _calculate_non_profiled_consumption(
    args: CalculatorArgs,
    non_profiled_consumption_per_es: EnergyResults,
    results: EnergyResults,
) -> None:
    # Non-profiled consumption per energy supplier
    results.non_profiled_consumption_per_es = factory.create(
        args,
        non_profiled_consumption_per_es,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.ENERGY_SUPPLIER,
    )

    if _is_aggregation_or_balance_fixing(args.calculation_type):
        # Non-profiled consumption per balance responsible
        results.non_profiled_consumption_per_brp = factory.create(
            args,
            grouping_aggr.aggregate_per_brp(non_profiled_consumption_per_es),
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.BALANCE_RESPONSIBLE_PARTY,
        )

    # Non-profiled consumption per grid area
    results.non_profiled_consumption = factory.create(
        args,
        grouping_aggr.aggregate(non_profiled_consumption_per_es),
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.GRID_AREA,
    )


@logging_configuration.use_span("calculate_total_consumption")
def _calculate_total_consumption(
    args: CalculatorArgs,
    production: EnergyResults,
    exchange: EnergyResults,
    results: EnergyResults,
) -> None:
    results.total_consumption = factory.create(
        args,
        grid_loss_aggr.calculate_total_consumption(production, exchange),
        TimeSeriesType.TOTAL_CONSUMPTION,
        AggregationLevel.GRID_AREA,
    )


def _is_aggregation_or_balance_fixing(calculation_type: CalculationType) -> bool:
    return (
        calculation_type == CalculationType.AGGREGATION
        or calculation_type == CalculationType.BALANCE_FIXING
    )
