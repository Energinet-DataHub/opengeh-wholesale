from dataclasses import dataclass, field
from datetime import datetime, timedelta

from pyspark.sql import SparkSession, DataFrame

from settlement_report_job.domain.utils.csv_column_names import (
    CsvColumnNames,
    EphemeralColumns,
)
from settlement_report_job.infrastructure.wholesale.data_values import (
    MeteringPointTypeDataProductValue,
    MeteringPointResolutionDataProductValue,
)

DEFAULT_METERING_POINT_TYPE = MeteringPointTypeDataProductValue.CONSUMPTION
DEFAULT_START_OF_DAY = datetime(2024, 1, 1, 23)
DEFAULT_GRID_AREA_CODES = ["804"]
DEFAULT_ENERGY_QUANTITY = 235.0
DEFAULT_RESOLUTION = MeteringPointResolutionDataProductValue.HOUR
DEFAULT_NUM_METERING_POINTS = 10
DEFAULT_NUM_DAYS_PER_METERING_POINT = 1


@dataclass
class TimeSeriesPointsCsvTestDataSpec:
    metering_point_type: MeteringPointTypeDataProductValue = DEFAULT_METERING_POINT_TYPE
    start_of_day: datetime = DEFAULT_START_OF_DAY
    grid_area_codes: list = field(default_factory=lambda: DEFAULT_GRID_AREA_CODES)
    energy_quantity: float = DEFAULT_ENERGY_QUANTITY
    resolution: MeteringPointResolutionDataProductValue = DEFAULT_RESOLUTION
    num_metering_points: int = DEFAULT_NUM_METERING_POINTS
    num_days_per_metering_point: int = DEFAULT_NUM_DAYS_PER_METERING_POINT


def create(
    spark: SparkSession,
    data_spec: TimeSeriesPointsCsvTestDataSpec,
    add_grid_area_code_partitioning_column: bool = False,
) -> DataFrame:
    rows = []
    counter = 0
    for grid_area_code in data_spec.grid_area_codes:
        for _ in range(data_spec.num_metering_points):
            counter += 1
            for i in range(data_spec.num_days_per_metering_point):
                row = {
                    CsvColumnNames.energy_supplier_id: "1234567890123",
                    CsvColumnNames.metering_point_id: str(1000000000000 + counter),
                    CsvColumnNames.metering_point_type: data_spec.metering_point_type.value,
                    EphemeralColumns.grid_area_code_partitioning: grid_area_code,
                    CsvColumnNames.time: data_spec.start_of_day + timedelta(days=i),
                }
                if add_grid_area_code_partitioning_column:
                    row[EphemeralColumns.grid_area_code_partitioning] = grid_area_code

                for j in range(
                    25
                    if data_spec.resolution.value
                    == MeteringPointResolutionDataProductValue.HOUR
                    else 100
                ):
                    row[f"{CsvColumnNames.energy_quantity}{j + 1}"] = (
                        data_spec.energy_quantity
                    )
                rows.append(row)

    df = spark.createDataFrame(rows)
    return df
