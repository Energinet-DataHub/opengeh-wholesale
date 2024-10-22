from dataclasses import dataclass
from datetime import datetime
from decimal import Decimal

from pyspark.sql import SparkSession, DataFrame


from settlement_report_job.wholesale.column_names import DataProductColumnNames
from settlement_report_job.wholesale.data_values import (
    CalculationTypeDataProductValue,
    ChargeTypeDataProductValue,
    ChargeResolutionDataProductValue,
    MeteringPointTypeDataProductValue,
)
from settlement_report_job.wholesale.data_values.settlement_method import (
    SettlementMethodDataProductValue,
)
from settlement_report_job.wholesale.schemas.amounts_per_charge_v1 import (
    amounts_per_charge_v1,
)


@dataclass
class AmountsPerChargeTestDataSpec:
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
    resolution: ChargeResolutionDataProductValue
    quantity_unit: str
    metering_point_type: MeteringPointTypeDataProductValue
    settlement_method: SettlementMethodDataProductValue | None
    is_tax: bool
    currency: str
    time: datetime
    quantity: Decimal
    quantity_qualities: list[str]
    price: Decimal
    amount: Decimal


def _get_base_wholesale_rows_from_spec(data_spec: AmountsPerChargeTestDataSpec):
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
        DataProductColumnNames.resolution: data_spec.resolution.value,
        DataProductColumnNames.quantity_unit: data_spec.quantity_unit,
        DataProductColumnNames.metering_point_type: data_spec.metering_point_type.value,
        DataProductColumnNames.settlement_method: (
            data_spec.settlement_method.value
            if data_spec.settlement_method is not None
            else None
        ),
        DataProductColumnNames.is_tax: data_spec.is_tax,
        DataProductColumnNames.currency: data_spec.currency,
        DataProductColumnNames.time: data_spec.time,
        DataProductColumnNames.quantity: data_spec.quantity,
        DataProductColumnNames.quantity_qualities: data_spec.quantity_qualities,
        DataProductColumnNames.price: data_spec.price,
        DataProductColumnNames.amount: data_spec.amount,
    }

    return row


def create_amounts_per_charge(
    spark: SparkSession, data_spec: AmountsPerChargeTestDataSpec
) -> DataFrame:
    row = _get_base_wholesale_rows_from_spec(data_spec)
    return spark.createDataFrame([row], amounts_per_charge_v1)
