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
import package.calculation.output.energy_storage_model_factory as factory
from package.calculation.calculation_results import EnergyResultsContainer
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.energy.data_structures.energy_results import EnergyResults
from package.calculation.energy.hour_to_quarter import transform_hour_to_quarter
from package.calculation.preparation.data_structures.grid_loss_responsible import (
    GridLossResponsible,
)
from package.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from package.calculation.preparation.data_structures.metering_point_time_series import (
    MeteringPointTimeSeries,
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
) -> Tuple[EnergyResultsContainer, EnergyResults, EnergyResults]:
    with logging_configuration.start_span("metering_point_time_series"):
        metering_point_time_series = transform_hour_to_quarter(
            prepared_metering_point_time_series
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
) -> Tuple[EnergyResultsContainer, EnergyResults, EnergyResults]:
    results = EnergyResultsContainer()

    # cache of net exchange per grid area did not improve performance (01/12/2023)
    net_exchange_per_ga = _calculate_net_exchange(
        args,
        metering_point_time_series,
        results,
    )

    temporary_production_per_ga_and_brp_and_es = (
        _calculate_temporary_production_per_per_ga_and_brp_and_es(
            args, metering_point_time_series, results
        )
    )

    temporary_flex_consumption_per_ga_and_brp_and_es = (
        _calculate_temporary_flex_consumption_per_per_ga_and_brp_and_es(
            args, metering_point_time_series, results
        )
    )

    non_profiled_consumption_per_ga_and_brp_and_es = (
        _calculate_non_profiled_consumption_per_ga_and_brp_and_es(
            metering_point_time_series
        )
    )
    non_profiled_consumption_per_ga_and_brp_and_es.cache_internal()

    positive_grid_loss, negative_grid_loss = _calculate_grid_loss(
        args,
        net_exchange_per_ga,
        temporary_production_per_ga_and_brp_and_es,
        temporary_flex_consumption_per_ga_and_brp_and_es,
        non_profiled_consumption_per_ga_and_brp_and_es,
        grid_loss_responsible_df,
        results,
    )

    production_per_ga_and_brp_and_es = (
        _calculate_adjust_production_per_ga_and_brp_and_es(
            temporary_production_per_ga_and_brp_and_es,
            negative_grid_loss,
            grid_loss_responsible_df,
        )
    )

    flex_consumption_per_ga_and_brp_and_es = (
        _calculate_adjust_flex_consumption_per_ga_and_brp_and_es(
            temporary_flex_consumption_per_ga_and_brp_and_es,
            positive_grid_loss,
            grid_loss_responsible_df,
        )
    )

    _calculate_non_profiled_consumption(
        args,
        non_profiled_consumption_per_ga_and_brp_and_es,
        results,
    )
    production_per_ga = _calculate_production(
        args,
        production_per_ga_and_brp_and_es,
        results,
    )
    _calculate_flex_consumption(
        args,
        flex_consumption_per_ga_and_brp_and_es,
        results,
    )

    _calculate_total_consumption(args, production_per_ga, net_exchange_per_ga, results)

    return results, positive_grid_loss, negative_grid_loss


@logging_configuration.use_span("calculate_net_exchange")
def _calculate_net_exchange(
    args: CalculatorArgs,
    metering_point_time_series: MeteringPointTimeSeries,
    results: EnergyResultsContainer,
) -> EnergyResults:
    exchange_per_neighbour_ga = exchange_aggr.aggregate_net_exchange_per_neighbour_ga(
        metering_point_time_series, args.calculation_grid_areas
    )
    if _is_aggregation_or_balance_fixing(args.calculation_type):
        exchange_per_neighbour_ga = (
            exchange_aggr.aggregate_net_exchange_per_neighbour_ga(
                metering_point_time_series, args.calculation_grid_areas
            )
        )

        results.net_exchange_per_neighbour_ga = factory.create(
            args,
            exchange_per_neighbour_ga,
            TimeSeriesType.NET_EXCHANGE_PER_NEIGHBORING_GA,
            AggregationLevel.TOTAL_GA,
        )

    exchange_per_grid_area = exchange_aggr.aggregate_net_exchange_per_ga(
        exchange_per_neighbour_ga
    )

    results.net_exchange_per_ga = factory.create(
        args,
        exchange_per_grid_area,
        TimeSeriesType.NET_EXCHANGE_PER_GA,
        AggregationLevel.TOTAL_GA,
    )

    return exchange_per_grid_area


