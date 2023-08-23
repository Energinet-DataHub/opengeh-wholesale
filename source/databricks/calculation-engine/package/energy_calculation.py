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

import package.steps.aggregation as agg_steps
import package.steps.setup as setup
from package.codelists import TimeSeriesType, AggregationLevel, ProcessType
from package.calculation_output.output_writers.basis_data_writer import BasisDataWriter
from package.calculation_output.output_writers.calculation_result_writer import CalculationResultWriter
from pyspark.sql import DataFrame
from typing import Tuple


def execute(
    batch_id: str,
    batch_process_type: ProcessType,
    batch_execution_time_start: datetime,
    wholesale_container_path: str,
    metering_points_periods_df: DataFrame,
    enriched_time_series_point_df: DataFrame,
    grid_loss_responsible_df: DataFrame,
    time_zone: str,
) -> None:

    calculation_result_writer = CalculationResultWriter(
        batch_id,
        batch_process_type,
        batch_execution_time_start,
    )

    basis_data_writer = BasisDataWriter(wholesale_container_path, batch_id)
    basis_data_writer.write(
        metering_points_periods_df,
        enriched_time_series_point_df,
        time_zone,
    )

    enriched_time_series_point_df = setup.transform_hour_to_quarter(
        enriched_time_series_point_df
    )
    _calculate(
        batch_process_type,
        calculation_result_writer,
        enriched_time_series_point_df,
        grid_loss_responsible_df,
    )


