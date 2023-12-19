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

from datetime import datetime

from pyspark.sql import DataFrame

import package.calculation.energy.aggregators.exchange_aggregators as exchange_aggr
import package.calculation.energy.aggregators.grid_loss_aggregators as grid_loss_aggr
import package.calculation.energy.aggregators.grouping_aggregators as grouping_aggr
import package.calculation.energy.aggregators.metering_point_time_series_aggregators as mp_aggr
from package.calculation.energy.energy_results import EnergyResults
from package.calculation.energy.hour_to_quarter import transform_hour_to_quarter
from package.calculation.preparation.grid_loss_responsible import GridLossResponsible
from package.calculation.preparation.quarterly_metering_point_time_series import (
    QuarterlyMeteringPointTimeSeries,
)
from package.calculation_output.energy_calculation_result_writer import (
    EnergyCalculationResultWriter,
)
from package.codelists import (
    TimeSeriesType,
    AggregationLevel,
    ProcessType,
    MeteringPointType,
)
from package.infrastructure import logging_configuration


@logging_configuration.use_span("calculation.energy")
def execute(
    batch_id: str,
    batch_process_type: ProcessType,
    batch_execution_time_start: datetime,
    batch_grid_areas: list[str],
    metering_point_time_series: DataFrame,
    grid_loss_responsible_df: GridLossResponsible,
) -> None:
    calculation_result_writer = EnergyCalculationResultWriter(
        batch_id,
        batch_process_type,
        batch_execution_time_start,
    )

    quarterly_metering_point_time_series = transform_hour_to_quarter(
        metering_point_time_series
    )
    quarterly_metering_point_time_series.cache_internal()

    _calculate(
        batch_process_type,
        batch_grid_areas,
        calculation_result_writer,
        quarterly_metering_point_time_series,
        grid_loss_responsible_df,
    )


def _calculate(
    process_type: ProcessType,
    batch_grid_areas: list[str],
    result_writer: EnergyCalculationResultWriter,
    quarterly_metering_point_time_series: QuarterlyMeteringPointTimeSeries,
    grid_loss_responsible_df: GridLossResponsible,
) -> None:
    # cache of net exchange per grid area did not improve performance (01/12/2023)
    net_exchange_per_ga = _calculate_net_exchange(
        process_type,
        batch_grid_areas,
        result_writer,
        quarterly_metering_point_time_series,
    )

    temporary_production_per_ga_and_brp_and_es = (
        _calculate_temporary_production_per_per_ga_and_brp_and_es(
            result_writer, quarterly_metering_point_time_series
        )
    )

    temporary_flex_consumption_per_ga_and_brp_and_es = (
        _calculate_temporary_flex_consumption_per_per_ga_and_brp_and_es(
            result_writer, quarterly_metering_point_time_series
        )
    )

    consumption_per_ga_and_brp_and_es = _calculate_consumption_per_ga_and_brp_and_es(
        quarterly_metering_point_time_series
    )
    consumption_per_ga_and_brp_and_es.cache_internal()

    positive_grid_loss, negative_grid_loss = _calculate_grid_loss(
        result_writer,
        net_exchange_per_ga,
        temporary_production_per_ga_and_brp_and_es,
        temporary_flex_consumption_per_ga_and_brp_and_es,
        consumption_per_ga_and_brp_and_es,
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
        process_type, result_writer, consumption_per_ga_and_brp_and_es
    )
    production_per_ga = _calculate_production(
        process_type, result_writer, production_per_ga_and_brp_and_es
    )
    _calculate_flex_consumption(
        process_type, result_writer, flex_consumption_per_ga_and_brp_and_es
    )

    _calculate_total_consumption(result_writer, production_per_ga, net_exchange_per_ga)


def _calculate_net_exchange(
    process_type: ProcessType,
    batch_grid_areas: list[str],
    result_writer: EnergyCalculationResultWriter,
    quarterly_metering_point_time_series: QuarterlyMeteringPointTimeSeries,
) -> EnergyResults:
    if _is_aggregation_or_balance_fixing(process_type):
        # Could the exchange_per_neighbour_ga be re-used for NET_EXCHANGE_PER_GA?
        exchange_per_neighbour_ga = (
            exchange_aggr.aggregate_net_exchange_per_neighbour_ga(
                quarterly_metering_point_time_series, batch_grid_areas
            )
        )

        with logging_configuration.start_span("net_exchange_per_neighbour_ga"):
            result_writer.write(
                exchange_per_neighbour_ga,
                TimeSeriesType.NET_EXCHANGE_PER_NEIGHBORING_GA,
                AggregationLevel.TOTAL_GA,
            )

    exchange_per_grid_area = exchange_aggr.aggregate_net_exchange_per_ga(
        quarterly_metering_point_time_series, batch_grid_areas
    )

    with logging_configuration.start_span("net_exchange_per_ga"):
        result_writer.write(
            exchange_per_grid_area,
            TimeSeriesType.NET_EXCHANGE_PER_GA,
            AggregationLevel.TOTAL_GA,
        )

    return exchange_per_grid_area


