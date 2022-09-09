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
    from_json,
    row_number,
    expr,
    when,
    lead,
    last,
    coalesce,
    explode,
    sum,
)
from pyspark.sql.types import (
    IntegerType,
    StructField,
    StringType,
    TimestampType,
    StructType,
    DecimalType,
)
from pyspark.sql.window import Window
from package.codelists import (
    ConnectionState,
    MeteringPointType,
    Quality,
    Resolution,
    TimeSeriesQuality,
)
from package.schemas import (
    grid_area_updated_event_schema,
    metering_point_generic_event_schema,
)
from package.db_logging import log, debug


metering_point_created_message_type = "MeteringPointCreated"
metering_point_connected_message_type = "MeteringPointConnected"


def calculate_balance_fixing_total_production(
    raw_integration_events_df,
    raw_time_series_points_df,
    batch_id,
    batch_grid_areas,
    batch_snapshot_datetime,
    period_start_datetime,
    period_end_datetime,
) -> DataFrame:
    log(f"Periodstarttime: {period_start_datetime}")
    log(f"Periodendtime: {period_end_datetime}")

    cached_integration_events_df = _get_cached_integration_events(
        raw_integration_events_df, batch_snapshot_datetime
    )
    time_series_points = _get_time_series_points(
        raw_time_series_points_df, batch_snapshot_datetime
    )

    grid_area_df = _get_grid_areas_df(cached_integration_events_df, batch_grid_areas)

    metering_point_period_df = _get_metering_point_periods_df(
        cached_integration_events_df,
        grid_area_df,
        period_start_datetime,
        period_end_datetime,
    )

    enriched_time_series_point_df = _get_enriched_time_series_points_df(
        time_series_points,
        metering_point_period_df,
        period_start_datetime,
        period_end_datetime,
    )

    result_df = _get_result_df(enriched_time_series_point_df)
    cached_integration_events_df.unpersist()

    return result_df


def _get_cached_integration_events(
    raw_integration_events_df, batch_snapshot_datetime
) -> DataFrame:
    return (
        raw_integration_events_df.where(col("storedTime") <= batch_snapshot_datetime)
        .withColumn("body", col("body").cast("string"))
        .cache()
    )


def _get_time_series_points(
    raw_time_series_points_df, batch_snapshot_datetime
) -> DataFrame:
    return raw_time_series_points_df.where(col("storedTime") <= batch_snapshot_datetime)


def _get_grid_areas_df(cached_integration_events_df, batch_grid_areas) -> DataFrame:
    message_type = "GridAreaUpdated"  # Must correspond to the value stored by the integration event listener

    grid_area_events_df = (
        cached_integration_events_df.withColumn(
            "body", from_json(col("body"), grid_area_updated_event_schema)
        )
        .where(col("body.MessageType") == message_type)
        .where(col("body.GridAreaCode").isin(batch_grid_areas))
    )

    # Use latest update for the grid area
    window = Window.partitionBy("body.GridAreaCode").orderBy(
        col("body.OperationTime").desc()
    )
    grid_area_df = (
        grid_area_events_df.withColumn("row", row_number().over(window))
        .filter(col("row") == 1)
        .drop("row")
        .select("body.GridAreaLinkId", "body.GridAreaCode")
    )

    if grid_area_df.count() != len(batch_grid_areas):
        raise Exception(
            "Grid areas for processes in batch does not match the known grid areas in wholesale"
        )

    log("Grid areas", grid_area_df)
    return grid_area_df


def _get_metering_point_periods_df(
    cached_integration_events_df,
    grid_area_df,
    period_start_datetime,
    period_end_datetime,
) -> DataFrame:
    metering_point_events_df = (
        cached_integration_events_df.withColumn(
            "body", from_json(col("body"), metering_point_generic_event_schema)
        ).where(
            col("body.MessageType").isin(
                metering_point_created_message_type,
                metering_point_connected_message_type,
            )
        )
        # If new properties to the Meteringpoints are added
        # Consider if they should be included in the 'dropDuplicates'
        # To remove events that could have been received multiple times
        .select(
            "storedTime",
            "body.MessageType",
            "body.MeteringPointId",
            "body.MeteringPointType",
            "body.GsrnNumber",
            "body.GridAreaLinkId",
            "body.ConnectionState",
            "body.EffectiveDate",
            "body.Resolution",
            "body.OperationTime",
        )
    ).dropDuplicates(
        [
            "MessageType",
            "MeteringPointId",
            "MeteringPointType",
            "GsrnNumber",
            "GridAreaLinkId",
            "ConnectionState",
            "EffectiveDate",
            "Resolution",
            "OperationTime",
        ]
    )
    debug(
        "Metering point created and connected events without duplicates",
        metering_point_events_df,
    )

    window = Window.partitionBy("MeteringPointId").orderBy("EffectiveDate")

    metering_point_periods_df = metering_point_events_df.withColumn(
        "toEffectiveDate",
        lead("EffectiveDate", 1, "3000-01-01T23:00:00.000+0000").over(window),
    )
    metering_point_periods_df = metering_point_periods_df.withColumn(
        "GridAreaLinkId",
        coalesce(col("GridAreaLinkId"), last("GridAreaLinkId", True).over(window)),
    )
    metering_point_periods_df = metering_point_periods_df.withColumn(
        "ConnectionState",
        when(
            col("MessageType") == metering_point_created_message_type,
            lit(ConnectionState.new.value),
        ).when(
            col("MessageType") == metering_point_connected_message_type,
            lit(ConnectionState.connected.value),
        ),
    )
    metering_point_periods_df = metering_point_periods_df.withColumn(
        "MeteringPointType",
        coalesce(
            col("MeteringPointType"), last("MeteringPointType", True).over(window)
        ),
    )
    metering_point_periods_df = metering_point_periods_df.withColumn(
        "Resolution",
        coalesce(col("Resolution"), last("Resolution", True).over(window)),
    )
    metering_point_periods_df = metering_point_periods_df.where(
        col("EffectiveDate") <= period_end_datetime
    )
    metering_point_periods_df = metering_point_periods_df.where(
        col("toEffectiveDate") >= period_start_datetime
    )
    metering_point_periods_df = metering_point_periods_df.where(
        col("ConnectionState") == ConnectionState.connected.value
    )  # Only aggregate when metering points is connected
    metering_point_periods_df = metering_point_periods_df.where(
        col("MeteringPointType") == MeteringPointType.production.value
    )

    debug(
        "Metering point events before join with grid areas", metering_point_periods_df
    )

    # Only include metering points in the selected grid areas
    metering_point_periods_df = metering_point_periods_df.join(
        grid_area_df,
        metering_point_periods_df["GridAreaLinkId"] == grid_area_df["GridAreaLinkId"],
        "inner",
    ).select("GsrnNumber", "GridAreaCode", "EffectiveDate", "toEffectiveDate")

    log("Metering point periods", metering_point_periods_df)
    return metering_point_periods_df


