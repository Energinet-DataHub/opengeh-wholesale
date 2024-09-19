# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
from uuid import UUID
from dataclasses import dataclass
from datetime import datetime

from settlement_report_job.domain.task_type import TaskType
from settlement_report_job.domain.calculation_type import CalculationType
from settlement_report_job.domain.market_role import MarketRole


@dataclass
class SettlementReportArgs:
    report_id: str
    period_start: datetime
    period_end: datetime
    calculation_type: CalculationType
    market_role: MarketRole
    calculation_id_by_grid_area: dict[str, UUID]
    """A dictionary containing grid area codes (keys) and calculation ids (values)."""
    energy_supplier_id: str | None
    split_report_by_grid_area: bool
    prevent_large_text_files: bool
    time_zone: str
    catalog_name: str
    task_type: TaskType
