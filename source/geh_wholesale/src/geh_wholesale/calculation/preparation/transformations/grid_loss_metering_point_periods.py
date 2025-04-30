from datetime import datetime

from pyspark.sql import DataFrame
from pyspark.sql import functions as F
from pyspark.sql.functions import col
from pyspark.sql.window import Window

from geh_wholesale.calculation.preparation.data_structures.grid_loss_metering_point_periods import (
    GridLossMeteringPointPeriods,
)
from geh_wholesale.constants import Colname
from geh_wholesale.databases import wholesale_internal


def get_grid_loss_metering_point_periods(
    grid_areas: list[str],
    metering_point_periods_df: DataFrame,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
    repository: wholesale_internal.WholesaleInternalRepository,
) -> GridLossMeteringPointPeriods:
    grid_loss_metering_point_periods = (
        repository.read_grid_loss_metering_point_ids()
        .join(
            metering_point_periods_df,
            Colname.metering_point_id,
            "inner",
        )
        .select(
            col(Colname.metering_point_id),
            col(Colname.grid_area_code),
            col(Colname.from_date),
            col(Colname.to_date),
            col(Colname.metering_point_type),
            col(Colname.energy_supplier_id),
            col(Colname.balance_responsible_party_id),
        )
        .where(F.col(Colname.grid_area_code).isin(grid_areas))
        .where(F.col(Colname.from_date) <= period_end_datetime)
        .where(F.col(Colname.to_date).isNull() | (F.col(Colname.to_date) >= period_start_datetime))
    )

    _throw_if_no_grid_loss_metering_point_periods_in_grid_area(
        grid_areas, grid_loss_metering_point_periods, period_start_datetime, period_end_datetime
    )

    return GridLossMeteringPointPeriods(grid_loss_metering_point_periods)


def _throw_if_no_grid_loss_metering_point_periods_in_grid_area(
    calculation_grid_areas: list[str], df: DataFrame, period_start_datetime: datetime, period_end_datetime: datetime
):
    df.show()
    # Sort within each grid_area and metering_point_type
    window = Window.partitionBy(Colname.grid_area_code, Colname.metering_point_type).orderBy(Colname.from_date)

    # Get the previous row's to_date within the same grid_area and metering_point_type
    df = df.withColumn("prev_to", F.lag(Colname.to_date).over(window))

    # Start new group if there's a gap
    df = df.withColumn(
        "has_gap",
        F.when(F.col("prev_to").isNull(), F.lit(0)).otherwise(
            (F.col(Colname.from_date) > F.col("prev_to")).cast("int")
        ),
    )

    df.show()

    # Find metering points without gaps
    grid_areas_with_coverage_df = (
        df.groupBy(Colname.grid_area_code, Colname.metering_point_type)
        .agg(
            F.sum("has_gap").alias("has_gaps"),
            F.min(Colname.from_date).alias("min_from_date"),
            F.max(Colname.to_date).alias("max_to_date"),
        )
        # Filter out grid areas with gaps
        .where(F.col("has_gaps") == F.lit(0))
        .where(F.col("min_from_date") <= F.lit(period_start_datetime))
        .where(F.col("max_to_date").isNull() | (F.col("max_to_date") >= F.lit(period_end_datetime)))
        .select(Colname.grid_area_code, Colname.metering_point_type)
        # Now select only grid areas with both positive and negative metering points
        .groupBy(Colname.grid_area_code)
        .agg(F.count("*").alias("count"))
        .where(F.col("count") == 2)
    )

    grid_areas_with_coverage_df.show()

    # Identify grid areas without gaps
    grid_areas_with_coverage = [row[Colname.grid_area_code] for row in grid_areas_with_coverage_df.collect()]

    # Find grid areas from input that are not in the DataFrame without gaps
    grid_areas_without_coverage = [
        grid_area for grid_area in calculation_grid_areas if grid_area not in grid_areas_with_coverage
    ]

    if grid_areas_without_coverage:
        raise ValueError(
            f"The following grid areas are missing positive or negative grid loss metering points: {', '.join(grid_areas_without_coverage)}"
        )
