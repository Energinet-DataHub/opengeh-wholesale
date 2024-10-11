from dataclasses import dataclass
from datetime import datetime

from pyspark.sql import SparkSession, DataFrame

from settlement_report_job.domain.DataProductValues.calculation_type import (
    CalculationTypeDataProductValue,
)
from settlement_report_job.domain.DataProductValues.charge_resolution import (
    ChargeResolutionDataProductValue,
)
from settlement_report_job.domain.DataProductValues.charge_type import (
    ChargeTypeDataProductValue,
)
from settlement_report_job.domain.calculation_type import CalculationType
from settlement_report_job.domain.DataProductValues.metering_point_type import (
    MeteringPointTypeDataProductValue,
)
from settlement_report_job.infrastructure.column_names import DataProductColumnNames
from settlement_report_job.infrastructure.schemas.charge_price_information_periods_v1 import (
    charge_price_information_periods_v1,
)
from settlement_report_job.infrastructure.schemas.latest_calculations_by_day_v1 import (
    latest_calculations_by_day_v1,
)


@dataclass
class LatestCalculationsTestDataSpec:
    """
    Data specification for creating a latest_calculations_per_day test data.
    """

    calculation_id: str
    calculation_type: CalculationTypeDataProductValue
    calculation_version: int
    grid_area_code: str
    start_of_day: datetime


def create(
    spark: SparkSession,
    data_specs: LatestCalculationsTestDataSpec | list[LatestCalculationsTestDataSpec],
) -> DataFrame:
    if not isinstance(data_specs, list):
        data_specs = [data_specs]

    rows = []
    for data_spec in data_specs:
        rows.append(
            {
                DataProductColumnNames.calculation_id: data_spec.calculation_id,
                DataProductColumnNames.calculation_type: data_spec.calculation_type,
                DataProductColumnNames.calculation_version: data_spec.calculation_version,
                DataProductColumnNames.grid_area_code: data_spec.grid_area_code,
                DataProductColumnNames.start_of_day: data_spec.start_of_day,
            }
        )

    return spark.createDataFrame(rows, latest_calculations_by_day_v1)
