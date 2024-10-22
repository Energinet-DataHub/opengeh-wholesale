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
class ChargePriceInformationPeriodsTestDataSpec:
    """
    Data specification for creating a price information periods test data.
    """

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
                DataProductColumnNames.calculation_type: data_spec.calculation_type.value,
                DataProductColumnNames.calculation_version: data_spec.calculation_version,
                DataProductColumnNames.charge_key: data_spec.charge_key,
                DataProductColumnNames.charge_code: data_spec.charge_code,
                DataProductColumnNames.charge_type: data_spec.charge_type.value,
                DataProductColumnNames.charge_owner_id: data_spec.charge_owner_id,
                DataProductColumnNames.resolution: data_spec.resolution.value,
                DataProductColumnNames.is_tax: data_spec.is_tax,
                DataProductColumnNames.from_date: data_spec.from_date,
                DataProductColumnNames.to_date: data_spec.to_date,
            }
        )

    return spark.createDataFrame(rows, charge_price_information_periods_v1)
