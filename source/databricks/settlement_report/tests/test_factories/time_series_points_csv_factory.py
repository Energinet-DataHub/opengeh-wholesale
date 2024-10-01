from dataclasses import dataclass
from datetime import datetime, timedelta

from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import DecimalType

from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.infrastructure.column_names import (
    TimeSeriesPointCsvColumnNames,
)
from settlement_report_job.domain.metering_point_type import MeteringPointType
from typing import List


@dataclass
class TimeSeriesPointCsvTestDataSpec:
    metering_point_ids: List[str]
    metering_point_type: MeteringPointType
    start_of_day: datetime
    energy_quantity: float


def create(spark: SparkSession, data_spec: TimeSeriesPointCsvTestDataSpec) -> DataFrame:
    rows = []

    for metering_point_id in data_spec.metering_point_ids:
        row = {
            TimeSeriesPointCsvColumnNames.metering_point_id: metering_point_id,
            TimeSeriesPointCsvColumnNames.metering_point_type: data_spec.metering_point_type,
            TimeSeriesPointCsvColumnNames.start_of_day: data_spec.start_of_day,
        }
        for i in range(
            24
            if data_spec.resolution == DataProductMeteringPointResolution.HOUR
            else 96
        ):
            row[f"energy_quantity_{i+1}"] = data_spec.energy_quantity
        rows.append(row)

    df = spark.createDataFrame(rows)
    return df
