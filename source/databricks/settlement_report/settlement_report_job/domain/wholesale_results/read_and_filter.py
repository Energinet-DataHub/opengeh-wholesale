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
from datetime import datetime

from pyspark.sql import DataFrame, functions as F

from telemetry_logging import Logger, use_span
from settlement_report_job.domain.utils.factory_filters import (
    filter_by_charge_owner_and_tax_depending_on_market_role,
    filter_by_calculation_id_by_grid_area,
)
from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.infrastructure.repository import WholesaleRepository
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)

log = Logger(__name__)


@use_span()
def read_and_filter_from_view(
    energy_supplier_ids: list[str] | None,
    calculation_id_by_grid_area: dict[str, UUID],
    period_start: datetime,
    period_end: datetime,
    requesting_actor_market_role: MarketRole,
    requesting_actor_id: str,
    repository: WholesaleRepository,
) -> DataFrame:
    df = repository.read_amounts_per_charge().where(
        (F.col(DataProductColumnNames.time) >= period_start)
        & (F.col(DataProductColumnNames.time) < period_end)
    )

    if energy_supplier_ids is not None:
        df = df.where(
            F.col(DataProductColumnNames.energy_supplier_id).isin(energy_supplier_ids)
        )

    df = df.where(filter_by_calculation_id_by_grid_area(calculation_id_by_grid_area))

    df = filter_by_charge_owner_and_tax_depending_on_market_role(
        df, requesting_actor_market_role, requesting_actor_id
    )

    return df
