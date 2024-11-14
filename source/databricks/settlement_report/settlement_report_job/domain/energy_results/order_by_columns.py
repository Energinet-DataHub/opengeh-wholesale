from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.domain.utils.csv_column_names import CsvColumnNames


def order_by_columns(requesting_actor_market_role: MarketRole) -> list:
    order_by_column_names = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.time,
    ]

    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        order_by_column_names.insert(1, CsvColumnNames.energy_supplier_id)

    return order_by_column_names