def _calculate_consumption_per_ga_and_brp_and_es(
    quarterly_metering_point_time_series: QuarterlyMeteringPointTimeSeries,
) -> EnergyResults:
    # Non-profiled consumption per balance responsible party and energy supplier
    consumption_per_ga_and_brp_and_es = (
        mp_aggr.aggregate_non_profiled_consumption_ga_brp_es(
            quarterly_metering_point_time_series
        )
    )
    return consumption_per_ga_and_brp_and_es


def _calculate_temporary_production_per_per_ga_and_brp_and_es(
    result_writer: EnergyCalculationResultWriter,
    quarterly_metering_point_time_series: QuarterlyMeteringPointTimeSeries,
) -> EnergyResults:
    temporary_production_per_ga_and_brp_and_es = mp_aggr.aggregate_production_ga_brp_es(
        quarterly_metering_point_time_series
    )
    temporary_production_per_ga_and_brp_and_es.cache_internal()
    # temp production per grid area - used as control result for grid loss
    temporary_production_per_ga = grouping_aggr.aggregate_per_ga(
        temporary_production_per_ga_and_brp_and_es
    )

    with logging_configuration.start_span("temporary_production_per_ga"):
        result_writer.write(
            temporary_production_per_ga,
            TimeSeriesType.TEMP_PRODUCTION,
            AggregationLevel.TOTAL_GA,
        )

    return temporary_production_per_ga_and_brp_and_es


def _calculate_temporary_flex_consumption_per_per_ga_and_brp_and_es(
    result_writer: EnergyCalculationResultWriter,
    quarterly_metering_point_time_series: QuarterlyMeteringPointTimeSeries,
) -> EnergyResults:
    temporary_flex_consumption_per_ga_and_brp_and_es = (
        mp_aggr.aggregate_flex_consumption_ga_brp_es(
            quarterly_metering_point_time_series
        )
    )
    temporary_flex_consumption_per_ga_and_brp_and_es.cache_internal()
    # temp flex consumption per grid area - used as control result for grid loss
    temporary_flex_consumption_per_ga = grouping_aggr.aggregate_per_ga(
        temporary_flex_consumption_per_ga_and_brp_and_es
    )

    with logging_configuration.start_span("temporary_flex_consumption_per_ga"):
        result_writer.write(
            temporary_flex_consumption_per_ga,
            TimeSeriesType.TEMP_FLEX_CONSUMPTION,
            AggregationLevel.TOTAL_GA,
        )

    return temporary_flex_consumption_per_ga_and_brp_and_es


def _calculate_grid_loss(
    result_writer: EnergyCalculationResultWriter,
    net_exchange_per_ga: EnergyResults,
    temporary_production_per_ga_and_brp_and_es: EnergyResults,
    temporary_flex_consumption_per_ga_and_brp_and_es: EnergyResults,
    consumption_per_ga_and_brp_and_es: EnergyResults,
) -> tuple[EnergyResults, EnergyResults]:
    grid_loss = grid_loss_aggr.calculate_grid_loss(
        net_exchange_per_ga,
        consumption_per_ga_and_brp_and_es,
        temporary_flex_consumption_per_ga_and_brp_and_es,
        temporary_production_per_ga_and_brp_and_es,
    )
    grid_loss.cache_internal()

    with logging_configuration.start_span("grid_loss"):
        result_writer.write(
            grid_loss,
            TimeSeriesType.GRID_LOSS,
            AggregationLevel.TOTAL_GA,
        )

    positive_grid_loss = grid_loss_aggr.calculate_positive_grid_loss(grid_loss)

    with logging_configuration.start_span("positive_grid_loss"):
        result_writer.write(
            positive_grid_loss,
            TimeSeriesType.POSITIVE_GRID_LOSS,
            AggregationLevel.TOTAL_GA,
        )

    negative_grid_loss = grid_loss_aggr.calculate_negative_grid_loss(grid_loss)

    with logging_configuration.start_span("negative_grid_loss"):
        result_writer.write(
            negative_grid_loss,
            TimeSeriesType.NEGATIVE_GRID_LOSS,
            AggregationLevel.TOTAL_GA,
        )

    return positive_grid_loss, negative_grid_loss


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


def _calculate_production(
    process_type: ProcessType,
    result_writer: EnergyCalculationResultWriter,
    production_per_ga_and_brp_and_es: EnergyResults,
) -> EnergyResults:
    if _is_aggregation_or_balance_fixing(process_type):
        # production per balance responsible
        with logging_configuration.start_span("production_per_ga_and_brp_and_es"):
            result_writer.write(
                production_per_ga_and_brp_and_es,
                TimeSeriesType.PRODUCTION,
                AggregationLevel.ES_PER_BRP_PER_GA,
            )

        production_per_ga_and_brp = grouping_aggr.aggregate_per_ga_and_brp(
            production_per_ga_and_brp_and_es
        )

        with logging_configuration.start_span("production_per_ga_and_brp"):
            result_writer.write(
                production_per_ga_and_brp,
                TimeSeriesType.PRODUCTION,
                AggregationLevel.BRP_PER_GA,
            )

    # production per energy supplier
    production_per_ga_and_es = grouping_aggr.aggregate_per_ga_and_es(
        production_per_ga_and_brp_and_es
    )

    with logging_configuration.start_span("production_per_ga_and_es"):
        result_writer.write(
            production_per_ga_and_es,
            TimeSeriesType.PRODUCTION,
            AggregationLevel.ES_PER_GA,
        )

    # production per grid area
    production_per_ga = grouping_aggr.aggregate_per_ga(production_per_ga_and_brp_and_es)

    with logging_configuration.start_span("production_per_ga"):
        result_writer.write(
            production_per_ga, TimeSeriesType.PRODUCTION, AggregationLevel.TOTAL_GA
        )

    return production_per_ga


