from dataclasses import dataclass
from datetime import datetime, timedelta
from decimal import Decimal

from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import DecimalType

from settlement_report_job.domain.calculation_type import CalculationType
from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.domain.metering_point_type import MeteringPointType
from settlement_report_job.infrastructure.column_names import DataProductColumnNames
from settlement_report_job.infrastructure.schemas.metering_point_time_series_v1 import (
    metering_point_time_series_v1,
)

DEFAULT_PERIOD_START = datetime(2024, 1, 1, 22)
DEFAULT_PERIOD_END = datetime(2024, 1, 2, 22)
DEFAULT_CALCULATION_ID = "11111111-1111-1111-1111-111111111111"
DEFAULT_CALCULATION_VERSION = 1
DEFAULT_METERING_POINT_ID = "12345678-1111-1111-1111-111111111111"
DEFAULT_METERING_TYPE = MeteringPointType.CONSUMPTION
DEFAULT_GRID_AREA_CODE = "804"
DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"


@dataclass
class MeteringPointTimeSeriesTestDataSpec:
    """
    Data specification for creating a metering point time series test data.
    Time series points are create between from_date and to_date with the specified resolution.
    """

    calculation_id: str = DEFAULT_CALCULATION_ID
    calculation_type: CalculationType = CalculationType.WHOLESALE_FIXING
    calculation_version: int = DEFAULT_CALCULATION_VERSION
    metering_point_id: str = DEFAULT_METERING_POINT_ID
    metering_point_type: MeteringPointType = MeteringPointType.CONSUMPTION
    resolution: DataProductMeteringPointResolution = (
        DataProductMeteringPointResolution.HOUR
    )
    grid_area_code: str = DEFAULT_GRID_AREA_CODE
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID
    from_date: datetime = DEFAULT_PERIOD_START
    to_date: datetime = DEFAULT_PERIOD_END
    quantity: DecimalType(18, 3) = Decimal("1.005")


def create(
    spark: SparkSession, data_spec: MeteringPointTimeSeriesTestDataSpec
) -> DataFrame:
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
                DataProductColumnNames.calculation_type: data_spec.calculation_type.value,
                DataProductColumnNames.calculation_version: data_spec.calculation_version,
                DataProductColumnNames.metering_point_id: data_spec.metering_point_id,
                DataProductColumnNames.metering_point_type: data_spec.metering_point_type,
                DataProductColumnNames.resolution: data_spec.resolution.value,
                DataProductColumnNames.grid_area_code: data_spec.grid_area_code,
                DataProductColumnNames.energy_supplier_id: data_spec.energy_supplier_id,
                DataProductColumnNames.observation_time: current_time,
                DataProductColumnNames.quantity: data_spec.quantity,
            }
        )
        current_time += resolution

    return spark.createDataFrame(rows, metering_point_time_series_v1)
