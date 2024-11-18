from dataclasses import dataclass
from datetime import datetime
from decimal import Decimal
from typing import Union, List

from pyspark.sql import SparkSession, DataFrame


from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from settlement_report_job.infrastructure.wholesale.data_values import (
    CalculationTypeDataProductValue,
    ChargeTypeDataProductValue,
    ChargeResolutionDataProductValue,
    MeteringPointTypeDataProductValue,
)
from settlement_report_job.infrastructure.wholesale.data_values.settlement_method import (
    SettlementMethodDataProductValue,
)
from settlement_report_job.infrastructure.wholesale.schemas.amounts_per_charge_v1 import (
    amounts_per_charge_v1,
)


@dataclass
class AmountsPerChargeRow:
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


def create(
    spark: SparkSession,
    data_spec: Union[AmountsPerChargeRow, List[AmountsPerChargeRow]],
) -> DataFrame:
    if not isinstance(data_spec, list):
        data_specs = [data_spec]
    else:
        data_specs = data_spec

    rows = []
    for spec in data_specs:
        row = {
            DataProductColumnNames.calculation_id: spec.calculation_id,
            DataProductColumnNames.calculation_type: spec.calculation_type.value,
            DataProductColumnNames.calculation_version: spec.calculation_version,
            DataProductColumnNames.result_id: spec.result_id,
            DataProductColumnNames.grid_area_code: spec.grid_area_code,
            DataProductColumnNames.energy_supplier_id: spec.energy_supplier_id,
            DataProductColumnNames.charge_code: spec.charge_code,
            DataProductColumnNames.charge_type: spec.charge_type.value,
            DataProductColumnNames.charge_owner_id: spec.charge_owner_id,
            DataProductColumnNames.resolution: spec.resolution.value,
            DataProductColumnNames.quantity_unit: spec.quantity_unit,
            DataProductColumnNames.metering_point_type: spec.metering_point_type.value,
            DataProductColumnNames.settlement_method: getattr(
                spec.settlement_method, "value", None
            ),
            DataProductColumnNames.is_tax: spec.is_tax,
            DataProductColumnNames.currency: spec.currency,
            DataProductColumnNames.time: spec.time,
            DataProductColumnNames.quantity: spec.quantity,
            DataProductColumnNames.quantity_qualities: spec.quantity_qualities,
            DataProductColumnNames.price: spec.price,
            DataProductColumnNames.amount: spec.amount,
        }
        rows.append(row)

    return spark.createDataFrame(rows, amounts_per_charge_v1)
