from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.domain.utils.csv_column_names import CsvColumnNames


def order_by_columns(requesting_actor_market_role: MarketRole) -> list[str]:
    order_by_column_names = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_code,
        CsvColumnNames.resolution,
    ]

    if requesting_actor_market_role not in [
        MarketRole.GRID_ACCESS_PROVIDER,
        MarketRole.SYSTEM_OPERATOR,
    ]:
        order_by_column_names.insert(2, CsvColumnNames.charge_owner_id)

    return order_by_column_names
