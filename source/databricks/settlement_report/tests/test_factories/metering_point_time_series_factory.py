from dataclasses import dataclass
from datetime import datetime, timedelta

from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import DecimalType

from settlement_report_job.domain.calculation_type import CalculationType
from settlement_report_job.domain.DataProductValues.metering_point_resolution import (
    MeteringPointResolution,
)
from settlement_report_job.domain.DataProductValues.metering_point_type import (
    MeteringPointType,
)
from settlement_report_job.infrastructure.column_names import DataProductColumnNames
from settlement_report_job.infrastructure.schemas.metering_point_time_series_v1 import (
    metering_point_time_series_v1,
)


@dataclass
class MeteringPointTimeSeriesTestDataSpec:
    """
    Data specification for creating a metering point time series test data.
    Time series points are create between from_date and to_date with the specified resolution.
    """

    calculation_id: str
    calculation_type: CalculationType
    calculation_version: int
    metering_point_id: str
    metering_point_type: MeteringPointType
    resolution: MeteringPointResolution
    grid_area_code: str
    energy_supplier_id: str
    from_date: datetime
    to_date: datetime
    quantity: DecimalType(18, 3)


def create(
    spark: SparkSession, data_spec: MeteringPointTimeSeriesTestDataSpec
) -> DataFrame:
    rows = []
    resolution = (
        timedelta(hours=1)
        if data_spec.resolution == MeteringPointResolution.HOUR
        else timedelta(minutes=15)
    )
    current_time = data_spec.from_date
    while current_time < data_spec.to_date:
        rows.append(
            {
                DataProductColumnNames.calculation_id: data_spec.calculation_id,
                DataProductColumnNames.calculation_type: data_spec.calculation_type.value,
                DataProductColumnNames.calculation_version: data_spec.calculation_version,
                DataProductColumnNames.metering_point_id: data_spec.metering_point_id,
                DataProductColumnNames.metering_point_type: data_spec.metering_point_type,
                DataProductColumnNames.resolution: data_spec.resolution,
                DataProductColumnNames.grid_area_code: data_spec.grid_area_code,
                DataProductColumnNames.energy_supplier_id: data_spec.energy_supplier_id,
                DataProductColumnNames.observation_time: current_time,
                DataProductColumnNames.quantity: data_spec.quantity,
            }
        )
        current_time += resolution

    return spark.createDataFrame(rows, metering_point_time_series_v1)
