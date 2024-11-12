from datetime import datetime
from uuid import UUID
from pyspark.sql import DataFrame, Column, functions as F

from telemetry_logging import Logger
from settlement_report_job.domain.utils.csv_column_names import EphemeralColumns
from settlement_report_job.domain.utils.get_start_of_day import (
    get_start_of_day,
)
from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from settlement_report_job.infrastructure.repository import (
    WholesaleRepository,
)
from settlement_report_job.infrastructure.wholesale.data_values.calculation_type import (
    CalculationTypeDataProductValue,
)

log = Logger(__name__)


def read_and_filter_by_latest_calculations(
    df: DataFrame,
    repository: WholesaleRepository,
    grid_area_codes: list[str],
    period_start: datetime,
    period_end: datetime,
    time_zone: str,
    time_column_name: str | Column,
) -> DataFrame:
    latest_balance_fixing_calculations = repository.read_latest_calculations().where(
        (
            F.col(DataProductColumnNames.calculation_type)
            == CalculationTypeDataProductValue.BALANCE_FIXING.value
        )
        & (F.col(DataProductColumnNames.grid_area_code).isin(grid_area_codes))
        & (F.col(DataProductColumnNames.start_of_day) >= period_start)
        & (F.col(DataProductColumnNames.start_of_day) < period_end)
    )

    df = filter_by_latest_calculations(
        df,
        latest_balance_fixing_calculations,
        df_time_column=time_column_name,
        time_zone=time_zone,
    )

    return df


def filter_by_latest_calculations(
    df: DataFrame,
    latest_calculations: DataFrame,
    df_time_column: str | Column,
    time_zone: str,
) -> DataFrame:
    df = df.withColumn(
        EphemeralColumns.start_of_day,
        get_start_of_day(df_time_column, time_zone),
    )

    return (
        df.join(
            latest_calculations,
            on=[
                df[DataProductColumnNames.calculation_id]
                == latest_calculations[DataProductColumnNames.calculation_id],
                df[DataProductColumnNames.grid_area_code]
                == latest_calculations[DataProductColumnNames.grid_area_code],
                df[EphemeralColumns.start_of_day]
                == latest_calculations[DataProductColumnNames.start_of_day],
            ],
            how="inner",
        )
        .select(df["*"])
        .drop(EphemeralColumns.start_of_day)
    )


def filter_by_calculation_id_by_grid_area(
    calculation_id_by_grid_area: dict[str, UUID],
) -> Column:
    calculation_id_by_grid_area_structs = [
        F.struct(F.lit(grid_area_code), F.lit(str(calculation_id)))
        for grid_area_code, calculation_id in calculation_id_by_grid_area.items()
    ]

    return F.struct(
        F.col(DataProductColumnNames.grid_area_code),
        F.col(DataProductColumnNames.calculation_id),
    ).isin(calculation_id_by_grid_area_structs)


def filter_by_energy_supplier_ids(energy_supplier_ids: list[str]) -> Column:
    return F.col(DataProductColumnNames.energy_supplier_id).isin(energy_supplier_ids)


def filter_by_grid_area_codes(grid_area_codes: list[str]) -> Column:
    return F.col(DataProductColumnNames.grid_area_code).isin(grid_area_codes)


def filter_by_charge_owner_and_tax_depending_on_market_role(
    df: DataFrame,
    requesting_actor_market_role: MarketRole,
    charge_owner_id: str,
) -> DataFrame:
    if requesting_actor_market_role == MarketRole.SYSTEM_OPERATOR:
        df = df.where(
            (F.col(DataProductColumnNames.charge_owner_id) == charge_owner_id)
            & (~F.col(DataProductColumnNames.is_tax))
        )

    if requesting_actor_market_role == MarketRole.GRID_ACCESS_PROVIDER:
        df = df.where(
            (
                (F.col(DataProductColumnNames.charge_owner_id) == charge_owner_id)
                & (~F.col(DataProductColumnNames.is_tax))
            )
            | (F.col(DataProductColumnNames.is_tax))
        )

    return df
