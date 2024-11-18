from dataclasses import dataclass
from datetime import datetime
from decimal import Decimal

from pyspark.sql import SparkSession, DataFrame


from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from settlement_report_job.infrastructure.wholesale.data_values import (
    CalculationTypeDataProductValue,
    ChargeTypeDataProductValue,
)
from settlement_report_job.infrastructure.wholesale.schemas import (
    monthly_amounts_per_charge_v1,
)


@dataclass
class MonthlyAmountsPerChargeRow:
    """
    Data specification for creating wholesale test data.
    """

    calculation_id: str
    calculation_type: CalculationTypeDataProductValue
    calculation_version: int
    result_id: str
    grid_area_code: str
    energy_supplier_id: str
    charge_code: str
    charge_type: ChargeTypeDataProductValue
    charge_owner_id: str
    quantity_unit: str
    is_tax: bool
    currency: str
    time: datetime
    amount: Decimal


def create(spark: SparkSession, data_spec: MonthlyAmountsPerChargeRow) -> DataFrame:
    row = {
        DataProductColumnNames.calculation_id: data_spec.calculation_id,
        DataProductColumnNames.calculation_type: data_spec.calculation_type.value,
        DataProductColumnNames.calculation_version: data_spec.calculation_version,
        DataProductColumnNames.result_id: data_spec.result_id,
        DataProductColumnNames.grid_area_code: data_spec.grid_area_code,
        DataProductColumnNames.energy_supplier_id: data_spec.energy_supplier_id,
        DataProductColumnNames.charge_code: data_spec.charge_code,
        DataProductColumnNames.charge_type: data_spec.charge_type.value,
        DataProductColumnNames.charge_owner_id: data_spec.charge_owner_id,
        DataProductColumnNames.quantity_unit: data_spec.quantity_unit,
        DataProductColumnNames.is_tax: data_spec.is_tax,
        DataProductColumnNames.currency: data_spec.currency,
        DataProductColumnNames.time: data_spec.time,
        DataProductColumnNames.amount: data_spec.amount,
    }

    return spark.createDataFrame([row], monthly_amounts_per_charge_v1)
