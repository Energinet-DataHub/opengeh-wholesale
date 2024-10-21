from dataclasses import dataclass
from datetime import datetime

from pyspark.sql import SparkSession, DataFrame

from settlement_report_job.wholesale.column_names import DataProductColumnNames
from settlement_report_job.wholesale.data_values import (
    CalculationTypeDataProductValue,
    ChargeTypeDataProductValue,
    ChargeResolutionDataProductValue,
)
from settlement_report_job.wholesale.schemas import (
    charge_price_information_periods_v1,
)


@dataclass
class ChargePriceInformationPeriodsRow:
    calculation_id: str
    calculation_type: CalculationTypeDataProductValue
    calculation_version: int
    charge_key: str
    charge_code: str
    charge_type: ChargeTypeDataProductValue
    charge_owner_id: str
    resolution: ChargeResolutionDataProductValue
    is_tax: bool
    from_date: datetime
    to_date: datetime


def create(
    spark: SparkSession,
    rows: ChargePriceInformationPeriodsRow | list[ChargePriceInformationPeriodsRow],
) -> DataFrame:
    if not isinstance(rows, list):
        rows = [rows]

    rows = []
    for data_spec in rows:
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
