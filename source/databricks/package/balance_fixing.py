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

from pyspark.sql.functions import col, expr, explode, sum, first, lit
from package.codelists import (
    MeteringPointResolution,
)

from package.db_logging import debug
import package.basis_data as basis_data
import package.steps as steps
import package.steps.aggregation as agg_steps
from datetime import timedelta, datetime
from package.constants import Colname, ResultKeyName
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

    results = {}
    results[ResultKeyName.aggregation_base_dataframe] = enriched_time_series_point_df
    metadata_fake = Metadata("1", "1", "1", "1")
    total_production_per_ga_df = agg_steps.aggregate_production(results, metadata_fake)
    total_production_per_ga_df = (
        total_production_per_ga_df.groupBy(Colname.grid_area, Colname.time_window)
        .agg(
            sum(Colname.sum_quantity).alias(Colname.quantity),
            first(Colname.quality).alias(Colname.quality),
        )
        .select(
            Colname.grid_area,
            Colname.quantity,
            col(Colname.quality).alias("quality"),
            col(Colname.time_window_start).alias("quarter_time"),
        )
        .orderBy(col(Colname.grid_area).asc(), col(Colname.time_window).asc())
        .withColumn("gln", lit("grid_area"))
        .withColumn("step", lit("production"))
    )

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
