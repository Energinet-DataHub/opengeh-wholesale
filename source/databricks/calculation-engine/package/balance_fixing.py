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
from package.codelists import TimeSeriesType, AggregationLevel
from package.file_writers.actors_writer import ActorsWriter
from package.file_writers.basis_data_writer import BasisDataWriter
from package.file_writers.process_step_result_writer import ProcessStepResultWriter
from pyspark.sql import DataFrame
from typing import Tuple
from package.constants import Colname
from pyspark.sql.functions import col


def calculate_balance_fixing(
    actors_writer: ActorsWriter,
    basis_data_writer: BasisDataWriter,
    process_step_result_writer: ProcessStepResultWriter,
    metering_points_periods_df: DataFrame,
    time_series_points_df: DataFrame,
    grid_loss_responsible_df: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
    time_zone: str,
) -> None:
    enriched_time_series_point_df = setup.get_enriched_time_series_points_df(
        time_series_points_df,
        metering_points_periods_df,
        period_start_datetime,
        period_end_datetime,
    )

    basis_data_writer.write(
        metering_points_periods_df,
        enriched_time_series_point_df,
        period_start_datetime,
        period_end_datetime,
        time_zone,
    )

    enriched_time_series_point_df = setup.transform_hour_to_quarter(
        enriched_time_series_point_df
    )
    _calculate(actors_writer, process_step_result_writer, enriched_time_series_point_df, grid_loss_responsible_df)


