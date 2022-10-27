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
    date_format,
    udf,
    concat,
    struct,
    first,
    array,
    array_contains,
    lit,
    col,
    collect_set,
    from_json,
    to_date,
    from_utc_timestamp,
    row_number,
    expr,
    when,
    lead,
    last,
    coalesce,
    explode,
    collect_list,
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
    TimeSeriesResolution,
    TimeSeriesQuality,
    MeteringPointResolution,
)
from package.schemas import (
    grid_area_updated_event_schema,
    metering_point_generic_event_schema,
    energy_supplier_changed_event_schema,
)
from package.db_logging import debug
from datetime import datetime, timedelta, date
import time
from pytz import timezone
import pytz
from decimal import Decimal

energy_supplier_changed_message_type = "EnergySupplierChanged"
metering_point_created_message_type = "MeteringPointCreated"
metering_point_connected_message_type = "MeteringPointConnected"


def calculate_balance_fixing_total_production(
    raw_integration_events_df,
    raw_time_series_points_df,
    batch_id,
    batch_grid_areas_df,
    batch_snapshot_datetime,
    period_start_datetime,
    period_end_datetime,
    time_zone,
) -> DataFrame:
    "Returns tuple (result_df, (time_series_quarter_basis_data_df, time_series_hour_basis_data_df))"

    cached_integration_events_df = _get_cached_integration_events(
        raw_integration_events_df, batch_snapshot_datetime
    )

    time_series_points = _get_time_series_points(
        raw_time_series_points_df, batch_snapshot_datetime
    )

    grid_area_df = _get_grid_areas_df(cached_integration_events_df, batch_grid_areas_df)

    energy_supplier_changed_df = _get_energy_supplier_changed_df(
        cached_integration_events_df, period_start_datetime, period_end_datetime
    )

    metering_point_period_df = _get_metering_point_periods_df(
        cached_integration_events_df,
        energy_supplier_changed_df,
        grid_area_df,
        period_start_datetime,
        period_end_datetime,
    )

    _check_all_grid_areas_have_metering_points(
        batch_grid_areas_df, metering_point_period_df
    )

    enriched_time_series_point_df = _get_enriched_time_series_points_df(
        time_series_points,
        metering_point_period_df,
        period_start_datetime,
        period_end_datetime,
    )

    time_series_basis_data_df = _get_time_series_basis_data(
        enriched_time_series_point_df, time_zone
    )

    master_basis_data_df = _get_master_basis_data(metering_point_period_df)

    result_df = _get_result_df(enriched_time_series_point_df)

    cached_integration_events_df.unpersist()

    return (result_df, time_series_basis_data_df, master_basis_data_df)


def _check_all_grid_areas_have_metering_points(
    batch_grid_areas_df, metering_point_period_df
):
    distinct_grid_areas_rows_df = metering_point_period_df.select(
        "GridAreaCode"
    ).distinct()

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


def _get_grid_areas_df(cached_integration_events_df, batch_grid_areas_df) -> DataFrame:
    message_type = "GridAreaUpdated"  # Must correspond to the value stored by the integration event listener

    grid_area_events_df = (
        cached_integration_events_df.withColumn(
            "body", from_json(col("body"), grid_area_updated_event_schema)
        )
        .where(col("body.MessageType") == message_type)
        .select("body.GridAreaLinkId", "body.GridAreaCode", "body.OperationTime")
    ).join(
        batch_grid_areas_df,
        ["GridAreaCode"],
        "inner",
    )

    # Use latest update for the grid area
    window = Window.partitionBy("GridAreaCode").orderBy(col("OperationTime").desc())
    grid_area_df = (
        grid_area_events_df.withColumn("row", row_number().over(window))
        .filter(col("row") == 1)
        .select("GridAreaLinkId", "GridAreaCode")
    )

    if grid_area_df.count() != batch_grid_areas_df.count():
        raise Exception(
            "Grid areas for processes in batch does not match the known grid areas in wholesale"
        )

    debug("Grid areas", grid_area_df.orderBy(col("GridAreaCode")))
    return grid_area_df


