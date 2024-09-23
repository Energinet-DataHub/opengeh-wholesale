from enum import Enum

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

    def create(self, grid_area_code: str) -> str:
        if self.report_data_type == ReportDataType.TimeSeriesHourly:
            return self._create_time_series_hourly_filename(grid_area_code)

    def _create_time_series_hourly_filename(
        self,
        grid_area_code: str,
        energy_supplier_id: str = None,
        split_index: str = None,
    ) -> str:

        filename_parts = [
            "TSSD60",
            grid_area_code,
        ]

        filename_parts.append(self._get_actor_id_for_filename(energy_supplier_id))

        filename_parts.append(
            self._get_market_role_identifier(self.args.requesters_market_role)
        )
        filename_parts.append(self.args.period_start.strftime("%d-%m-%Y"))
        filename_parts.append(self.args.period_end.strftime("%d-%m-%Y"))

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

    def _get_actor_id_for_filename(self, energy_supplier_id: str) -> str | None:
        if self.args.requesters_market_role in {
            MarketRole.ENERGY_SUPPLIER,
            MarketRole.GRID_ACCESS_PROVIDER,
        }:
            return self.args.requesters_id
        elif energy_supplier_id is not None:
            return energy_supplier_id

    def _is_energy_supplier_or_grid_access_provider(self) -> bool:
        return self.args.requesters_market_role in {
            MarketRole.ENERGY_SUPPLIER,
            MarketRole.GRID_ACCESS_PROVIDER,
        }
