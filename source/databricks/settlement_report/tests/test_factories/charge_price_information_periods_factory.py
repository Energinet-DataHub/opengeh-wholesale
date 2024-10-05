from dataclasses import dataclass
from datetime import datetime

from pyspark.sql import SparkSession, DataFrame

from settlement_report_job.domain.DataProductValues.charge_resolution import (
    ChargeResolution,
)
from settlement_report_job.domain.DataProductValues.charge_type import ChargeType
from settlement_report_job.domain.calculation_type import CalculationType
from settlement_report_job.domain.DataProductValues.metering_point_type import (
    MeteringPointType,
)
from settlement_report_job.infrastructure.column_names import DataProductColumnNames
from settlement_report_job.infrastructure.schemas.charge_price_information_periods_v1 import (
    charge_price_information_periods_v1,
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
class ChargePriceInformationPeriodsTestDataSpec:
    """
    Data specification for creating a price information periods test data.
    """

    calculation_id: str = DEFAULT_CALCULATION_ID
    calculation_type: CalculationType = CalculationType.WHOLESALE_FIXING
    calculation_version: int = DEFAULT_CALCULATION_VERSION
    charge_key: str = DEFAULT_CHARGE_KEY
    charge_code: str = DEFAULT_CHARGE_CODE
    charge_type: ChargeType = DEFAULT_CHARGE_TYPE
    charge_owner_id: str = DEFAULT_CHARGE_OWNER_ID
    resolution: ChargeResolution = ChargeResolution.HOUR
    is_tax: bool = False
    from_date: datetime = DEFAULT_PERIOD_START
    to_date: datetime = DEFAULT_PERIOD_END


def create(
    spark: SparkSession,
    data_specs: (
        ChargePriceInformationPeriodsTestDataSpec
        | list[ChargePriceInformationPeriodsTestDataSpec]
    ),
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
                DataProductColumnNames.charge_key: data_spec.charge_key,
                DataProductColumnNames.charge_code: data_spec.charge_code,
                DataProductColumnNames.charge_type: data_spec.charge_type,
                DataProductColumnNames.charge_owner_id: data_spec.charge_owner_id,
                DataProductColumnNames.resolution: data_spec.resolution,
                DataProductColumnNames.is_tax: data_spec.is_tax,
                DataProductColumnNames.from_date: data_spec.from_date,
                DataProductColumnNames.to_date: data_spec.to_date,
            }
        )

    return spark.createDataFrame(rows, charge_price_information_periods_v1)
