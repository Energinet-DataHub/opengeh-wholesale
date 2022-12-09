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

from pyspark.sql.functions import (
    col,
    expr,
    when,
    explode,
    greatest,
    least,
)
from package.codelists import (
    ConnectionState,
    MeteringPointType,
    MeteringPointResolution,
)

from package.db_logging import debug
import package.basis_data as basis_data
import package.steps as steps
from datetime import timedelta, datetime


def calculate_balance_fixing_total_production(
    timeseries_points: DataFrame,
    metering_points_periods_df: DataFrame,
    market_roles_periods_df: DataFrame,
    batch_grid_areas_df: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
    time_zone: str,
) -> tuple[DataFrame, tuple[DataFrame, DataFrame], DataFrame]:

    master_basis_data_df = _get_master_basis_data_df(
        metering_points_periods_df,
        market_roles_periods_df,
        batch_grid_areas_df,
        period_start_datetime,
        period_end_datetime,
    )

    _check_all_grid_areas_have_metering_points(
        batch_grid_areas_df, master_basis_data_df
    )

    enriched_time_series_point_df = _get_enriched_time_series_points_df(
        timeseries_points,
        master_basis_data_df,
        period_start_datetime,
        period_end_datetime,
    )

    time_series_basis_data_df = basis_data.get_time_series_basis_data_dfs(
        enriched_time_series_point_df, time_zone
    )

    output_master_basis_data_df = basis_data.get_master_basis_data_df(
        master_basis_data_df, period_start_datetime, period_end_datetime
    )

    total_production_per_ga_df = steps.get_total_production_per_ga_df(
        enriched_time_series_point_df
    )

    return (
        total_production_per_ga_df,
        time_series_basis_data_df,
        output_master_basis_data_df,
    )


def _check_all_grid_areas_have_metering_points(
    batch_grid_areas_df: DataFrame, master_basis_data_df: DataFrame
) -> None:
    distinct_grid_areas_rows_df = master_basis_data_df.select("GridAreaCode").distinct()
    distinct_grid_areas_rows_df.show()
    grid_area_with_no_metering_point_df = batch_grid_areas_df.join(
        distinct_grid_areas_rows_df, "GridAreaCode", "leftanti"
    )

    if grid_area_with_no_metering_point_df.count() > 0:
        grid_areas_to_inform_about = grid_area_with_no_metering_point_df.select(
            "GridAreaCode"
        ).collect()

        grid_area_codes_to_inform_about = map(
            lambda x: x.__getitem__("GridAreaCode"), grid_areas_to_inform_about
        )
        raise Exception(
            f"There are no metering points for the grid areas {list(grid_area_codes_to_inform_about)} in the requested period"
        )


def _get_master_basis_data_df(
    metering_points_periods_df: DataFrame,
    market_roles_periods_df: DataFrame,
    grid_area_df: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:
    metering_points_in_grid_area = metering_points_periods_df.join(
        grid_area_df,
        metering_points_periods_df["GridArea"] == grid_area_df["GridAreaCode"],
        "inner",
    )

    metering_point_periods_df = (
        metering_points_in_grid_area.where(col("FromDate") < period_end_datetime)
        .where(col("ToDate") > period_start_datetime)
        .where(
            (col("ConnectionState") == ConnectionState.connected.value)
            | (col("ConnectionState") == ConnectionState.disconnected.value)
        )
        .where(col("MeteringPointType") == MeteringPointType.production.value)
    )

    market_roles_periods_df = market_roles_periods_df.where(
        col("FromDate") < period_end_datetime
    ).where(col("ToDate") > period_start_datetime)

    master_basis_data_df = (
        metering_point_periods_df.join(
            market_roles_periods_df,
            (
                metering_point_periods_df["MeteringPointId"]
                == market_roles_periods_df["MeteringPointId"]
            )
            & (
                market_roles_periods_df["FromDate"]
                < metering_point_periods_df["ToDate"]
            )
            & (
                metering_point_periods_df["FromDate"]
                < market_roles_periods_df["ToDate"]
            ),
            "left",
        )
        .withColumn(
            "EffectiveDate",
            greatest(
                metering_point_periods_df["FromDate"],
                market_roles_periods_df["FromDate"],
            ),
        )
        .withColumn(
            "toEffectiveDate",
            least(
                metering_point_periods_df["ToDate"], market_roles_periods_df["ToDate"]
            ),
        )
        .withColumn(
            "EffectiveDate",
            when(
                col("EffectiveDate") < period_start_datetime, period_start_datetime
            ).otherwise(col("EffectiveDate")),
        )
        .withColumn(
            "toEffectiveDate",
            when(
                col("toEffectiveDate") > period_end_datetime, period_end_datetime
            ).otherwise(col("toEffectiveDate")),
        )
    )

    master_basis_data_df = master_basis_data_df.select(
        metering_point_periods_df["MeteringPointId"],
        "GridAreaCode",
        "EffectiveDate",
        "toEffectiveDate",
        "MeteringPointType",
        "SettlementMethod",
        metering_point_periods_df["ToGridAreaCode"],
        metering_point_periods_df["FromGridAreaCode"],
        "Resolution",
        market_roles_periods_df["EnergySupplierId"],
    )
    debug(
        "Metering point events before join with grid areas",
        metering_point_periods_df.orderBy(col("FromDate").desc()),
    )

    debug(
        "Metering point periods",
        metering_point_periods_df.orderBy(
            col("GridAreaCode"), col("MeteringPointId"), col("FromDate")
        ),
    )
    return master_basis_data_df


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

    enriched_points_for_each_metering_point_df = (
        new_points_for_each_metering_point_df.join(
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
            master_basis_data_renamed_df["master_MeteringpointId"],
            "MeteringPointType",
            master_basis_data_renamed_df["master_Resolution"],
            "Time",
            "Quantity",
            "Quality",
        )
    )

    return enriched_points_for_each_metering_point_df.withColumnRenamed(
        "master_MeteringpointId", "MeteringpointId"
    ).withColumnRenamed("master_Resolution", "Resolution")
