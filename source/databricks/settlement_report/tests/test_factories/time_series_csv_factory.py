from dataclasses import dataclass, field
from datetime import datetime, timedelta

from pyspark.sql import SparkSession, DataFrame

from settlement_report_job.domain.DataProductValues.metering_point_resolution import (
    MeteringPointResolutionDataProductValue,
)
from settlement_report_job.infrastructure.column_names import (
    TimeSeriesPointCsvColumnNames,
    DataProductColumnNames,
)
from settlement_report_job.domain.DataProductValues.metering_point_type import (
    MeteringPointTypeDataProductValue,
)


DEFAULT_METERING_POINT_TYPE = MeteringPointTypeDataProductValue.CONSUMPTION
DEFAULT_START_OF_DAY = datetime(2024, 1, 1, 23)
DEFAULT_GRID_AREA_CODES = ["804"]
DEFAULT_ENERGY_QUANTITY = 235.0
DEFAULT_RESOLUTION = MeteringPointResolutionDataProductValue.HOUR
DEFAULT_NUM_METERING_POINTS = 10
DEFAULT_NUM_DAYS_PER_METERING_POINT = 1


@dataclass
class TimeSeriesCsvTestDataSpec:
    metering_point_type: MeteringPointTypeDataProductValue = DEFAULT_METERING_POINT_TYPE
    start_of_day: datetime = DEFAULT_START_OF_DAY
    grid_area_codes: list = field(default_factory=lambda: DEFAULT_GRID_AREA_CODES)
    energy_quantity: float = DEFAULT_ENERGY_QUANTITY
    resolution: MeteringPointResolutionDataProductValue = DEFAULT_RESOLUTION
    num_metering_points: int = DEFAULT_NUM_METERING_POINTS
    num_days_per_metering_point: int = DEFAULT_NUM_DAYS_PER_METERING_POINT


def create(spark: SparkSession, data_spec: TimeSeriesCsvTestDataSpec) -> DataFrame:
    rows = []
    counter = 0
    for grid_area_code in data_spec.grid_area_codes:
        for _ in range(data_spec.num_metering_points):
            counter += 1
            for i in range(data_spec.num_days_per_metering_point):
                row = {
                    TimeSeriesPointCsvColumnNames.metering_point_id: str(
                        1000000000000 + counter
                    ),
                    TimeSeriesPointCsvColumnNames.metering_point_type: data_spec.metering_point_type,
                    DataProductColumnNames.grid_area_code: grid_area_code,
                    TimeSeriesPointCsvColumnNames.start_of_day: data_spec.start_of_day
                    + timedelta(days=i),
                }
                for i in range(
                    25
                    if data_spec.resolution
                    == MeteringPointResolutionDataProductValue.HOUR
                    else 100
                ):
                    row[f"{TimeSeriesPointCsvColumnNames.energy_prefix}{i+1}"] = (
                        data_spec.energy_quantity
                    )
                rows.append(row)

    df = spark.createDataFrame(rows)
    return df