def _calculate(
    process_type: ProcessType,
    result_writer: CalculationResultWriter,
    enriched_time_series_point_df: DataFrame,
    grid_loss_responsible_df: DataFrame,
) -> None:

    net_exchange_per_ga = _calculate_net_exchange(
        process_type, result_writer, enriched_time_series_point_df
    )

    temporay_production_per_ga_and_brp_and_es = (
        _calculate_temporay_production_per_per_ga_and_brp_and_es(
            result_writer,
            enriched_time_series_point_df
        )
    )

    temporay_flex_consumption_per_ga_and_brp_and_es = (
        _calculate_temporay_flex_consumption_per_per_ga_and_brp_and_es(
            result_writer,
            enriched_time_series_point_df
        )
    )

    consumption_per_ga_and_brp_and_es = _calculate_consumption_per_ga_and_brp_and_es(
        enriched_time_series_point_df
    )

    positive_grid_loss, negative_grid_loss = _calculate_grid_loss(
        result_writer,
        net_exchange_per_ga,
        temporay_production_per_ga_and_brp_and_es,
        temporay_flex_consumption_per_ga_and_brp_and_es,
        consumption_per_ga_and_brp_and_es,
    )

    production_per_ga_and_brp_and_es = (
        _calculate_adjust_production_per_ga_and_brp_and_es(
            temporay_production_per_ga_and_brp_and_es,
            negative_grid_loss,
            grid_loss_responsible_df,
        )
    )

    flex_consumption_per_ga_and_brp_and_es = (
        _calculate_adjust_flex_consumption_per_ga_and_brp_and_es(
            temporay_flex_consumption_per_ga_and_brp_and_es,
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
    _calculate_flex_consumption(process_type, result_writer, flex_consumption_per_ga_and_brp_and_es)

    _calculate_total_consumption(result_writer, production_per_ga, net_exchange_per_ga)


def _calculate_net_exchange(
    process_type: ProcessType,
    result_writer: CalculationResultWriter,
    enriched_time_series: DataFrame
) -> DataFrame:

    if _is_aggregation_or_balance_fixing(process_type):
        # Could the exchange_per_neighbour_ga be re-used for NET_EXCHANGE_PER_GA?
        exchange_per_neighbour_ga = agg_steps.aggregate_net_exchange_per_neighbour_ga(enriched_time_series)

        result_writer.write(
            exchange_per_neighbour_ga,
            TimeSeriesType.NET_EXCHANGE_PER_NEIGHBORING_GA,
            AggregationLevel.TOTAL_GA,
        )

    exchange_per_grid_area = agg_steps.aggregate_net_exchange_per_ga(enriched_time_series)

    result_writer.write(
        exchange_per_grid_area,
        TimeSeriesType.NET_EXCHANGE_PER_GA,
        AggregationLevel.TOTAL_GA,
    )

    return exchange_per_grid_area


def _calculate_consumption_per_ga_and_brp_and_es(
    enriched_time_series: DataFrame,
) -> DataFrame:
    # Non-profiled consumption per balance responsible party and energy supplier
    consumption_per_ga_and_brp_and_es = (
        agg_steps.aggregate_non_profiled_consumption_ga_brp_es(enriched_time_series)
    )
    return consumption_per_ga_and_brp_and_es


def _calculate_temporay_production_per_per_ga_and_brp_and_es(
    result_writer: CalculationResultWriter,
    enriched_time_series: DataFrame,
) -> DataFrame:
    temporay_production_per_per_ga_and_brp_and_es = (
        agg_steps.aggregate_production_ga_brp_es(enriched_time_series)
    )
    # temp production per grid area - used as control result for grid loss
    temporay_production_per_ga = agg_steps.aggregate_production_ga(
        temporay_production_per_per_ga_and_brp_and_es
    )
    result_writer.write(
        temporay_production_per_ga,
        TimeSeriesType.TEMP_PRODUCTION,
        AggregationLevel.TOTAL_GA,
    )
    return temporay_production_per_per_ga_and_brp_and_es


def _calculate_temporay_flex_consumption_per_per_ga_and_brp_and_es(
    result_writer: CalculationResultWriter,
    enriched_time_series: DataFrame,
) -> DataFrame:
    temporay_flex_consumption_per_ga_and_brp_and_es = (
        agg_steps.aggregate_flex_consumption_ga_brp_es(enriched_time_series)
    )
    # temp flex consumption per grid area - used as control result for grid loss
    temporay_flex_consumption_per_ga = agg_steps.aggregate_flex_consumption_ga(
        temporay_flex_consumption_per_ga_and_brp_and_es
    )
    result_writer.write(
        temporay_flex_consumption_per_ga,
        TimeSeriesType.TEMP_FLEX_CONSUMPTION,
        AggregationLevel.TOTAL_GA,
    )
    return temporay_flex_consumption_per_ga_and_brp_and_es


def _calculate_grid_loss(
    result_writer: CalculationResultWriter,
    net_exchange_per_ga: DataFrame,
    temporay_production_per_ga_and_brp_and_es: DataFrame,
    temporay_flex_consumption_per_ga_and_brp_and_es: DataFrame,
    consumption_per_ga_and_brp_and_es: DataFrame,
) -> Tuple[DataFrame, DataFrame]:
    grid_loss = agg_steps.calculate_grid_loss(
        net_exchange_per_ga,
        consumption_per_ga_and_brp_and_es,
        temporay_flex_consumption_per_ga_and_brp_and_es,
        temporay_production_per_ga_and_brp_and_es,
    )

    result_writer.write(
        grid_loss,
        TimeSeriesType.GRID_LOSS,
        AggregationLevel.TOTAL_GA,
    )

    positive_grid_loss = agg_steps.calculate_positive_grid_loss(grid_loss)

    result_writer.write(
        positive_grid_loss,
        TimeSeriesType.POSITIVE_GRID_LOSS,
        AggregationLevel.TOTAL_GA,
    )

    negative_grid_loss = agg_steps.calculate_negative_grid_loss(grid_loss)

    result_writer.write(
        negative_grid_loss,
        TimeSeriesType.NEGATIVE_GRID_LOSS,
        AggregationLevel.TOTAL_GA,
    )

    return positive_grid_loss, negative_grid_loss


def _calculate_adjust_production_per_ga_and_brp_and_es(
    temporay_production_per_ga_and_brp_and_es: DataFrame,
    negative_grid_loss: DataFrame,
    grid_loss_responsible_df: DataFrame,
) -> DataFrame:
    production_per_ga_and_brp_and_es = agg_steps.adjust_production(
        temporay_production_per_ga_and_brp_and_es,
        negative_grid_loss,
        grid_loss_responsible_df,
    )

    return production_per_ga_and_brp_and_es


def _calculate_adjust_flex_consumption_per_ga_and_brp_and_es(
    temporay_flex_consumption_per_ga_and_brp_and_es: DataFrame,
    positive_grid_loss: DataFrame,
    grid_loss_responsible_df: DataFrame,
) -> DataFrame:
    flex_consumption_per_ga_and_brp_and_es = agg_steps.adjust_flex_consumption(
        temporay_flex_consumption_per_ga_and_brp_and_es,
        positive_grid_loss,
        grid_loss_responsible_df,
    )

    return flex_consumption_per_ga_and_brp_and_es


def _calculate_production(
    process_type: ProcessType,
    result_writer: CalculationResultWriter,
    production_per_ga_and_brp_and_es: DataFrame,
) -> DataFrame:

    if _is_aggregation_or_balance_fixing(process_type):
        # production per balance responsible
        result_writer.write(
            production_per_ga_and_brp_and_es,
            TimeSeriesType.PRODUCTION,
            AggregationLevel.ES_PER_BRP_PER_GA,
        )

        production_per_ga_and_brp = agg_steps.aggregate_production_ga_brp(
            production_per_ga_and_brp_and_es
        )

        result_writer.write(
            production_per_ga_and_brp,
            TimeSeriesType.PRODUCTION,
            AggregationLevel.BRP_PER_GA,
        )

    # production per energy supplier
    production_per_ga_and_es = agg_steps.aggregate_production_ga_es(
        production_per_ga_and_brp_and_es
    )

    result_writer.write(
        production_per_ga_and_es,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.ES_PER_GA,
    )

    # production per grid area
    production_per_ga = agg_steps.aggregate_production_ga(
        production_per_ga_and_brp_and_es
    )

    result_writer.write(
        production_per_ga,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.TOTAL_GA
    )

    return production_per_ga


def _calculate_flex_consumption(
    process_type: ProcessType,
    result_writer: CalculationResultWriter,
    flex_consumption_per_ga_and_brp_and_es: DataFrame,
) -> None:

    # flex consumption per grid area
    flex_consumption_per_ga = agg_steps.aggregate_flex_consumption_ga(
        flex_consumption_per_ga_and_brp_and_es
    )

    result_writer.write(
        flex_consumption_per_ga,
        TimeSeriesType.FLEX_CONSUMPTION,
        AggregationLevel.TOTAL_GA,
    )

    # flex consumption per energy supplier
    flex_consumption_per_ga_and_es = agg_steps.aggregate_flex_consumption_ga_es(
        flex_consumption_per_ga_and_brp_and_es
    )

    result_writer.write(
        flex_consumption_per_ga_and_es,
        TimeSeriesType.FLEX_CONSUMPTION,
        AggregationLevel.ES_PER_GA,
    )

    # flex consumption per balance responsible
    if _is_aggregation_or_balance_fixing(process_type):
        result_writer.write(
            flex_consumption_per_ga_and_brp_and_es,
            TimeSeriesType.FLEX_CONSUMPTION,
            AggregationLevel.ES_PER_BRP_PER_GA,
        )

        flex_consumption_per_ga_and_brp = agg_steps.aggregate_flex_consumption_ga_brp(
            flex_consumption_per_ga_and_brp_and_es
        )

        result_writer.write(
            flex_consumption_per_ga_and_brp,
            TimeSeriesType.FLEX_CONSUMPTION,
            AggregationLevel.BRP_PER_GA,
        )


def _calculate_non_profiled_consumption(
    process_type: ProcessType,
    result_writer: CalculationResultWriter,
    consumption_per_ga_and_brp_and_es: DataFrame,
) -> None:

    # Non-profiled consumption per balance responsible
    if _is_aggregation_or_balance_fixing(process_type):

        consumption_per_ga_and_brp = agg_steps.aggregate_non_profiled_consumption_ga_brp(
            consumption_per_ga_and_brp_and_es
        )

        result_writer.write(
            consumption_per_ga_and_brp,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.BRP_PER_GA,
        )

        result_writer.write(
            consumption_per_ga_and_brp_and_es,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.ES_PER_BRP_PER_GA,
        )

    # Non-profiled consumption per energy supplier
    consumption_per_ga_and_es = agg_steps.aggregate_non_profiled_consumption_ga_es(
        consumption_per_ga_and_brp_and_es
    )

    result_writer.write(
        consumption_per_ga_and_es,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.ES_PER_GA,
    )

    # Non-profiled consumption per grid area
    consumption_per_ga = agg_steps.aggregate_non_profiled_consumption_ga(
        consumption_per_ga_and_brp_and_es
    )

    result_writer.write(
        consumption_per_ga,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.TOTAL_GA,
    )


def _calculate_total_consumption(
    result_writer: CalculationResultWriter,
    production_per_ga: DataFrame,
    net_exchange_per_ga: DataFrame,
) -> None:
    total_consumption = agg_steps.calculate_total_consumption(
        production_per_ga, net_exchange_per_ga
    )
    result_writer.write(
        total_consumption,
        TimeSeriesType.TOTAL_CONSUMPTION,
        AggregationLevel.TOTAL_GA,
    )


def _is_aggregation_or_balance_fixing(process_type: ProcessType) -> bool:
    return process_type == ProcessType.AGGREGATION or process_type == ProcessType.BALANCE_FIXING