@logging_configuration.use_span(
    "calculate_non_profiled_consumption_per_ga_and_brp_and_es"
)
def _calculate_non_profiled_consumption_per_ga_and_brp_and_es(
    metering_point_time_series: MeteringPointTimeSeries,
) -> EnergyResults:
    # Non-profiled consumption per balance responsible party and energy supplier
    non_profiled_consumption_per_ga_and_brp_and_es = (
        mp_aggr.aggregate_non_profiled_consumption_ga_brp_es(metering_point_time_series)
    )

    return non_profiled_consumption_per_ga_and_brp_and_es


@logging_configuration.use_span(
    "calculate_temporary_production_per_per_ga_and_brp_and_es"
)
def _calculate_temporary_production_per_per_ga_and_brp_and_es(
    args: CalculatorArgs,
    metering_point_time_series: MeteringPointTimeSeries,
    results: EnergyResultsContainer,
) -> EnergyResults:
    temporary_production_per_ga_and_brp_and_es = mp_aggr.aggregate_production_ga_brp_es(
        metering_point_time_series
    )
    temporary_production_per_ga_and_brp_and_es.cache_internal()
    # temp production per grid area - used as control result for grid loss
    temporary_production_per_ga = grouping_aggr.aggregate_per_ga(
        temporary_production_per_ga_and_brp_and_es
    )

    results.temporary_production_per_ga = factory.create(
        args,
        temporary_production_per_ga,
        TimeSeriesType.TEMP_PRODUCTION,
        AggregationLevel.TOTAL_GA,
    )

    return temporary_production_per_ga_and_brp_and_es


@logging_configuration.use_span(
    "calculate_temporary_flex_consumption_per_per_ga_and_brp_and_es"
)
def _calculate_temporary_flex_consumption_per_per_ga_and_brp_and_es(
    args: CalculatorArgs,
    metering_point_time_series: MeteringPointTimeSeries,
    results: EnergyResultsContainer,
) -> EnergyResults:
    temporary_flex_consumption_per_ga_and_brp_and_es = (
        mp_aggr.aggregate_flex_consumption_ga_brp_es(metering_point_time_series)
    )
    temporary_flex_consumption_per_ga_and_brp_and_es.cache_internal()
    # temp flex consumption per grid area - used as control result for grid loss
    temporary_flex_consumption_per_ga = grouping_aggr.aggregate_per_ga(
        temporary_flex_consumption_per_ga_and_brp_and_es
    )

    results.temporary_flex_consumption_per_ga = factory.create(
        args,
        temporary_flex_consumption_per_ga,
        TimeSeriesType.TEMP_FLEX_CONSUMPTION,
        AggregationLevel.TOTAL_GA,
    )

    return temporary_flex_consumption_per_ga_and_brp_and_es


