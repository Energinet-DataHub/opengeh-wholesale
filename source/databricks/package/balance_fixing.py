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
    array,
    array_contains,
    lit,
    col,
    collect_set,
    row_number,
    expr,
    when,
    explode,
    sum,
)
from pyspark.sql.types import (
    DecimalType,
)
from pyspark.sql.window import Window
from package.codelists import (
    TimeSeriesQuality,
    MeteringPointResolution,
)

from package.db_logging import debug
import package.basis_data as basis_data
from datetime import timedelta, datetime
from decimal import Decimal


def calculate_balance_fixing(
    metering_points_periods_df: DataFrame,
    timeseries_points: DataFrame,
    batch_grid_areas_df: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
    time_zone: str,
) -> tuple[DataFrame, tuple[DataFrame, DataFrame], DataFrame]:

    _check_all_grid_areas_have_metering_points(
        batch_grid_areas_df, metering_points_periods_df
    )

    enriched_time_series_point_df = _get_enriched_time_series_points_df(
        timeseries_points,
        metering_points_periods_df,
        period_start_datetime,
        period_end_datetime,
    )

    time_series_basis_data_df = basis_data.get_time_series_basis_data_dfs(
        enriched_time_series_point_df, time_zone
    )

    output_master_basis_data_df = basis_data.get_master_basis_data_df(
        metering_points_periods_df, period_start_datetime, period_end_datetime
    )

    result_df = _get_result_df(enriched_time_series_point_df)

    return (result_df, time_series_basis_data_df, output_master_basis_data_df)


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


def _get_result_df(enriched_time_series_points_df: DataFrame) -> DataFrame:
    # Total production in batch grid areas with quarterly resolution per grid area
    result_df = (
        enriched_time_series_points_df.withColumn(
            "quarter_times",
            when(
                col("Resolution") == MeteringPointResolution.hour.value,
                array(
                    col("time"),
                    col("time") + expr("INTERVAL 15 minutes"),
                    col("time") + expr("INTERVAL 30 minutes"),
                    col("time") + expr("INTERVAL 45 minutes"),
                ),
            ).when(
                col("Resolution") == MeteringPointResolution.quarterly.value,
                array(col("time")),
            ),
        )
        .select(
            enriched_time_series_points_df["*"],
            explode("quarter_times").alias("quarter_time"),
        )
        .withColumn("Quantity", col("Quantity").cast(DecimalType(18, 6)))
        .withColumn(
            "quarter_quantity",
            when(
                col("Resolution") == MeteringPointResolution.hour.value,
                col("Quantity") / 4,
            ).when(
                col("Resolution") == MeteringPointResolution.quarterly.value,
                col("Quantity"),
            ),
        )
        .groupBy("GridAreaCode", "quarter_time")
        .agg(sum("quarter_quantity"), collect_set("Quality"))
        .withColumn(
            "Quality",
            when(
                array_contains(
                    col("collect_set(Quality)"), lit(TimeSeriesQuality.missing.value)
                ),
                lit(TimeSeriesQuality.missing.value),
            )
            .when(
                array_contains(
                    col("collect_set(Quality)"),
                    lit(TimeSeriesQuality.estimated.value),
                ),
                lit(TimeSeriesQuality.estimated.value),
            )
            .when(
                array_contains(
                    col("collect_set(Quality)"),
                    lit(TimeSeriesQuality.measured.value),
                ),
                lit(TimeSeriesQuality.measured.value),
            ),
        )
        .withColumnRenamed("Quality", "quality")
    )

    debug(
        "Pre-result split into quarter times",
        result_df.orderBy(col("GridAreaCode"), col("quarter_time")),
    )

    window = Window.partitionBy("GridAreaCode").orderBy(col("quarter_time"))

    # Points may be missing in result time series if all metering points are missing a point at a certain moment.
    # According to PO and SME we can for now assume that full time series have been submitted for the processes/tests in question.
    result_df = (
        result_df.withColumn("position", row_number().over(window))
        .withColumnRenamed("sum(quarter_quantity)", "Quantity")
        .withColumn(
            "Quantity",
            when(col("Quantity").isNull(), Decimal("0.000")).otherwise(col("Quantity")),
        )
        .withColumn(
            "quality",
            when(col("quality").isNull(), TimeSeriesQuality.missing.value).otherwise(
                col("quality")
            ),
        )
        .select(
            "GridAreaCode",
            col("Quantity").cast(DecimalType(18, 3)),
            col("quality"),
            "position",
            "quarter_time",
        )
    )

    debug(
        "Balance fixing total production result",
        result_df.orderBy(col("GridAreaCode"), col("position")),
    )
    return result_df
