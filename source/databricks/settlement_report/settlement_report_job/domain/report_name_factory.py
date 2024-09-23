from enum import Enum
from zoneinfo import ZoneInfo

from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.report_data_type import ReportDataType
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs


class MarketRoleInFileName:
    ENERGY_SUPPLIER = "DDQ"
    GRID_ACCESS_PROVIDER = "DDM"


class FileNameFactory:
    def __init__(self, report_data_type: ReportDataType, args: SettlementReportArgs):
        self.report_data_type = report_data_type
        self.args = args

    def create(
        self,
        grid_area_code: str,
        energy_supplier_id: str | None,
        chunk_index: str = None,
    ) -> str:
        if self.report_data_type in {
            ReportDataType.TimeSeriesHourly,
            ReportDataType.TimeSeriesQuarterly,
        }:
            return self._create_time_series_filename(
                grid_area_code, energy_supplier_id, chunk_index
            )

    def _create_time_series_filename(
        self,
        grid_area_code: str,
        energy_supplier_id: str,
        chunk_index: str = None,
    ) -> str:

        time_zone_info = ZoneInfo(self.args.time_zone)

        filename_parts = [
            self._get_pre_fix(),
            grid_area_code,
            (
                self.args.requesters_id
                if self.args.requesters_market_role is MarketRole.GRID_ACCESS_PROVIDER
                else energy_supplier_id
            ),
            (
                MarketRoleInFileName.GRID_ACCESS_PROVIDER
                if energy_supplier_id is None
                else MarketRoleInFileName.ENERGY_SUPPLIER
            ),
            self.args.period_start.astimezone(time_zone_info).strftime("%d-%m-%Y"),
            self.args.period_end.astimezone(time_zone_info).strftime("%d-%m-%Y"),
            chunk_index,
        ]
        filename_parts = [part for part in filename_parts if part is not None]

        return "_".join(filename_parts) + ".csv"

    def _get_pre_fix(self) -> str:
        if self.report_data_type == ReportDataType.TimeSeriesHourly:
            return "TSSD60"
        elif self.report_data_type == ReportDataType.TimeSeriesQuarterly:
            return "TSSD15"
