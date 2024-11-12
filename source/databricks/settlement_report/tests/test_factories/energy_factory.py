from dataclasses import dataclass
from datetime import datetime, timedelta
from decimal import Decimal

from pyspark.sql import SparkSession, DataFrame

from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from settlement_report_job.infrastructure.wholesale.data_values import (
    CalculationTypeDataProductValue,
    MeteringPointTypeDataProductValue,
    MeteringPointResolutionDataProductValue,
)
from settlement_report_job.infrastructure.wholesale.schemas.energy_v1 import energy_v1
from settlement_report_job.infrastructure.wholesale.schemas.energy_per_es_v1 import (
    energy_per_es_v1,
)


@dataclass
class EnergyTestDataSpec:
    """
    Data specification for creating energy test data.
    Time series points are create between from_date and to_date with the specified resolution.
    """

    calculation_id: str
    calculation_type: CalculationTypeDataProductValue
    calculation_period_start: datetime
    calculation_period_end: datetime
    calculation_version: int
    result_id: str
    grid_area_code: str
    metering_point_type: MeteringPointTypeDataProductValue
    settlement_method: str
    resolution: MeteringPointResolutionDataProductValue
    quantity: Decimal
    quantity_unit: str
    quantity_qualities: list[str]
    from_date: datetime
    to_date: datetime
    energy_supplier_id: str
    balance_responsible_party_id: str


def _get_base_energy_rows_from_spec(data_spec: EnergyTestDataSpec):
    rows = []
    resolution = (
        timedelta(hours=1) if data_spec.resolution == "PT1H" else timedelta(minutes=15)
    )
    current_time = data_spec.from_date
    while current_time < data_spec.to_date:
        rows.append(
            {
                DataProductColumnNames.calculation_id: data_spec.calculation_id,
                DataProductColumnNames.calculation_type: data_spec.calculation_type.value,
                DataProductColumnNames.calculation_period_start: data_spec.calculation_period_start,
                DataProductColumnNames.calculation_period_end: data_spec.calculation_period_end,
                DataProductColumnNames.calculation_version: data_spec.calculation_version,
                DataProductColumnNames.result_id: data_spec.result_id,
                DataProductColumnNames.grid_area_code: data_spec.grid_area_code,
                DataProductColumnNames.energy_supplier_id: data_spec.energy_supplier_id,
                DataProductColumnNames.balance_responsible_party_id: data_spec.balance_responsible_party_id,
                DataProductColumnNames.metering_point_type: data_spec.metering_point_type.value,
                DataProductColumnNames.settlement_method: data_spec.settlement_method,
                DataProductColumnNames.resolution: data_spec.resolution.value,
                DataProductColumnNames.time: current_time,
                DataProductColumnNames.quantity: data_spec.quantity,
                DataProductColumnNames.quantity_unit: data_spec.quantity_unit,
                DataProductColumnNames.quantity_qualities: data_spec.quantity_qualities,
            }
        )
        current_time += resolution

    return rows


def create_energy_per_es_v1(
    spark: SparkSession,
    data_spec: EnergyTestDataSpec,
) -> DataFrame:
    rows = _get_base_energy_rows_from_spec(data_spec)
    return spark.createDataFrame(rows, energy_per_es_v1)


def create_energy_v1(spark: SparkSession, data_spec: EnergyTestDataSpec) -> DataFrame:
    rows = _get_base_energy_rows_from_spec(data_spec)
    for row in rows:
        del row[DataProductColumnNames.energy_supplier_id]
        del row[DataProductColumnNames.balance_responsible_party_id]

    return spark.createDataFrame(rows, energy_v1)