@logging_configuration.use_span("calculate_grid_loss")
def _calculate_grid_loss(
    args: CalculatorArgs,
    net_exchange_per_ga: EnergyResults,
    temporary_production_per_ga_and_brp_and_es: EnergyResults,
    temporary_flex_consumption_per_ga_and_brp_and_es: EnergyResults,
    non_profiled_consumption_per_ga_and_brp_and_es: EnergyResults,
    grid_loss_responsible_df: GridLossResponsible,
    results: EnergyResultsContainer,
) -> tuple[EnergyResults, EnergyResults]:
    grid_loss = grid_loss_aggr.calculate_grid_loss(
        net_exchange_per_ga,
        non_profiled_consumption_per_ga_and_brp_and_es,
        temporary_flex_consumption_per_ga_and_brp_and_es,
        temporary_production_per_ga_and_brp_and_es,
    )
    grid_loss.cache_internal()

    results.grid_loss = factory.create(
        args, grid_loss, TimeSeriesType.GRID_LOSS, AggregationLevel.TOTAL_GA
    )

    positive_grid_loss = grid_loss_aggr.calculate_positive_grid_loss(
        grid_loss, grid_loss_responsible_df
    )

    results.positive_grid_loss = factory.create(
        args,
        positive_grid_loss,
        TimeSeriesType.POSITIVE_GRID_LOSS,
        AggregationLevel.TOTAL_GA,
    )

    negative_grid_loss = grid_loss_aggr.calculate_negative_grid_loss(
        grid_loss, grid_loss_responsible_df
    )

    results.negative_grid_loss = factory.create(
        args,
        negative_grid_loss,
        TimeSeriesType.NEGATIVE_GRID_LOSS,
        AggregationLevel.TOTAL_GA,
    )

    return positive_grid_loss, negative_grid_loss


@logging_configuration.use_span("calculate_adjust_production_per_ga_and_brp_and_es")
def _calculate_adjust_production_per_ga_and_brp_and_es(
    temporary_production_per_ga_and_brp_and_es: EnergyResults,
    negative_grid_loss: EnergyResults,
    grid_loss_responsible_df: GridLossResponsible,
) -> EnergyResults:
    production_per_ga_and_brp_and_es = grid_loss_aggr.apply_grid_loss_adjustment(
        temporary_production_per_ga_and_brp_and_es,
        negative_grid_loss,
        grid_loss_responsible_df,
        MeteringPointType.PRODUCTION,
    )

    return production_per_ga_and_brp_and_es


@logging_configuration.use_span(
    "calculate_adjust_flex_consumption_per_ga_and_brp_and_es"
)
def _calculate_adjust_flex_consumption_per_ga_and_brp_and_es(
    temporary_flex_consumption_per_ga_and_brp_and_es: EnergyResults,
    positive_grid_loss: EnergyResults,
    grid_loss_responsible_df: GridLossResponsible,
) -> EnergyResults:
    flex_consumption_per_ga_and_brp_and_es = grid_loss_aggr.apply_grid_loss_adjustment(
        temporary_flex_consumption_per_ga_and_brp_and_es,
        positive_grid_loss,
        grid_loss_responsible_df,
        MeteringPointType.CONSUMPTION,
    )

    return flex_consumption_per_ga_and_brp_and_es


@logging_configuration.use_span("calculate_production")
def _calculate_production(
    args: CalculatorArgs,
    production_per_ga_and_brp_and_es: EnergyResults,
    results: EnergyResultsContainer,
) -> EnergyResults:
    if _is_aggregation_or_balance_fixing(args.calculation_type):
        # production per balance responsible
        results.production_per_ga_and_brp_and_es = factory.create(
            args,
            production_per_ga_and_brp_and_es,
            TimeSeriesType.PRODUCTION,
            AggregationLevel.ES_PER_BRP_PER_GA,
        )

        results.production_per_ga_and_brp = factory.create(
            args,
            grouping_aggr.aggregate_per_ga_and_brp(production_per_ga_and_brp_and_es),
            TimeSeriesType.PRODUCTION,
            AggregationLevel.BRP_PER_GA,
        )

    # production per energy supplier
    results.production_per_ga_and_es = factory.create(
        args,
        grouping_aggr.aggregate_per_ga_and_es(production_per_ga_and_brp_and_es),
        TimeSeriesType.PRODUCTION,
        AggregationLevel.ES_PER_GA,
    )

    # production per grid area
    aggregate_per_ga = grouping_aggr.aggregate_per_ga(production_per_ga_and_brp_and_es)
    results.production_per_ga = factory.create(
        args,
        aggregate_per_ga,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.TOTAL_GA,
    )

    return aggregate_per_ga


