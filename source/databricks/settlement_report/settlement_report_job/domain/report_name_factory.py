from collections.abc import Iterable
from datetime import timedelta
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
        chunk_index: str | None,
    ) -> str:
        if self.report_data_type in {
            ReportDataType.TimeSeriesHourly,
            ReportDataType.TimeSeriesQuarterly,
        }:
            return self._create_time_series_filename(
                grid_area_code, energy_supplier_id, chunk_index
            )
        else:
            raise NotImplementedError(
                f"Report data type {self.report_data_type} is not supported."
            )

    def _create_time_series_filename(
        self,
        grid_area_code: str,
        energy_supplier_id: str | None,
        chunk_index: str | None,
    ) -> str:

        filename_parts = [
            self._get_pre_fix(),
            grid_area_code,
            (
                self.args.requesting_actor_id
                if self.args.requesting_actor_market_role
                is MarketRole.GRID_ACCESS_PROVIDER
                else energy_supplier_id
            ),
            (
                MarketRoleInFileName.GRID_ACCESS_PROVIDER
                if energy_supplier_id is None
                else MarketRoleInFileName.ENERGY_SUPPLIER
            ),
            self._get_start_date(),
            self._get_end_date(),
            chunk_index,
        ]

        filename_parts_without_none = [
            part for part in filename_parts if part is not None
        ]

        return "_".join(filename_parts_without_none) + ".csv"

    def _get_start_date(self) -> str:
        time_zone_info = ZoneInfo(self.args.time_zone)
        return self.args.period_start.astimezone(time_zone_info).strftime("%d-%m-%Y")

    def _get_end_date(self) -> str:
        time_zone_info = ZoneInfo(self.args.time_zone)
        return (
            self.args.period_end.astimezone(time_zone_info) - timedelta(days=1)
        ).strftime("%d-%m-%Y")

    def _get_pre_fix(self) -> str:
        if self.report_data_type == ReportDataType.TimeSeriesHourly:
            return "TSSD60"
        elif self.report_data_type == ReportDataType.TimeSeriesQuarterly:
            return "TSSD15"
