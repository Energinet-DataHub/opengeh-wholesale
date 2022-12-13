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

from pyspark.sql import DataFrame

from pyspark.sql.functions import col, expr, explode, lit
from package.codelists import (
    MeteringPointResolution,
)

from package.db_logging import debug
import package.basis_data as basis_data
import package.steps as steps
import package.steps.aggregation as agg_steps
from datetime import timedelta, datetime
from geh_stream.codelists import ResultKeyName
from package.shared.data_classes import Metadata
from pyspark.sql.types import (
    DecimalType,
)


def calculate_balance_fixing(
    metering_points_periods_df: DataFrame,
    timeseries_points: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
    time_zone: str,
) -> tuple[DataFrame, tuple[DataFrame, DataFrame], DataFrame]:

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

    total_production_per_ga_df = steps.get_total_production_per_ga_df(
        enriched_time_series_point_df
    )
    total_production_per_ga_df.show(1000, False)
    results = {}
    results[ResultKeyName.aggregation_base_dataframe] = (
        enriched_time_series_point_df.withColumn(
            "Quantity", col("Quantity").cast(DecimalType(18, 6))
        )
        .withColumn(
            "BalanceResponsibleId",
            lit("1"),  # this is not the corect value, so this need to be changed
        )
        .withColumn(
            "EnergySupplierId",
            lit("1"),  # this is not the corect value, so this need to be changed
        )
        .withColumn(
            "aggregated_quality",
            col("Quality"),  # this is not the corect value, so this need to be changed
        )
    )
    metadata_fake = Metadata("1", "1", "1", "1", "1")
    total_production_per_ga_df_agg = agg_steps.aggregate_hourly_production(
        results, metadata_fake
    )
    total_production_per_ga_df_agg = total_production_per_ga_df_agg.select(
        "GridAreaCode", "sum_quantity", "Quality", "time_window"
    ).orderBy(col("GridAreaCode").asc())
    total_production_per_ga_df_agg.show(1000, False)

    return (
        total_production_per_ga_df,
        time_series_basis_data_df,
        master_basis_data_df,
    )


def _get_enriched_time_series_points_df(
    new_timeseries_df: DataFrame,
    master_basis_data_df: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:
    new_timeseries_df = new_timeseries_df.where(
        col("Time") >= period_start_datetime
    ).where(col("Time") < period_end_datetime)

    quarterly_mp_df = master_basis_data_df.where(
        col("Resolution") == MeteringPointResolution.quarterly.value
    )
    hourly_mp_df = master_basis_data_df.where(
        col("Resolution") == MeteringPointResolution.hour.value
    )

    exclusive_period_end_datetime = period_end_datetime - timedelta(milliseconds=1)

    quarterly_times_df = (
        quarterly_mp_df.select("MeteringPointId")
        .distinct()
        .select(
            "MeteringPointId",
            expr(
                f"sequence(to_timestamp('{period_start_datetime}'), to_timestamp('{exclusive_period_end_datetime}'), interval 15 minutes)"
            ).alias("quarter_times"),
        )
        .select("MeteringPointId", explode("quarter_times").alias("Time"))
    )

    hourly_times_df = (
        hourly_mp_df.select("MeteringPointId")
        .distinct()
        .select(
            "MeteringPointId",
            expr(
                f"sequence(to_timestamp('{period_start_datetime}'), to_timestamp('{exclusive_period_end_datetime}'), interval 1 hour)"
            ).alias("times"),
        )
        .select("MeteringPointId", explode("times").alias("Time"))
    )

    empty_points_for_each_metering_point_df = quarterly_times_df.union(hourly_times_df)

    debug(
        "Time series points where time is within period",
        new_timeseries_df.orderBy(col("MeteringPointId"), col("Time")),
    )

    new_timeseries_df = new_timeseries_df.select(
        "MeteringPointId", "Time", "Quantity", "Quality"
    ).withColumnRenamed("Time", "time")

    new_points_for_each_metering_point_df = (
        empty_points_for_each_metering_point_df.join(
            new_timeseries_df, ["MeteringPointId", "Time"], "left"
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
        "MeteringPointId", "master_MeteringpointId"
    ).withColumnRenamed("Resolution", "master_Resolution")

    return new_points_for_each_metering_point_df.join(
        master_basis_data_renamed_df,
        (
            master_basis_data_renamed_df["master_MeteringpointId"]
            == new_points_for_each_metering_point_df["pfemp_MeteringPointId"]
        )
        & (new_points_for_each_metering_point_df["time"] >= col("EffectiveDate"))
        & (new_points_for_each_metering_point_df["time"] < col("toEffectiveDate")),
        "left",
    ).select(
        "GridAreaCode",
        master_basis_data_renamed_df["master_MeteringpointId"].alias("MeteringpointId"),
        "MeteringPointType",
        master_basis_data_renamed_df["master_Resolution"].alias("Resolution"),
        "Time",
        "Quantity",
        "Quality",
    )
