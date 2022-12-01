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
    concat,
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
    sum,
    greatest,
    least,
)
from pyspark.sql.types import (
    StringType,
    DecimalType,
)
from pyspark.sql.window import Window
from package.codelists import (
    NewConnectionState,
    NewMeteringPointType,
    ConnectionState,
    MeteringPointType,
    Quality,
    TimeSeriesQuality,
    MeteringPointResolution,
)
from package.schemas import (
    grid_area_updated_event_schema,
    metering_point_generic_event_schema,
)
from package.db_logging import debug
from datetime import timedelta, datetime
from decimal import Decimal

ENERGY_SUPPLIER_CHANGED_MESSAGE_TYPE = "EnergySupplierChanged"
METERING_POINT_CREATED_MESSAGE_TYPE = "MeteringPointCreated"
METERING_POINT_CONNECTED_MESSAGE_TYPE = "MeteringPointConnected"


def calculate_balance_fixing_total_production(
    metering_points_periods_df: DataFrame,
    market_roles_periods_df: DataFrame,
    raw_integration_events_df: DataFrame,
    all_time_series_points_df: DataFrame,
    batch_grid_areas_df: DataFrame,
    batch_snapshot_datetime: datetime,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
    time_zone: str,
) -> tuple[DataFrame, tuple[DataFrame, DataFrame], DataFrame]:
    "Returns tuple (result_df, (time_series_quarter_basis_data_df, time_series_hour_basis_data_df))"
    "TODO: is this correct?"

    cached_integration_events_df = _get_cached_integration_events(
        raw_integration_events_df, batch_snapshot_datetime
    )

    time_series_points_df = _get_time_series_points_df(
        all_time_series_points_df, batch_snapshot_datetime
    )

    grid_area_df = _get_grid_areas_df(cached_integration_events_df, batch_grid_areas_df)

    master_basis_data_df = _get_master_basis_data_df(
        metering_points_periods_df,
        market_roles_periods_df,
        grid_area_df,
        period_start_datetime,
        period_end_datetime,
    )

    _check_all_grid_areas_have_metering_points(
        batch_grid_areas_df, master_basis_data_df
    )

    enriched_time_series_point_df = _get_enriched_time_series_points_df(
        time_series_points_df,
        master_basis_data_df,
        period_start_datetime,
        period_end_datetime,
    )

    time_series_basis_data_df = _get_time_series_basis_data(
        enriched_time_series_point_df, time_zone
    )

    output_master_basis_data_df = _get_output_master_basis_data_df(
        master_basis_data_df, period_start_datetime, period_end_datetime
    )

    result_df = _get_result_df(enriched_time_series_point_df)

    cached_integration_events_df.unpersist()

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


def _get_cached_integration_events(
    raw_integration_events_df: DataFrame, batch_snapshot_datetime: datetime
) -> DataFrame:
    return (
        raw_integration_events_df.where(col("storedTime") <= batch_snapshot_datetime)
        .withColumn("body", col("body").cast("string"))
        .cache()
    )


def _get_time_series_points_df(
    all_time_series_points_df: DataFrame, batch_snapshot_datetime: datetime
) -> DataFrame:
    return all_time_series_points_df.where(col("storedTime") <= batch_snapshot_datetime)