def _calculate_flex_consumption(
    process_type: ProcessType,
    result_writer: EnergyCalculationResultWriter,
    flex_consumption_per_ga_and_brp_and_es: EnergyResults,
) -> None:
    # flex consumption per grid area
    flex_consumption_per_ga = grouping_aggr.aggregate_per_ga(
        flex_consumption_per_ga_and_brp_and_es
    )

    with logging_configuration.start_span("flex_consumption_per_ga"):
        result_writer.write(
            flex_consumption_per_ga,
            TimeSeriesType.FLEX_CONSUMPTION,
            AggregationLevel.TOTAL_GA,
        )

    # flex consumption per energy supplier
    flex_consumption_per_ga_and_es = grouping_aggr.aggregate_per_ga_and_es(
        flex_consumption_per_ga_and_brp_and_es
    )

    with logging_configuration.start_span("flex_consumption_per_ga_and_es"):
        result_writer.write(
            flex_consumption_per_ga_and_es,
            TimeSeriesType.FLEX_CONSUMPTION,
            AggregationLevel.ES_PER_GA,
        )

    # flex consumption per balance responsible
    if _is_aggregation_or_balance_fixing(process_type):
        with logging_configuration.start_span("flex_consumption_per_ga_and_brp_and_es"):
            result_writer.write(
                flex_consumption_per_ga_and_brp_and_es,
                TimeSeriesType.FLEX_CONSUMPTION,
                AggregationLevel.ES_PER_BRP_PER_GA,
            )

        flex_consumption_per_ga_and_brp = grouping_aggr.aggregate_per_ga_and_brp(
            flex_consumption_per_ga_and_brp_and_es
        )

        with logging_configuration.start_span("flex_consumption_per_ga_and_brp"):
            result_writer.write(
                flex_consumption_per_ga_and_brp,
                TimeSeriesType.FLEX_CONSUMPTION,
                AggregationLevel.BRP_PER_GA,
            )


def _calculate_non_profiled_consumption(
    process_type: ProcessType,
    result_writer: EnergyCalculationResultWriter,
    consumption_per_ga_and_brp_and_es: EnergyResults,
) -> None:
    # Non-profiled consumption per balance responsible
    if _is_aggregation_or_balance_fixing(process_type):
        consumption_per_ga_and_brp = grouping_aggr.aggregate_per_ga_and_brp(
            consumption_per_ga_and_brp_and_es
        )

        with logging_configuration.start_span("consumption_per_ga_and_brp"):
            result_writer.write(
                consumption_per_ga_and_brp,
                TimeSeriesType.NON_PROFILED_CONSUMPTION,
                AggregationLevel.BRP_PER_GA,
            )

        with logging_configuration.start_span("consumption_per_ga_and_brp_and_es"):
            result_writer.write(
                consumption_per_ga_and_brp_and_es,
                TimeSeriesType.NON_PROFILED_CONSUMPTION,
                AggregationLevel.ES_PER_BRP_PER_GA,
            )

    # Non-profiled consumption per energy supplier
    consumption_per_ga_and_es = grouping_aggr.aggregate_per_ga_and_es(
        consumption_per_ga_and_brp_and_es
    )

    with logging_configuration.start_span("consumption_per_ga_and_es"):
        result_writer.write(
            consumption_per_ga_and_es,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.ES_PER_GA,
        )

    # Non-profiled consumption per grid area
    consumption_per_ga = grouping_aggr.aggregate_per_ga(
        consumption_per_ga_and_brp_and_es
    )

    with logging_configuration.start_span("consumption_per_ga"):
        result_writer.write(
            consumption_per_ga,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.TOTAL_GA,
        )


def _calculate_total_consumption(
    result_writer: EnergyCalculationResultWriter,
    production_per_ga: EnergyResults,
    net_exchange_per_ga: EnergyResults,
) -> None:
    total_consumption = grid_loss_aggr.calculate_total_consumption(
        production_per_ga, net_exchange_per_ga
    )

    with logging_configuration.start_span("total_consumption"):
        result_writer.write(
            total_consumption,
            TimeSeriesType.TOTAL_CONSUMPTION,
            AggregationLevel.TOTAL_GA,
        )


def _is_aggregation_or_balance_fixing(process_type: ProcessType) -> bool:
    return (
        process_type == ProcessType.AGGREGATION
        or process_type == ProcessType.BALANCE_FIXING
    )
