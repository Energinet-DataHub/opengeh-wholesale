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

import package.basis_data as basis_data
import package.steps.aggregation as agg_steps
from package.codelists import MeteringPointResolution
from package.constants import Colname, ResultKeyName
from package.constants.result_grouping import ResultGrouping
from package.constants.time_series_type import TimeSeriesType
from package.db_logging import debug
from package.process_result_writer import ProcessResultWriter
from package.shared.data_classes import Metadata
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, explode, expr, lit, first
from pyspark.sql.types import DecimalType


def calculate_balance_fixing(
    process_result_writer: ProcessResultWriter,
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

    time_series_basis_data_df = basis_data.get_time_series_basis_data_dfs(
        enriched_time_series_point_df, time_zone
    )

    master_basis_data_df = basis_data.get_master_basis_data_df(
        metering_points_periods_df, period_start_datetime, period_end_datetime
    )

    metadata_fake = Metadata("1", "1", "1", "1")

    results = {}
    results[ResultKeyName.aggregation_base_dataframe] = enriched_time_series_point_df
    results[
        ResultKeyName.non_profiled_consumption
    ] = agg_steps.aggregate_non_profiled_consumption(results, metadata_fake)

    # Non-profiled consumption per energy supplier
    consumption_per_ga_and_es = agg_steps.aggregate_non_profiled_consumption_ga_es(
        results, metadata_fake
    )

    consumption_per_ga_and_es = _prepare_result_for_output(
        consumption_per_ga_and_es,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        ResultGrouping.PER_ENERGY_SUPPLIER,
    )

    # Total production per grid
    total_production_per_per_ga_and_brp_and_es = agg_steps.aggregate_production(
        results, metadata_fake
    )
    # Sum within a grid area
    total_production_per_ga_df = total_production_per_per_ga_and_brp_and_es.groupBy(
        Colname.grid_area, Colname.time_window
    ).agg(
        sum(Colname.sum_quantity).alias(Colname.sum_quantity),
        first(Colname.quality).alias(Colname.quality),
    )

    total_production_per_ga_df = _prepare_result_for_output(
        total_production_per_ga_df,
        TimeSeriesType.PRODUCTION,
        ResultGrouping.PER_GRID_AREA,
    )

    # Write to file(s)
    (timeseries_quarter_df, timeseries_hour_df) = time_series_basis_data_df
    process_result_writer.write_basis_data(
        master_basis_data_df, timeseries_quarter_df, timeseries_hour_df
    )
    process_result_writer.write_result(total_production_per_ga_df)
    process_result_writer.write_result(consumption_per_ga_and_es)


def _prepare_result_for_output(
    result_df: DataFrame,
    time_series_type: TimeSeriesType,
    result_grouping: ResultGrouping,
) -> DataFrame:

    result_df = _add_gln_and_time_series_type(
        result_df,
        result_grouping,
        time_series_type,
    )

    result_df = result_df.select(
        col(Colname.grid_area).alias("grid_area"),
        Colname.gln,
        Colname.time_series_type,
        col(Colname.sum_quantity).alias("quantity").cast("string"),
        col(Colname.quality).alias("quality"),
        col(Colname.time_window_start).alias("quarter_time"),
    )

    return result_df


def _add_gln_and_time_series_type(
    result_df: DataFrame,
    result_grouping: ResultGrouping,
    time_series_type: TimeSeriesType,
) -> DataFrame:

    result_df = result_df.withColumn(
        Colname.time_series_type, lit(time_series_type.value)
    )

    if result_grouping is ResultGrouping.PER_GRID_AREA:
        result_df = result_df.withColumn(Colname.gln, lit("grid_area"))
    elif result_grouping is ResultGrouping.PER_ENERGY_SUPPLIER:
        result_df = result_df.withColumnRenamed(Colname.energy_supplier_id, Colname.gln)
    else:
        raise NotImplementedError(
            f"Result grouping, {result_grouping}, is not supported yet"
        )

    return result_df


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
