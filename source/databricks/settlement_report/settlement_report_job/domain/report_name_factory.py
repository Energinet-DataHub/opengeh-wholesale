from datetime import timedelta
from zoneinfo import ZoneInfo

from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.report_data_type import ReportDataType
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs


class MarketRoleInFileName:
    """
    Market role identifiers used in the csv file name in the settlement report.
    System operator and datahub admin are not included as they are not part of the file name.
    """

    ENERGY_SUPPLIER = "DDQ"
    GRID_ACCESS_PROVIDER = "DDM"


class FileNameFactory:
    def __init__(self, report_data_type: ReportDataType, args: SettlementReportArgs):
        self.report_data_type = report_data_type
        self.args = args

    def create(
        self,
        grid_area_code: str | None,
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
        if self.report_data_type in [
            ReportDataType.EnergyResults,
            ReportDataType.WholesaleResults,
        ]:
            return self._create_result_filename(grid_area_code, energy_supplier_id)
        else:
            raise NotImplementedError(
                f"Report data type {self.report_data_type} is not supported."
            )

    def _create_result_filename(
        self,
        grid_area_code: str | None,
        energy_supplier_id: str | None,
    ) -> str:
        filename_parts = [
            self._get_pre_fix(),
            grid_area_code if grid_area_code is not None else "flere-net",
            self._get_actor_id_in_file_name(energy_supplier_id),
            self._get_market_role_in_file_name(),
            self._get_start_date(),
            self._get_end_date(),
        ]

        filename_parts_without_none = [
            part for part in filename_parts if part is not None
        ]

        return "_".join(filename_parts_without_none) + ".csv"

    def _create_time_series_filename(
        self,
        grid_area_code: str | None,
        energy_supplier_id: str | None,
        chunk_index: str | None,
    ) -> str:

        filename_parts = [
            self._get_pre_fix(),
            grid_area_code,
            self._get_actor_id_in_file_name(energy_supplier_id),
            self._get_market_role_in_file_name(),
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
        elif self.report_data_type == ReportDataType.EnergyResults:
            return "RESULTENERGY"
        elif self.report_data_type == ReportDataType.WholesaleResults:
            return "RESULTWHOLESALE"
        raise NotImplementedError(
            f"Report data type {self.report_data_type} is not supported."
        )

    def _get_actor_id_in_file_name(self, energy_supplier_id: str | None) -> str | None:
        if self.args.requesting_actor_market_role in {
            MarketRole.ENERGY_SUPPLIER,
            MarketRole.GRID_ACCESS_PROVIDER,
        }:
            return self.args.requesting_actor_id
        elif self.args.requesting_actor_market_role in {
            MarketRole.SYSTEM_OPERATOR,
            MarketRole.DATAHUB_ADMINISTRATOR,
        }:
            return energy_supplier_id
        else:
            raise ValueError(
                f"Market role {self.args.requesting_actor_market_role} is not supported."
            )

    def _get_market_role_in_file_name(self) -> str | None:
        if self.args.requesting_actor_market_role == MarketRole.ENERGY_SUPPLIER:
            return MarketRoleInFileName.ENERGY_SUPPLIER
        elif self.args.requesting_actor_market_role == MarketRole.GRID_ACCESS_PROVIDER:
            return MarketRoleInFileName.GRID_ACCESS_PROVIDER

        return None
