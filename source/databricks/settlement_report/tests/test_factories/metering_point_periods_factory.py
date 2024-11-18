from dataclasses import dataclass
from datetime import datetime

from pyspark.sql import SparkSession, DataFrame

from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from settlement_report_job.infrastructure.wholesale.data_values import (
    CalculationTypeDataProductValue,
    MeteringPointTypeDataProductValue,
    MeteringPointResolutionDataProductValue,
)
from settlement_report_job.infrastructure.wholesale.data_values.settlement_method import (
    SettlementMethodDataProductValue,
)
from settlement_report_job.infrastructure.wholesale.schemas import (
    metering_point_periods_v1,
)


@dataclass
class MeteringPointPeriodsRow:
    calculation_id: str
    calculation_type: CalculationTypeDataProductValue
    calculation_version: int
    metering_point_id: str
    metering_point_type: MeteringPointTypeDataProductValue
    settlement_method: SettlementMethodDataProductValue
    grid_area_code: str
    resolution: MeteringPointResolutionDataProductValue
    from_grid_area_code: str | None
    to_grid_area_code: str | None
    parent_metering_point_id: str | None
    energy_supplier_id: str
    balance_responsible_party_id: str
    from_date: datetime
    to_date: datetime


def create(
    spark: SparkSession,
    rows: MeteringPointPeriodsRow | list[MeteringPointPeriodsRow],
) -> DataFrame:
    if not isinstance(rows, list):
        rows = [rows]

    row_list = []
    for row in rows:
        row_list.append(
            {
                DataProductColumnNames.calculation_id: row.calculation_id,
                DataProductColumnNames.calculation_type: row.calculation_type.value,
                DataProductColumnNames.calculation_version: row.calculation_version,
                DataProductColumnNames.metering_point_id: row.metering_point_id,
                DataProductColumnNames.metering_point_type: row.metering_point_type.value,
                DataProductColumnNames.settlement_method: (
                    row.settlement_method.value if row.settlement_method else None
                ),
                DataProductColumnNames.grid_area_code: row.grid_area_code,
                DataProductColumnNames.resolution: row.resolution.value,
                DataProductColumnNames.from_grid_area_code: row.from_grid_area_code,
                DataProductColumnNames.to_grid_area_code: row.to_grid_area_code,
                DataProductColumnNames.parent_metering_point_id: row.parent_metering_point_id,
                DataProductColumnNames.energy_supplier_id: row.energy_supplier_id,
                DataProductColumnNames.balance_responsible_party_id: row.balance_responsible_party_id,
                DataProductColumnNames.from_date: row.from_date,
                DataProductColumnNames.to_date: row.to_date,
            }
        )

    return spark.createDataFrame(row_list, metering_point_periods_v1)
