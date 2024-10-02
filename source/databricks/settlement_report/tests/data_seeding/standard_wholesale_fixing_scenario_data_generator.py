from datetime import datetime, timedelta

from pyspark.sql import SparkSession, DataFrame

import metering_point_time_series_factory
from settlement_report_job.domain.calculation_type import CalculationType
from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.domain.metering_point_type import MeteringPointType

GRID_AREAS = ["804", "805"]
CALCULATION_ID = "12345678-6f20-40c5-9a95-f419a1245d7e"
ENERGY_SUPPLIER_IDS = ["1000000000000", "2000000000000"]
FROM_DATE = datetime(2024, 1, 1, 23)
TO_DATE = FROM_DATE + timedelta(days=1)


def create_metering_point_time_series(spark: SparkSession) -> DataFrame:
    """
    Creates a DataFrame with metering point time series data for testing purposes.
    There is one row for each combination of resolution, grid area code, and energy supplier id.
    There is one calculation with two grid areas, and each grid area has two energy suppliers and each energy supplier
    has one metering point in the grid area
    """
    df = None
    count = 0
    for resolution in {
        DataProductMeteringPointResolution.HOUR,
        DataProductMeteringPointResolution.QUARTER,
    }:
        for grid_area_code in GRID_AREAS:
            for energy_supplier_id in ENERGY_SUPPLIER_IDS:
                count += 1
                data_spec = metering_point_time_series_factory.MeteringPointTimeSeriesTestDataSpec(
                    calculation_id=CALCULATION_ID,
                    calculation_type=CalculationType.WHOLESALE_FIXING,
                    calculation_version=1,
                    metering_point_id=str(1000000000000 + count),
                    metering_point_type=MeteringPointType.CONSUMPTION,
                    resolution=resolution,
                    grid_area_code=grid_area_code,
                    energy_supplier_id=energy_supplier_id,
                    from_date=FROM_DATE,
                    to_date=TO_DATE,
                )
                next_df = metering_point_time_series_factory.create(spark, data_spec)
                if df is None:
                    df = next_df
                else:
                    df = df.union(next_df)

    return df