def _get_energy_supplier_changed_df(
    cached_integration_events_df, period_start_datetime, period_end_datetime
) -> DataFrame:
    energy_supplier_changed_df = (
        cached_integration_events_df.withColumn(
            "body", from_json(col("body"), energy_supplier_changed_event_schema)
        )
        .where(
            col("body.MessageType").isin(
                energy_supplier_changed_message_type,
            )
        )
        .select(
            "body.AccountingpointId",
            "body.GsrnNumber",
            "body.EnergySupplierGln",
            "body.EffectiveDate",
            "body.Id",
            "body.CorrelationId",
            "body.MessageType",
            "body.OperationTime",
        )
    ).dropDuplicates(
        [
            "AccountingpointId",
            "GsrnNumber",
            "EnergySupplierGln",
            "EffectiveDate",
            "Id",
            "CorrelationId",
            "MessageType",
            "OperationTime",
        ]
    )

    return energy_supplier_changed_df


def _get_metering_point_periods_df(
    cached_integration_events_df,
    energy_supplier_changed_df,
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
            "body.SettlementMethod",
            "body.FromGridAreaCode",
            "body.ToGridAreaCode",
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
            "SettlementMethod",
            "FromGridAreaCode",
            "ToGridAreaCode",
            "AccountingpointId",
        ]
    )
    debug(
        "Metering point created and connected events without duplicates",
        metering_point_events_df.orderBy(col("storedTime").desc()),
    )

    window = Window.partitionBy("MeteringPointId").orderBy("EffectiveDate")

    metering_point_periods_df = (
        metering_point_events_df.withColumn(
            "toEffectiveDate",
            lead("EffectiveDate", 1, "3000-01-01T23:00:00.000+0000").over(window),
        )
        .withColumn(
            "GridAreaLinkId",
            coalesce(col("GridAreaLinkId"), last("GridAreaLinkId", True).over(window)),
        )
        .withColumn(
            "ConnectionState",
            when(
                col("MessageType") == metering_point_created_message_type,
                lit(ConnectionState.new.value),
            ).when(
                col("MessageType") == metering_point_connected_message_type,
                lit(ConnectionState.connected.value),
            ),
        )
        .withColumn(
            "MeteringPointType",
            coalesce(
                col("MeteringPointType"), last("MeteringPointType", True).over(window)
            ),
        )
        .withColumn(
            "Resolution",
            coalesce(col("Resolution"), last("Resolution", True).over(window)),
        )
        .withColumn(
            "SettlementMethod",
            coalesce(
                col("SettlementMethod"), last("SettlementMethod", True).over(window)
            ),
        )
        .withColumn(
            "FromGridAreaCode",
            coalesce(
                col("FromGridAreaCode"), last("FromGridAreaCode", True).over(window)
            ),
        )
        .withColumn(
            "ToGridAreaCode",
            coalesce(col("ToGridAreaCode"), last("ToGridAreaCode", True).over(window)),
        )
        .withColumn(
            "AccountingpointId",
            coalesce(
                col("AccountingpointId"), last("AccountingpointId", True).over(window)
            ),
        )
        .where(col("EffectiveDate") <= period_end_datetime)
        .where(col("toEffectiveDate") >= period_start_datetime)
        .where(
            col("ConnectionState") == ConnectionState.connected.value
        )  # Only aggregate when metering points is connected
        .where(col("MeteringPointType") == MeteringPointType.production.value)
    )

    debug(
        "Metering point events before join with grid areas",
        metering_point_periods_df.orderBy(col("storedTime").desc()),
    )

    # Only include metering points in the selected grid areas
    metering_point_periods_df = metering_point_periods_df.join(
        grid_area_df,
        metering_point_periods_df["GridAreaLinkId"] == grid_area_df["GridAreaLinkId"],
        "inner",
    ).select(
        "MeteringPointId",
        "GsrnNumber",
        "GridAreaCode",
        "EffectiveDate",
        "toEffectiveDate",
        "MeteringPointType",
        "SettlementMethod",
        "FromGridAreaCode",
        "ToGridAreaCode",
        "Resolution",
    )

    debug(
        "Metering point periods",
        metering_point_periods_df.orderBy(
            col("GridAreaCode"), col("GsrnNumber"), col("EffectiveDate")
        ),
    )

    metering_point_periods_df = metering_point_periods_df.join(
        energy_supplier_changed_df, "MeteringPointId" == "AccountingpointId"
    ).select(
        "GsrnNumber",
        "GridAreaCode",
        "EffectiveDate",
        "toEffectiveDate",
        "MeteringPointType",
        "SettlementMethod",
        "FromGridAreaCode",
        "ToGridAreaCode",
        "Resolution",
        "EnergySupplierGln",
    )

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

    quarterly_mp_df = metering_point_period_df.where(
        col("Resolution") == MeteringPointResolution.quarterly.value
    )
    hourly_mp_df = metering_point_period_df.where(
        col("Resolution") == MeteringPointResolution.hour.value
    )

    exclusive_period_end_datetime = period_end_datetime - timedelta(milliseconds=1)

    quarterly_times_df = (
        quarterly_mp_df.select("GsrnNumber")
        .distinct()
        .select(
            "GsrnNumber",
            expr(
                f"sequence(to_timestamp('{period_start_datetime}'), to_timestamp('{exclusive_period_end_datetime}'), interval 15 minutes)"
            ).alias("quarter_times"),
        )
        .select("GsrnNumber", explode("quarter_times").alias("time"))
    )

    hourly_times_df = (
        hourly_mp_df.select("GsrnNumber")
        .distinct()
        .select(
            "GsrnNumber",
            expr(
                f"sequence(to_timestamp('{period_start_datetime}'), to_timestamp('{exclusive_period_end_datetime}'), interval 1 hour)"
            ).alias("times"),
        )
        .select("GsrnNumber", explode("times").alias("time"))
    )

    empty_points_for_each_metering_point_df = quarterly_times_df.union(hourly_times_df)

    debug(
        "Time series points where time is within period",
        timeseries_df.orderBy(
            col("GsrnNumber"),
            col("time"),
            col("storedTime").desc(),
        ),
    )

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
        timeseries_df.orderBy(
            col("GsrnNumber"),
            col("time"),
            col("storedTime").desc(),
        ),
    )

    timeseries_df = timeseries_df.select(
        "GsrnNumber", "time", "Quantity", "Quality", "Resolution"
    )

    points_for_each_metering_point_df = empty_points_for_each_metering_point_df.join(
        timeseries_df, ["GsrnNumber", "time"], "left"
    )

    # the metering_point_period_df is allready used once when creating the empty_points_for_each_metering_point_df
    # rejoining metering_point_period_df with empty_points_for_each_metering_point_df requires the GsrNumber and
    # Resolution column must be renamed for the select to be succesfull.
    points_for_each_metering_point_df = (
        points_for_each_metering_point_df.withColumnRenamed(
            "GsrnNumber", "pfemp_GsrnNumber"
        ).withColumnRenamed("Resolution", "pfemp_Resolution")
    )

    enriched_points_for_each_metering_point_df = points_for_each_metering_point_df.join(
        metering_point_period_df,
        (
            metering_point_period_df["GsrnNumber"]
            == points_for_each_metering_point_df["pfemp_GsrnNumber"]
        )
        & (points_for_each_metering_point_df["time"] >= col("EffectiveDate"))
        & (points_for_each_metering_point_df["time"] < col("toEffectiveDate")),
        "left",
    ).select(
        "GridAreaCode",
        metering_point_period_df["GsrnNumber"],
        "MeteringPointType",
        metering_point_period_df["Resolution"],
        "time",
        "Quantity",
        "Quality",
    )

    debug(
        "Enriched time series points",
        timeseries_df.orderBy(col("GsrnNumber"), col("time")),
    )

    return enriched_points_for_each_metering_point_df