def _calculate(
    actors_writer: ActorsWriter,
    result_writer: ProcessStepResultWriter,
    enriched_time_series_point_df: DataFrame,
    grid_loss_responsible_df: DataFrame,
) -> None:
    _calculate_net_exchange_per_neighboring_ga(
        result_writer, enriched_time_series_point_df
    )
    net_exchange_per_ga = _calculate_net_exchange_per_ga(
        result_writer, enriched_time_series_point_df
    )

    temporay_production_per_ga_and_brp_and_es = (
        _calculate_temporay_production_per_per_ga_and_brp_and_es(
            enriched_time_series_point_df
        )
    )
    temporay_flex_consumption_per_ga_and_brp_and_es = (
        _calculate_temporay_flex_consumption_per_per_ga_and_brp_and_es(
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

    production_per_ga_and_brp_and_es = _calculate_adjust_production_per_ga_and_brp_and_es(
        temporay_production_per_ga_and_brp_and_es,
        negative_grid_loss,
        grid_loss_responsible_df
    )

    flex_consumption_per_ga_and_brp_and_es = _calculate_adjust_flex_consumption_per_ga_and_brp_and_es(
        temporay_flex_consumption_per_ga_and_brp_and_es,
        positive_grid_loss,
        grid_loss_responsible_df
    )

    consumption_per_ga = _calculate_non_profiled_consumption(
        actors_writer, result_writer, consumption_per_ga_and_brp_and_es
    )
    production_per_ga = _calculate_production(actors_writer, result_writer, production_per_ga_and_brp_and_es)
    flex_consumption_per_ga = _calculate_flex_consumption(actors_writer, result_writer, flex_consumption_per_ga_and_brp_and_es)

    _calculate_total_consumption(actors_writer, result_writer, production_per_ga, net_exchange_per_ga)
    _calculate_residual_ga(actors_writer, result_writer, net_exchange_per_ga, consumption_per_ga, flex_consumption_per_ga, production_per_ga)


def _calculate_net_exchange_per_neighboring_ga(
    result_writer: ProcessStepResultWriter, enriched_time_series: DataFrame
) -> None:
    exchange = agg_steps.aggregate_net_exchange_per_neighbour_ga(enriched_time_series)

    result_writer.write(
        exchange,
        TimeSeriesType.NET_EXCHANGE_PER_NEIGHBORING_GA,
        AggregationLevel.total_ga,
    )


def _calculate_net_exchange_per_ga(
    result_writer: ProcessStepResultWriter, enriched_time_series: DataFrame
) -> DataFrame:
    exchange = agg_steps.aggregate_net_exchange_per_ga(enriched_time_series)

    result_writer.write(
        exchange,
        TimeSeriesType.NET_EXCHANGE_PER_GA,
        AggregationLevel.total_ga,
    )

    return exchange


def _calculate_consumption_per_ga_and_brp_and_es(
    enriched_time_series: DataFrame,
) -> DataFrame:
    # Non-profiled consumption per balance responsible party and energy supplier
    consumption_per_ga_and_brp_and_es = (
        agg_steps.aggregate_non_profiled_consumption_ga_brp_es(enriched_time_series)
    )
    return consumption_per_ga_and_brp_and_es


def _calculate_temporay_production_per_per_ga_and_brp_and_es(
    enriched_time_series: DataFrame,
) -> DataFrame:
    temporay_production_per_per_ga_and_brp_and_es = (
        agg_steps.aggregate_production_ga_brp_es(enriched_time_series)
    )
    return temporay_production_per_per_ga_and_brp_and_es


def _calculate_temporay_flex_consumption_per_per_ga_and_brp_and_es(
    enriched_time_series: DataFrame,
) -> DataFrame:
    temporay_flex_consumption_per_ga_and_brp_and_es = (
        agg_steps.aggregate_flex_consumption_ga_brp_es(enriched_time_series)
    )
    return temporay_flex_consumption_per_ga_and_brp_and_es


def _calculate_grid_loss(
    result_writer: ProcessStepResultWriter,
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
        AggregationLevel.total_ga,
    )

    positive_grid_loss = agg_steps.calculate_positive_grid_loss(grid_loss)

    result_writer.write(
        positive_grid_loss,
        TimeSeriesType.POSITIVE_GRID_LOSS,
        AggregationLevel.total_ga,
    )

    negative_grid_loss = agg_steps.calculate_negative_grid_loss(grid_loss)

    result_writer.write(
        negative_grid_loss,
        TimeSeriesType.NEGATIVE_GRID_LOSS,
        AggregationLevel.total_ga,
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
        grid_loss_responsible_df
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
        grid_loss_responsible_df
    )

    return flex_consumption_per_ga_and_brp_and_es


def _calculate_production(
    actors_writer: ActorsWriter,
    result_writer: ProcessStepResultWriter,
    production_per_ga_and_brp_and_es: DataFrame,
) -> DataFrame:
    result_writer.write(
        production_per_ga_and_brp_and_es,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.es_per_brp_per_ga,
    )

    # production per energy supplier
    production_per_ga_and_es = agg_steps.aggregate_production_ga_es(
        production_per_ga_and_brp_and_es
    )

    result_writer.write(
        production_per_ga_and_es,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.es_per_ga,
    )

    # production per balance responsible
    production_per_ga_and_brp = agg_steps.aggregate_production_ga_brp(
        production_per_ga_and_brp_and_es
    )

    result_writer.write(
        production_per_ga_and_brp,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.brp_per_ga,
    )

    # production per grid area
    production_per_ga = agg_steps.aggregate_production_ga(
        production_per_ga_and_brp_and_es
    )

    result_writer.write(
        production_per_ga, TimeSeriesType.PRODUCTION, AggregationLevel.total_ga
    )

    # write actors list to datalake
    actors_writer.write(
        production_per_ga_and_brp_and_es, TimeSeriesType.PRODUCTION
    )

    return production_per_ga


def _calculate_flex_consumption(
    actors_writer: ActorsWriter,
    result_writer: ProcessStepResultWriter,
    flex_consumption_per_ga_and_brp_and_es: DataFrame,
) -> DataFrame:
    result_writer.write(
        flex_consumption_per_ga_and_brp_and_es,
        TimeSeriesType.FLEX_CONSUMPTION,
        AggregationLevel.es_per_brp_per_ga,
    )

    # flex consumption per energy supplier
    flex_consumption_per_ga_and_es = agg_steps.aggregate_flex_consumption_ga_es(
        flex_consumption_per_ga_and_brp_and_es
    )

    result_writer.write(
        flex_consumption_per_ga_and_es,
        TimeSeriesType.FLEX_CONSUMPTION,
        AggregationLevel.es_per_ga,
    )

    # flex consumption per balance responsible
    flex_consumption_per_ga_and_brp = agg_steps.aggregate_flex_consumption_ga_brp(
        flex_consumption_per_ga_and_brp_and_es
    )

    result_writer.write(
        flex_consumption_per_ga_and_brp,
        TimeSeriesType.FLEX_CONSUMPTION,
        AggregationLevel.brp_per_ga,
    )

    # flex consumption per grid area
    flex_consumption_per_ga = agg_steps.aggregate_flex_consumption_ga(
        flex_consumption_per_ga_and_brp_and_es
    )

    result_writer.write(
        flex_consumption_per_ga,
        TimeSeriesType.FLEX_CONSUMPTION,
        AggregationLevel.total_ga,
    )

    # write actors list to datalake
    actors_writer.write(
        flex_consumption_per_ga_and_brp_and_es, TimeSeriesType.FLEX_CONSUMPTION
    )

    return flex_consumption_per_ga


def _calculate_non_profiled_consumption(
    actors_writer: ActorsWriter,
    result_writer: ProcessStepResultWriter,
    consumption_per_ga_and_brp_and_es: DataFrame,
) -> DataFrame:
    result_writer.write(
        consumption_per_ga_and_brp_and_es,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.es_per_brp_per_ga,
    )

    # Non-profiled consumption per energy supplier
    consumption_per_ga_and_es = agg_steps.aggregate_non_profiled_consumption_ga_es(
        consumption_per_ga_and_brp_and_es
    )

    result_writer.write(
        consumption_per_ga_and_es,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.es_per_ga,
    )

    # Non-profiled consumption per balance responsible
    consumption_per_ga_and_brp = agg_steps.aggregate_non_profiled_consumption_ga_brp(
        consumption_per_ga_and_brp_and_es
    )

    result_writer.write(
        consumption_per_ga_and_brp,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.brp_per_ga,
    )

    # Non-profiled consumption per grid area
    consumption_per_ga = agg_steps.aggregate_non_profiled_consumption_ga(
        consumption_per_ga_and_brp_and_es
    )

    result_writer.write(
        consumption_per_ga,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.total_ga,
    )

    # write actors list to datalake
    actors_writer.write(
        consumption_per_ga_and_brp_and_es, TimeSeriesType.NON_PROFILED_CONSUMPTION
    )

    return consumption_per_ga


def _calculate_total_consumption(
    actors_writer: ActorsWriter,
    result_writer: ProcessStepResultWriter,
    production_per_ga: DataFrame,
    net_exchange_per_ga: DataFrame
) -> None:
    total_consumption = agg_steps.calculate_total_consumption(production_per_ga, net_exchange_per_ga)
    result_writer.write(
        total_consumption,
        TimeSeriesType.TOTAL_CONSUMPTION,
        AggregationLevel.total_ga,
    )

    # write actors list to datalake
    actors_writer.write(
        total_consumption, TimeSeriesType.TOTAL_CONSUMPTION
    )


def _calculate_residual_ga(
    actors_writer: ActorsWriter,
    result_writer: ProcessStepResultWriter,
    net_exchange_per_ga: DataFrame,
    consumption_per_ga: DataFrame,
    flex_consumption_per_ga: DataFrame,
    production_per_ga: DataFrame
) -> None:
    residual = agg_steps.calculate_residual_ga(net_exchange_per_ga, consumption_per_ga, flex_consumption_per_ga, production_per_ga)
    result_writer.write(
        residual,
        TimeSeriesType.RESIDUAL,
        AggregationLevel.total_ga,
    )

    # write actors list to datalake
    actors_writer.write(
        residual, TimeSeriesType.RESIDUAL
    )
