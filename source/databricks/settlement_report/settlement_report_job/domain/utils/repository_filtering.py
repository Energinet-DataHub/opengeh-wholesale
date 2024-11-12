from datetime import datetime
from uuid import UUID

from pyspark.sql import DataFrame, functions as F

from settlement_report_job.domain.utils.factory_filters import (
    filter_by_calculation_id_by_grid_area,
    filter_by_charge_owner_and_tax_depending_on_market_role,
)
from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.infrastructure.repository import WholesaleRepository
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)


def read_metering_point_periods_by_calculation_ids(
    repository: WholesaleRepository,
    period_start: datetime,
    period_end: datetime,
    energy_supplier_ids: list[str] | None,
    calculation_id_by_grid_area: dict[str, UUID],
) -> DataFrame:
    metering_point_periods = repository.read_metering_point_periods().where(
        (F.col(DataProductColumnNames.from_date) < period_end)
        & (F.col(DataProductColumnNames.to_date) > period_start)
    )

    metering_point_periods = metering_point_periods.where(
        filter_by_calculation_id_by_grid_area(calculation_id_by_grid_area)
    )

    if energy_supplier_ids is not None:
        metering_point_periods = metering_point_periods.where(
            F.col(DataProductColumnNames.energy_supplier_id).isin(energy_supplier_ids)
        )

    return metering_point_periods


def read_filtered_metering_point_periods_by_grid_area_codes(
    repository: WholesaleRepository,
    period_start: datetime,
    period_end: datetime,
    energy_supplier_ids: list[str] | None,
    grid_area_codes: list[str],
) -> DataFrame:
    metering_point_periods = repository.read_metering_point_periods().where(
        (F.col(DataProductColumnNames.from_date) < period_end)
        & (F.col(DataProductColumnNames.to_date) > period_start)
    )

    metering_point_periods = metering_point_periods.where(
        F.col(DataProductColumnNames.grid_area_code).isin(grid_area_codes)
    )

    if energy_supplier_ids is not None:
        metering_point_periods = metering_point_periods.where(
            F.col(DataProductColumnNames.energy_supplier_id).isin(energy_supplier_ids)
        )

    return metering_point_periods


def read_charge_link_periods(
    repository: WholesaleRepository,
    period_start: datetime,
    period_end: datetime,
    charge_owner_id: str,
    requesting_actor_market_role: MarketRole,
) -> DataFrame:
    charge_link_periods = repository.read_charge_link_periods().where(
        (F.col(DataProductColumnNames.from_date) < period_end)
        & (F.col(DataProductColumnNames.to_date) > period_start)
    )

    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.GRID_ACCESS_PROVIDER,
    ]:
        charge_price_information_periods = (
            repository.read_charge_price_information_periods()
        )

        charge_link_periods = _filter_by_charge_owner_and_tax(
            charge_link_periods=charge_link_periods,
            charge_price_information_periods=charge_price_information_periods,
            charge_owner_id=charge_owner_id,
            requesting_actor_market_role=requesting_actor_market_role,
        )

    return charge_link_periods


def _filter_by_charge_owner_and_tax(
    charge_link_periods: DataFrame,
    charge_price_information_periods: DataFrame,
    charge_owner_id: str,
    requesting_actor_market_role: MarketRole,
) -> DataFrame:
    charge_price_information_periods = (
        filter_by_charge_owner_and_tax_depending_on_market_role(
            charge_price_information_periods,
            requesting_actor_market_role,
            charge_owner_id,
        )
    )

    charge_link_periods = charge_link_periods.join(
        charge_price_information_periods,
        on=[DataProductColumnNames.calculation_id, DataProductColumnNames.charge_key],
        how="inner",
    ).select(charge_link_periods["*"])

    return charge_link_periods