def _get_master_basis_data(metering_point_df):
    productionType = MeteringPointType.production.value

    return (
        metering_point_df.withColumn("ENERGYSUPPLIERID", lit(""))
        .withColumn("TYPEOFMP", when(col("MeteringPointType") == productionType, "E18"))
        .select(
            col("GridAreaCode"),  # column is only used for partitioning
            col("GsrnNumber").alias("METERINGPOINTID"),
            col("EffectiveDate").alias("VALIDFROM"),
            col("toEffectiveDate").alias("VALIDTO"),
            col("GridAreaCode").alias("GRIDAREA"),
            col("ToGridAreaCode").alias("TOGRIDAREA"),
            col("FromGridAreaCode").alias("FROMGRIDAREA"),
            col("TYPEOFMP"),
            col("SettlementMethod").alias("SETTLEMENTMETHOD"),
            col("ENERGYSUPPLIERID"),  # column is soley there for completness
        )
    )


def _get_time_series_basis_data(enriched_time_series_point_df, time_zone):
    "Returns tuple (time_series_quarter_basis_data, time_series_hour_basis_data)"

    time_series_quarter_basis_data_df = _get_time_series_basis_data_by_resolution(
        enriched_time_series_point_df,
        MeteringPointResolution.quarterly.value,
        time_zone,
    )

    time_series_hour_basis_data_df = _get_time_series_basis_data_by_resolution(
        enriched_time_series_point_df,
        MeteringPointResolution.hour.value,
        time_zone,
    )

    return (time_series_quarter_basis_data_df, time_series_hour_basis_data_df)