def _get_enriched_time_series_points_df(
    time_series_points,
    metering_point_period_df,
    period_start_datetime,
    period_end_datetime,
) -> DataFrame:

    timeseries_df = time_series_points.where(
        col("time") >= period_start_datetime
    ).where(col("time") < period_end_datetime)

    debug("Time series points where time is within period", timeseries_df)

    # Only use latest registered points
    window = Window.partitionBy("GsrnNumber", "time").orderBy(
        col("RegistrationDateTime").desc()
    )
    # If we end up with more than one point for the same Meteringpoint and "time".
    # We only need the latest point, this is essential to handle updates of points.
    timeseries_df = timeseries_df.withColumn(
        "row_number", row_number().over(window)
    ).where(col("row_number") == 1)

    debug(
        "Time series points with only latest points by registration date time",
        timeseries_df,
    )

    timeseries_df = timeseries_df.select(
        col("GsrnNumber"), "time", "Quantity", "Quality", "Resolution"
    )

    enriched_time_series_point_df = timeseries_df.join(
        metering_point_period_df,
        (metering_point_period_df["GsrnNumber"] == timeseries_df["GsrnNumber"])
        & (timeseries_df["time"] >= metering_point_period_df["EffectiveDate"])
        & (timeseries_df["time"] < metering_point_period_df["toEffectiveDate"]),
        "inner",
    ).select(
        "GridAreaCode",
        metering_point_period_df["GsrnNumber"],
        "Resolution",
        "time",
        "Quantity",
        "Quality",
    )

    debug("Enriched time series points", timeseries_df)

    return enriched_time_series_point_df


def _get_result_df(enriched_time_series_points_df) -> DataFrame:
    # Total production in batch grid areas with quarterly resolution per grid area
    result_df = (
        enriched_time_series_points_df.withColumn(
            "quarter_times",
            when(
                col("Resolution") == Resolution.hour.value,
                array(
                    col("time"),
                    col("time") + expr("INTERVAL 15 minutes"),
                    col("time") + expr("INTERVAL 30 minutes"),
                    col("time") + expr("INTERVAL 45 minutes"),
                ),
            ).when(col("Resolution") == Resolution.quarter.value, array(col("time"))),
        )
        .select(
            enriched_time_series_points_df["*"],
            explode("quarter_times").alias("quarter_time"),
        )
        .withColumn("Quantity", col("Quantity").cast(DecimalType(18, 6)))
        .withColumn(
            "quarter_quantity",
            when(
                col("Resolution") == Resolution.hour.value,
                col("Quantity") / 4,
            ).when(col("Resolution") == Resolution.quarter.value, col("Quantity")),
        )
        .groupBy("GridAreaCode", "quarter_time")
        .agg(sum("quarter_quantity"), collect_set("Quality"))
        .withColumn(
            "Quality",
            when(
                array_contains(
                    col("collect_set(Quality)"), lit(TimeSeriesQuality.incomplete.value)
                ),
                lit(Quality.incomplete.value),
            )
            .when(
                array_contains(
                    col("collect_set(Quality)"), lit(TimeSeriesQuality.estimated.value)
                ),
                lit(Quality.estimated.value),
            )
            .when(
                array_contains(
                    col("collect_set(Quality)"),
                    lit(TimeSeriesQuality.asProvided.value),
                ),
                lit(Quality.measured.value),
            ),
        )
        .withColumnRenamed("Quality", "quality")
    )

    debug("Pre-result split into quarter times", result_df)

    window = Window.partitionBy("GridAreaCode").orderBy(col("quarter_time"))

    # Points may be missing in result time series if all metering points are missing a point at a certain moment.
    # According to PO and SME we can for now assume that full time series have been submitted for the processes/tests in question.
    result_df = (
        result_df.withColumn("position", row_number().over(window))
        .withColumnRenamed("sum(quarter_quantity)", "Quantity")
        .select(
            "GridAreaCode",
            col("Quantity").cast(DecimalType(18, 3)),
            col("quality"),
            "position",
        )
    )

    log("Balance fixing total production result", result_df)
    return result_df
