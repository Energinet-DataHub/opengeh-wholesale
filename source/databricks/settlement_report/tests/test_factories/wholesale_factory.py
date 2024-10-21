from dataclasses import dataclass
from datetime import datetime, timedelta
from decimal import Decimal

from pyspark.sql import SparkSession, DataFrame


from settlement_report_job.wholesale.column_names import DataProductColumnNames
from settlement_report_job.wholesale.schemas.amounts_per_charge_v1 import (
    amounts_per_charge_v1,
)


@dataclass
class WholesaleTestDataSpec:
    """
    Data specification for creating wholesale test data.
    Time series points are create between from_date and to_date with the specified resolution.
    """

    calculation_id: str
    calculation_type: str
    calculation_version: int
    result_id: str
    grid_area_code: str
    energy_supplier_id: str
    charge_code: str
    charge_type: str
    charge_owner_id: str
    resolution: str
    quantity_unit: str
    metering_point_type: str
    settlement_method: str
    is_tax: bool
    currency: str
    time: datetime
    quantity: Decimal
    quantity_qualities: list[str]
    price: Decimal
    amount: Decimal


def _get_base_wholesale_rows_from_spec(data_spec: WholesaleTestDataSpec):
    rows = []
    resolution = (
        timedelta(hours=1) if data_spec.resolution == "PT1H" else timedelta(minutes=15)
    )
    current_time = data_spec.from_date
    while current_time < data_spec.to_date:
        rows.append(
            {
                DataProductColumnNames.calculation_id: data_spec.calculation_id,
                DataProductColumnNames.calculation_type: data_spec.calculation_type,
                DataProductColumnNames.calculation_period_start: data_spec.calculation_period_start,
                DataProductColumnNames.calculation_period_end: data_spec.calculation_period_end,
                DataProductColumnNames.calculation_version: data_spec.calculation_version,
                DataProductColumnNames.result_id: data_spec.result_id,
                DataProductColumnNames.grid_area_code: data_spec.grid_area_code,
                DataProductColumnNames.energy_supplier_id: data_spec.energy_supplier_id,
                DataProductColumnNames.balance_responsible_party_id: data_spec.balance_responsible_party_id,
                DataProductColumnNames.metering_point_type: data_spec.metering_point_type,
                DataProductColumnNames.settlement_method: data_spec.settlement_method,
                DataProductColumnNames.resolution: data_spec.resolution,
                DataProductColumnNames.time: current_time,
                DataProductColumnNames.quantity: data_spec.quantity,
                DataProductColumnNames.quantity_unit: data_spec.quantity_unit,
                DataProductColumnNames.quantity_qualities: data_spec.quantity_qualities,
            }
        )
        current_time += resolution

    return rows


def create_wholesale(
    spark: SparkSession, data_spec: WholesaleTestDataSpec
) -> DataFrame:
    rows = _get_base_wholesale_rows_from_spec(data_spec)
    return spark.createDataFrame(rows, amounts_per_charge_v1)
