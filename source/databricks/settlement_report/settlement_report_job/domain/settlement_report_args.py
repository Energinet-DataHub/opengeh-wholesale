from dataclasses import dataclass
from datetime import datetime

from settlement_report_job.infrastructure.calculation_type import CalculationType
from settlement_report_job.domain.market_role import MarketRole


@dataclass
class SettlementReportArgs:
    report_id: str
    period_start: datetime
    period_end: datetime
    calculation_type: CalculationType
    requesting_actor_market_role: MarketRole
    requesting_actor_id: str
    calculation_id_by_grid_area_codes: dict[str, str]
    grid_area_codes: list[str]
    """A dictionary containing grid area codes (keys) and calculation ids (values)."""
    energy_supplier_ids: list[str] | None
    split_report_by_grid_area: bool
    prevent_large_text_files: bool
    time_zone: str
    catalog_name: str
    settlement_reports_output_path: str
    """The path to the folder where the settlement reports are stored."""
    include_basis_data: bool
    locale: str
