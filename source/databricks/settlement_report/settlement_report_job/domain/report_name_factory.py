from enum import Enum
from zoneinfo import ZoneInfo

from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs


class ReportDataType(Enum):
    TimeSeriesHourly = "time_series_hourly"
    TimeSeriesQuarterly = "time_series_quarterly"


class MarketRoleInFileName:
    ENERGY_SUPPLIER = "DDQ"
    GRID_ACCESS_PROVIDER = "DDM"


class FileNameFactory:
    def __init__(self, report_data_type: ReportDataType, args: SettlementReportArgs):
        self.report_data_type = report_data_type
        self.args = args

    def create(
        self, grid_area_code: str, energy_supplier: str | None, split_index: str = None
    ) -> str:
        if self.report_data_type in {
            ReportDataType.TimeSeriesHourly,
            ReportDataType.TimeSeriesQuarterly,
        }:
            return self._create_time_series_filename(
                grid_area_code, energy_supplier, split_index
            )

    def _create_time_series_filename(
        self,
        grid_area_code: str,
        energy_supplier_id: str,
        split_index: str = None,
    ) -> str:

        time_zone_info = ZoneInfo(self.args.time_zone)

        filename_parts = [
            self._get_post_fix(),
            grid_area_code,
            (
                self.args.requesters_id
                if self.args.requesters_market_role is MarketRole.GRID_ACCESS_PROVIDER
                else energy_supplier_id
            ),
            self._get_market_role_identifier(self.args.requesters_market_role),
            self.args.period_start.astimezone(time_zone_info).strftime("%d-%m-%Y"),
            self.args.period_end.astimezone(time_zone_info).strftime("%d-%m-%Y"),
            split_index,
        ]
        filename_parts = [part for part in filename_parts if part is not None]

        return "_".join(filename_parts) + ".csv"

    def _get_market_role_identifier(self, requesters_market_role: MarketRole) -> str:
        if requesters_market_role == MarketRole.ENERGY_SUPPLIER:
            return MarketRoleInFileName.ENERGY_SUPPLIER
        elif requesters_market_role == MarketRole.GRID_ACCESS_PROVIDER:
            return MarketRoleInFileName.GRID_ACCESS_PROVIDER
        elif requesters_market_role in {
            MarketRole.DATAHUB_ADMINISTRATOR,
            MarketRole.SYSTEM_OPERATOR,
        }:
            return (
                MarketRoleInFileName.ENERGY_SUPPLIER
                if self.args.energy_supplier_id
                else MarketRoleInFileName.GRID_ACCESS_PROVIDER
            )
        else:
            raise ValueError(
                f"Market role {requesters_market_role} is not supported for file naming"
            )

    def _get_post_fix(self) -> str:
        if self.report_data_type == ReportDataType.TimeSeriesHourly:
            return "TSSD60"
        elif self.report_data_type == ReportDataType.TimeSeriesQuarterly:
            return "TSSD15"

    def _get_actor_id_for_filename(self, energy_supplier_id: str) -> str | None:

        if self.args.requesters_market_role is MarketRole.GRID_ACCESS_PROVIDER:
            return self.args.requesters_id
        else:
            return energy_supplier_id
