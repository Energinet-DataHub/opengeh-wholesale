from dataclasses import dataclass
from datetime import datetime, timedelta
from decimal import Decimal

from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import DecimalType, ArrayType, StringType

from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.infrastructure.column_names import DataProductColumnNames
from settlement_report_job.infrastructure.schemas.energy_v1 import (
    energy_v1,
)


@dataclass
class EnergyTestDataSpec:
    """
    Data specification for creating energy test data.
    Time series points are create between from_date and to_date with the specified resolution.
    """

    calculation_id: str
    calculation_type: str
    calculation_version: int
    result_id: str
    grid_area_code: str
    metering_point_type: str
    settlement_method: str
    resolution: str
    quantity: DecimalType(18, 3)
    from_date: datetime
    to_date: datetime


def create(spark: SparkSession, data_spec: EnergyTestDataSpec) -> DataFrame:
    rows = []
    resolution = (
        timedelta(hours=1)
        if data_spec.resolution == DataProductMeteringPointResolution.HOUR
        else timedelta(minutes=15)
    )
    current_time = data_spec.from_date
    while current_time < data_spec.to_date:
        rows.append(
            {
                DataProductColumnNames.calculation_id: data_spec.calculation_id,
                DataProductColumnNames.calculation_type: data_spec.calculation_type,
                DataProductColumnNames.calculation_version: data_spec.calculation_version,
                DataProductColumnNames.result_id: data_spec.result_id,
                DataProductColumnNames.grid_area_code: data_spec.grid_area_code,
                DataProductColumnNames.metering_point_type: data_spec.metering_point_type,
                DataProductColumnNames.settlement_method: data_spec.settlement_method,
                DataProductColumnNames.resolution: data_spec.resolution.value,
                DataProductColumnNames.time: current_time,
                DataProductColumnNames.quantity: data_spec.quantity,
            }
        )
        current_time += resolution

    return spark.createDataFrame(rows, energy_v1)
