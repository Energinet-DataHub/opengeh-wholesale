from enum import Enum

from settlement_report_job.domain.report_naming_convention import MARKET_ROLES
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs


class ReportType(Enum):
    TimeSeriesHourly = "time_series_hourly"
    TimeSeriesQuarterly = "time_series_quarterly"


class ReportNameFactory:
    def __init__(self, report_type: ReportType, args: SettlementReportArgs):
        self.report_type = report_type
        self.args = args

    def create(self, grid_area_code: str) -> str:
        if self.report_type == ReportType.TimeSeriesHourly:
            return self._create_time_series_hourly_filename(grid_area_code)

    def _create_time_series_hourly_filename(
        self, grid_area_code: str, split_index: str = None
    ) -> str:

        return (
            f"TSSD60"
            f"_{grid_area_code}"
            # f"_{self.args.gln}" ToDo JMG: This is not in the args
            f"_{MARKET_ROLES[self.args.market_role]}"
            f"_{self.args.period_start.strftime('%d-%m-%Y')}"
            f"_{self.args.period_end.strftime('%d-%m-%Y')}"(
                # f"_{split_index}" if split_index is not None else ""
                ".csv"
            )
        )
