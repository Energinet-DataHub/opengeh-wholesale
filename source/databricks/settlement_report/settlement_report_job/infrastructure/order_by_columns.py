from pyspark.sql import functions as F

from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.domain.utils.report_data_type import ReportDataType
from settlement_report_job.domain.csv_column_names import CsvColumnNames


def get_order_by_columns(
    report_data_type: ReportDataType, requesting_actor_market_role: MarketRole
) -> list[str]:
    if report_data_type in [
        ReportDataType.TimeSeriesHourly,
        ReportDataType.TimeSeriesQuarterly,
    ]:
        return _order_by_time_series(requesting_actor_market_role)
    elif report_data_type == ReportDataType.MeteringPointPeriods:
        return _order_by_metering_point_periods(requesting_actor_market_role)
    elif report_data_type == ReportDataType.ChargeLinks:
        return _get_order_by_columns_charge_links(requesting_actor_market_role)
    if report_data_type == ReportDataType.EnergyResults:
        return _order_by_energy_results(requesting_actor_market_role)
    elif report_data_type == ReportDataType.WholesaleResults:
        return _order_by_wholesale_results()
    elif report_data_type == ReportDataType.MonthlyAmounts:
        return _order_by_monthly_amounts(requesting_actor_market_role)
    else:
        raise ValueError(f"Unsupported report data type: {report_data_type}")


def _order_by_time_series(requesting_actor_market_role: MarketRole) -> list[str]:
    order_by_columns = [
        CsvColumnNames.metering_point_type,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.time,
    ]
    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        order_by_columns.insert(0, CsvColumnNames.energy_supplier_id)

    return order_by_columns


def _order_by_metering_point_periods(
    requesting_actor_market_role: MarketRole,
) -> list[str]:
    order_by_columns = [
        CsvColumnNames.grid_area_code_in_metering_points_csv,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.metering_point_from_date,
    ]
    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        order_by_columns.insert(1, CsvColumnNames.energy_supplier_id)

    return order_by_columns


def _get_order_by_columns_charge_links(
    requesting_actor_market_role: MarketRole,
) -> list[str]:
    order_by_columns = [
        CsvColumnNames.metering_point_type,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.charge_owner_id,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_link_from_date,
    ]
    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        order_by_columns.insert(0, CsvColumnNames.energy_supplier_id)

    return order_by_columns


def _order_by_energy_results(requesting_actor_market_role: MarketRole) -> list:
    order_by_columns = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.time,
    ]

    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        order_by_columns.insert(1, CsvColumnNames.energy_supplier_id)

    return order_by_columns


def _order_by_wholesale_results() -> list:
    return [
        F.col(CsvColumnNames.grid_area_code),
        F.col(CsvColumnNames.energy_supplier_id),
        F.col(CsvColumnNames.metering_point_type),
        F.col(CsvColumnNames.settlement_method),
        F.col(CsvColumnNames.time),
        F.col(CsvColumnNames.charge_owner_id),
        F.col(CsvColumnNames.charge_type),
        F.col(CsvColumnNames.charge_code),
    ]


def _order_by_monthly_amounts(requesting_actor_market_role: MarketRole) -> list[str]:
    order_by_columns = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_code,
        CsvColumnNames.resolution,
    ]

    if requesting_actor_market_role in [
        MarketRole.GRID_ACCESS_PROVIDER,
        MarketRole.SYSTEM_OPERATOR,
    ]:
        order_by_columns.insert(2, CsvColumnNames.charge_owner_id)

    return order_by_columns