def _get_time_series_basis_data_by_resolution(
    enriched_time_series_point_df, resolution, time_zone
):
    w = Window.partitionBy("gsrnNumber", "localDate").orderBy("time")

    timeseries_basis_data_df = (
        enriched_time_series_point_df.where(col("Resolution") == resolution)
        .withColumn("localDate", to_date(from_utc_timestamp(col("time"), time_zone)))
        .withColumn("position", concat(lit("ENERGYQUANTITY"), row_number().over(w)))
        .withColumn("STARTDATETIME", first("time").over(w))
        .groupBy(
            "gsrnNumber",
            "localDate",
            "STARTDATETIME",
            "GridAreaCode",
            "MeteringPointType",
            "Resolution",
        )
        .pivot("position")
        .agg(first("Quantity"))
        .withColumnRenamed("gsrnNumber", "METERINGPOINTID")
        .withColumn(
            "TYPEOFMP",
            when(col("MeteringPointType") == MeteringPointType.production.value, "E18"),
        )
    )

    quantity_columns = _get_sorted_quantity_columns(timeseries_basis_data_df)
    timeseries_basis_data_df = timeseries_basis_data_df.select(
        "GridAreaCode",
        "METERINGPOINTID",
        "TYPEOFMP",
        "STARTDATETIME",
        *quantity_columns,
    )
    return timeseries_basis_data_df


def _get_sorted_quantity_columns(timeseries_basis_data):
    def num_sort(col_name):
        "Extracts the nuber in the string"
        import re

        return list(map(int, re.findall(r"\d+", col_name)))[0]

    quantity_columns = [
        c for c in timeseries_basis_data.columns if c.startswith("ENERGYQUANTITY")
    ]
    quantity_columns.sort(key=num_sort)
    return quantity_columns


def _get_result_df(enriched_time_series_points_df) -> DataFrame:
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
                    lit(TimeSeriesQuality.measured.value),
                ),
                lit(Quality.measured.value),
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
            when(col("quality").isNull(), Quality.incomplete.value).otherwise(
                col("quality")
            ),
        )
        .select(
            "GridAreaCode",
            col("Quantity").cast(DecimalType(18, 3)),
            col("quality"),
            "position",
        )
    )

    debug(
        "Balance fixing total production result",
        result_df.orderBy(col("GridAreaCode"), col("position")),
    )
    return result_df