@logging_configuration.use_span("calculate_flex_consumption")
def _calculate_flex_consumption(
    args: CalculatorArgs,
    flex_consumption_per_ga_and_brp_and_es: EnergyResults,
    results: EnergyResultsContainer,
) -> None:
    # flex consumption per grid area
    results.flex_consumption_per_ga = factory.create(
        args,
        grouping_aggr.aggregate_per_ga(flex_consumption_per_ga_and_brp_and_es),
        TimeSeriesType.FLEX_CONSUMPTION,
        AggregationLevel.TOTAL_GA,
    )

    # flex consumption per energy supplier
    results.flex_consumption_per_ga_and_es = factory.create(
        args,
        grouping_aggr.aggregate_per_ga_and_es(flex_consumption_per_ga_and_brp_and_es),
        TimeSeriesType.FLEX_CONSUMPTION,
        AggregationLevel.ES_PER_GA,
    )

    # flex consumption per balance responsible
    if _is_aggregation_or_balance_fixing(args.calculation_type):
        results.flex_consumption_per_ga_and_brp_and_es = factory.create(
            args,
            flex_consumption_per_ga_and_brp_and_es,
            TimeSeriesType.FLEX_CONSUMPTION,
            AggregationLevel.ES_PER_BRP_PER_GA,
        )

        flex_consumption_per_ga_and_brp = grouping_aggr.aggregate_per_ga_and_brp(
            flex_consumption_per_ga_and_brp_and_es
        )

        results.flex_consumption_per_ga_and_brp = factory.create(
            args,
            flex_consumption_per_ga_and_brp,
            TimeSeriesType.FLEX_CONSUMPTION,
            AggregationLevel.BRP_PER_GA,
        )


@logging_configuration.use_span("calculate_non_profiled_consumption")
def _calculate_non_profiled_consumption(
    args: CalculatorArgs,
    non_profiled_consumption_per_ga_and_brp_and_es: EnergyResults,
    results: EnergyResultsContainer,
) -> None:
    # Non-profiled consumption per balance responsible
    if _is_aggregation_or_balance_fixing(args.calculation_type):
        non_profiled_consumption_per_ga_and_brp = (
            grouping_aggr.aggregate_per_ga_and_brp(
                non_profiled_consumption_per_ga_and_brp_and_es
            )
        )

        results.non_profiled_consumption_per_ga_and_brp = factory.create(
            args,
            non_profiled_consumption_per_ga_and_brp,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.BRP_PER_GA,
        )
        results.non_profiled_consumption_per_ga_and_brp_and_es = factory.create(
            args,
            non_profiled_consumption_per_ga_and_brp_and_es,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.ES_PER_BRP_PER_GA,
        )

    # Non-profiled consumption per energy supplier
    results.non_profiled_consumption_per_ga_and_es = factory.create(
        args,
        grouping_aggr.aggregate_per_ga_and_es(
            non_profiled_consumption_per_ga_and_brp_and_es
        ),
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.ES_PER_GA,
    )

    # Non-profiled consumption per grid area
    results.non_profiled_consumption_per_ga = factory.create(
        args,
        grouping_aggr.aggregate_per_ga(non_profiled_consumption_per_ga_and_brp_and_es),
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.TOTAL_GA,
    )


@logging_configuration.use_span("calculate_total_consumption")
def _calculate_total_consumption(
    args: CalculatorArgs,
    production_per_ga: EnergyResults,
    net_exchange_per_ga: EnergyResults,
    results: EnergyResultsContainer,
) -> None:
    results.total_consumption = factory.create(
        args,
        grid_loss_aggr.calculate_total_consumption(
            production_per_ga, net_exchange_per_ga
        ),
        TimeSeriesType.TOTAL_CONSUMPTION,
        AggregationLevel.TOTAL_GA,
    )


def _is_aggregation_or_balance_fixing(calculation_type: CalculationType) -> bool:
    return (
        calculation_type == CalculationType.AGGREGATION
        or calculation_type == CalculationType.BALANCE_FIXING
    )
