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


def calculate_balance_fixing(
    actors_writer: ActorsWriter,
    basis_data_writer: BasisDataWriter,
    process_step_result_writer: ProcessStepResultWriter,
    metering_points_periods_df: DataFrame,
    time_series_points_df: DataFrame,
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
    _calculate(actors_writer, process_step_result_writer, enriched_time_series_point_df)


def _calculate(
    actors_writer: ActorsWriter,
    result_writer: ProcessStepResultWriter,
    enriched_time_series_point_df: DataFrame,
) -> None:
    _calculate_net_exchange_per_neighboring_ga(
        result_writer, enriched_time_series_point_df
    )
    _calculate_net_exchange_per_ga(result_writer, enriched_time_series_point_df)
    _calculate_production(result_writer, enriched_time_series_point_df)
    _calculate_flex_consumption(result_writer, enriched_time_series_point_df)
    _calculate_non_profiled_consumption(
        actors_writer, result_writer, enriched_time_series_point_df
    )


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
) -> None:
    exchange = agg_steps.aggregate_net_exchange_per_ga(enriched_time_series)

    result_writer.write(
        exchange,
        TimeSeriesType.NET_EXCHANGE_PER_GA,
        AggregationLevel.total_ga,
    )


def _calculate_production(
    result_writer: ProcessStepResultWriter, enriched_time_series: DataFrame
) -> None:
    production_per_per_ga_and_brp_and_es = agg_steps.aggregate_production_ga_brp_es(
        enriched_time_series
    )
    production_per_ga = agg_steps.aggregate_production_ga(
        production_per_per_ga_and_brp_and_es
    )

    result_writer.write(
        production_per_ga, TimeSeriesType.PRODUCTION, AggregationLevel.total_ga
    )


def _calculate_flex_consumption(
    result_writer: ProcessStepResultWriter, enriched_time_series: DataFrame
) -> None:
    flex_consumption_per_per_ga_and_brp_and_es = (
        agg_steps.aggregate_flex_consumption_ga_brp_es(enriched_time_series)
    )

    flex_consumption_per_ga = agg_steps.aggregate_flex_consumption_ga(
        flex_consumption_per_per_ga_and_brp_and_es
    )

    result_writer.write(
        flex_consumption_per_ga,
        TimeSeriesType.FLEX_CONSUMPTION,
        AggregationLevel.total_ga,
    )


def _calculate_non_profiled_consumption(
    actors_writer: ActorsWriter,
    result_writer: ProcessStepResultWriter,
    enriched_time_series_point_df: DataFrame,
) -> None:
    # Non-profiled consumption per balance responsible party and energy supplier
    consumption_per_ga_and_brp_and_es = (
        agg_steps.aggregate_non_profiled_consumption_ga_brp_es(
            enriched_time_series_point_df
        )
    )

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