def _get_grid_areas_df(
    cached_integration_events_df: DataFrame, batch_grid_areas_df: DataFrame
) -> DataFrame:
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
            (col("ConnectionState") == NewConnectionState.connected.value)
            | (col("ConnectionState") == NewConnectionState.disconnected.value)
        )
        .where(col("MeteringPointType") == NewMeteringPointType.production.value)
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
    time_series_points: DataFrame,
    master_basis_data_df: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:

    timeseries_df = time_series_points.where(
        col("time") >= period_start_datetime
    ).where(col("time") < period_end_datetime)

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
        .select("MeteringPointId", explode("quarter_times").alias("time"))
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
        .select("MeteringPointId", explode("times").alias("time"))
    )

    empty_points_for_each_metering_point_df = quarterly_times_df.union(hourly_times_df)

    debug(
        "Time series points where time is within period",
        timeseries_df.orderBy(
            col("MeteringPointId"),
            col("time"),
            col("storedTime").desc(),
        ),
    )

    # Only use latest registered points
    window = Window.partitionBy("MeteringPointId", "time").orderBy(
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
            col("MeteringPointId"),
            col("time"),
            col("storedTime").desc(),
        ),
    )

    timeseries_df = timeseries_df.select(
        "MeteringPointId", "time", "Quantity", "Quality", "Resolution"
    )

    points_for_each_metering_point_df = empty_points_for_each_metering_point_df.join(
        timeseries_df, ["MeteringPointId", "time"], "left"
    )

    # the master_basis_data_df is allready used once when creating the empty_points_for_each_metering_point_df
    # rejoining master_basis_data_df with empty_points_for_each_metering_point_df requires the GsrNumber and
    # Resolution column must be renamed for the select to be succesfull.
    points_for_each_metering_point_df = (
        points_for_each_metering_point_df.withColumnRenamed(
            "MeteringPointId", "pfemp_MeteringPointId"
        ).withColumnRenamed("Resolution", "pfemp_Resolution")
    )

    master_basis_data_renamed_df = master_basis_data_df.withColumnRenamed(
        "MeteringPointId", "master_MeteringpointId"
    ).withColumnRenamed("Resolution", "master_Resolution")

    enriched_points_for_each_metering_point_df = points_for_each_metering_point_df.join(
        master_basis_data_renamed_df,
        (
            master_basis_data_renamed_df["master_MeteringpointId"]
            == points_for_each_metering_point_df["pfemp_MeteringPointId"]
        )
        & (points_for_each_metering_point_df["time"] >= col("EffectiveDate"))
        & (points_for_each_metering_point_df["time"] < col("toEffectiveDate")),
        "left",
    ).select(
        "GridAreaCode",
        master_basis_data_renamed_df["master_MeteringpointId"],
        "MeteringPointType",
        master_basis_data_renamed_df["master_Resolution"],
        "time",
        "Quantity",
        "Quality",
    )

    debug(
        "Enriched time series points",
        timeseries_df.orderBy(col("MeteringPointId"), col("time")),
    )

    return enriched_points_for_each_metering_point_df.withColumnRenamed(
        "master_MeteringpointId", "MeteringpointId"
    ).withColumnRenamed("master_Resolution", "Resolution")


def _get_output_master_basis_data_df(
    metering_point_df: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:
    productionType = MeteringPointType.production.value
    return (
        metering_point_df.withColumn(
            "TYPEOFMP", when(col("MeteringPointType") == productionType, "E18")
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
        .select(
            col("GridAreaCode"),  # column is only used for partitioning
            col("MeteringPointId").alias("METERINGPOINTID"),
            col("EffectiveDate").alias("VALIDFROM"),
            col("toEffectiveDate").alias("VALIDTO"),
            col("GridAreaCode").alias("GRIDAREA"),
            col("ToGridAreaCode").alias("TOGRIDAREA"),
            col("FromGridAreaCode").alias("FROMGRIDAREA"),
            col("TYPEOFMP"),
            col("SettlementMethod").alias("SETTLEMENTMETHOD"),
            col("EnergySupplierId").alias(("ENERGYSUPPLIERID")),
        )
    )


def _get_time_series_basis_data(
    enriched_time_series_point_df: DataFrame, time_zone: str
) -> tuple[DataFrame, DataFrame]:
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
    enriched_time_series_point_df: DataFrame,
    resolution: int,
    time_zone: str,
) -> DataFrame:
    w = Window.partitionBy("MeteringPointId", "localDate").orderBy("time")

    timeseries_basis_data_df = (
        enriched_time_series_point_df.where(col("Resolution") == resolution)
        .withColumn("localDate", to_date(from_utc_timestamp(col("time"), time_zone)))
        .withColumn("position", concat(lit("ENERGYQUANTITY"), row_number().over(w)))
        .withColumn("STARTDATETIME", first("time").over(w))
        .groupBy(
            "MeteringPointId",
            "localDate",
            "STARTDATETIME",
            "GridAreaCode",
            "MeteringPointType",
            "Resolution",
        )
        .pivot("position")
        .agg(first("Quantity"))
        .withColumnRenamed("MeteringPointId", "METERINGPOINTID")
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


def _get_sorted_quantity_columns(timeseries_basis_data: DataFrame) -> list[str]:
    def num_sort(col_name: str) -> int:
        "Extracts the nuber in the string"
        import re

        return list(map(int, re.findall(r"\d+", col_name)))[0]

    quantity_columns = [
        c for c in timeseries_basis_data.columns if c.startswith("ENERGYQUANTITY")
    ]
    quantity_columns.sort(key=num_sort)
    return quantity_columns


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
            "quarter_time",
        )
    )

    debug(
        "Balance fixing total production result",
        result_df.orderBy(col("GridAreaCode"), col("position")),
    )
    return result_df
