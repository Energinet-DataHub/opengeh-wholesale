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
from package.codelists import MeteringPointResolution
from package.constants import Colname
from package.codelists import TimeSeriesType, AggregationLevel
from package.db_logging import debug
from package.file_writers.actors_writer import ActorsWriter
from package.file_writers.basis_data_writer import BasisDataWriter
from package.file_writers.process_step_result_writer import ProcessStepResultWriter
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, explode, expr
from pyspark.sql.types import DecimalType


def calculate_balance_fixing(
    actors_writer: ActorsWriter,
    basis_data_writer: BasisDataWriter,
    process_step_result_writer: ProcessStepResultWriter,
    metering_points_periods_df: DataFrame,
    timeseries_points: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
    time_zone: str,
) -> None:
    enriched_time_series_point_df = _get_enriched_time_series_points_df(
        timeseries_points,
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

    _calculate(actors_writer, process_step_result_writer, enriched_time_series_point_df)


def _calculate(
    actors_writer: ActorsWriter,
    result_writer: ProcessStepResultWriter,
    enriched_time_series_point_df: DataFrame,
) -> None:
    _calculate_production(result_writer, enriched_time_series_point_df)

    _calculate_non_profiled_consumption(
        actors_writer, result_writer, enriched_time_series_point_df
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
        Grouping.es_per_brp_per_ga,
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


def _get_enriched_time_series_points_df(
    new_timeseries_df: DataFrame,
    master_basis_data_df: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:
    new_timeseries_df = new_timeseries_df.where(
        col(Colname.observation_time) >= period_start_datetime
    ).where(col(Colname.observation_time) < period_end_datetime)

    quarterly_mp_df = master_basis_data_df.where(
        col("Resolution") == MeteringPointResolution.quarter.value
    ).withColumn("ToDate", (col("ToDate") - expr("INTERVAL 1 seconds")))

    hourly_mp_df = master_basis_data_df.where(
        col("Resolution") == MeteringPointResolution.hour.value
    ).withColumn("ToDate", (col("ToDate") - expr("INTERVAL 1 seconds")))

    quarterly_times_df = (
        quarterly_mp_df.select("MeteringPointId", "FromDate", "ToDate")
        .distinct()
        .select(
            "MeteringPointId",
            expr(
                "sequence(to_timestamp(FromDate), to_timestamp(ToDate), interval 15 minutes)"
            ).alias("quarter_times"),
        )
        .select(
            "MeteringPointId", explode("quarter_times").alias(Colname.observation_time)
        )
    )

    hourly_times_df = (
        hourly_mp_df.select("MeteringPointId", "FromDate", "ToDate")
        .distinct()
        .select(
            "MeteringPointId",
            expr(
                "sequence(to_timestamp(FromDate), to_timestamp(ToDate), interval 1 hour)"
            ).alias("times"),
        )
        .select("MeteringPointId", explode("times").alias(Colname.observation_time))
    )

    empty_points_for_each_metering_point_df = quarterly_times_df.union(hourly_times_df)

    debug(
        "Time series points where time is within period",
        new_timeseries_df.orderBy(
            Colname.metering_point_id, col(Colname.observation_time)
        ),
    )

    new_timeseries_df = new_timeseries_df.select(
        Colname.metering_point_id,
        Colname.observation_time,
        Colname.quantity,
        Colname.quality,
    )

    new_points_for_each_metering_point_df = (
        empty_points_for_each_metering_point_df.join(
            new_timeseries_df,
            [Colname.metering_point_id, Colname.observation_time],
            "left",
        )
    )

    # the master_basis_data_df is allready used once when creating the empty_points_for_each_metering_point_df
    # rejoining master_basis_data_df with empty_points_for_each_metering_point_df requires the GsrNumber and
    # Resolution column must be renamed for the select to be succesfull.

    new_points_for_each_metering_point_df = (
        new_points_for_each_metering_point_df.withColumnRenamed(
            "MeteringPointId", "pfemp_MeteringPointId"
        ).withColumnRenamed("Resolution", "pfemp_Resolution")
    )

    master_basis_data_renamed_df = master_basis_data_df.withColumnRenamed(
        "MeteringPointId", "master_MeteringPointId"
    ).withColumnRenamed("Resolution", "master_Resolution")

    return (
        new_points_for_each_metering_point_df.withColumn(
            Colname.quantity, col(Colname.quantity).cast(DecimalType(18, 6))
        )
        .join(
            master_basis_data_renamed_df,
            (
                master_basis_data_renamed_df["master_MeteringPointId"]
                == new_points_for_each_metering_point_df["pfemp_MeteringPointId"]
            )
            & (
                new_points_for_each_metering_point_df[Colname.observation_time]
                >= col("FromDate")
            )
            & (
                new_points_for_each_metering_point_df[Colname.observation_time]
                < col("ToDate")
            ),
            "left",
        )
        .select(
            "GridAreaCode",
            master_basis_data_renamed_df["master_MeteringPointId"].alias(
                "MeteringPointId"
            ),
            "Type",
            master_basis_data_renamed_df["master_Resolution"].alias("Resolution"),
            Colname.observation_time,
            "Quantity",
            "Quality",
            "EnergySupplierId",
            "BalanceResponsibleId",
        )
    )
