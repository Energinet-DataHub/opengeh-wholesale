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
import telemetry.logging.logging_configuration as config

from settlement_report_job import logging
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.repository import WholesaleRepository
from settlement_report_job.wholesale.column_names import DataProductColumnNames

log = logging.Logger(__name__)


@logging.use_span()
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

    if calculation_id_by_grid_area is not None:
        calculation_id_by_grid_area_structs = [
            F.struct(F.lit(grid_area_code), F.lit(str(calculation_id)))
            for grid_area_code, calculation_id in calculation_id_by_grid_area.items()
        ]

        df = df.where(
            F.struct(
                F.col(DataProductColumnNames.grid_area_code),
                F.col(DataProductColumnNames.calculation_id),
            ).isin(calculation_id_by_grid_area_structs)
        )

    if (
        requesting_actor_market_role == MarketRole.GRID_ACCESS_PROVIDER
        or requesting_actor_market_role == MarketRole.SYSTEM_OPERATOR
    ):
        df = df.where(
            F.col(DataProductColumnNames.charge_owner_id) == requesting_actor_id
        )
        if requesting_actor_market_role == MarketRole.GRID_ACCESS_PROVIDER:
            df = df.where(F.col(DataProductColumnNames.is_tax) == F.lit(True))
        elif requesting_actor_market_role == MarketRole.SYSTEM_OPERATOR:
            df = df.where(F.col(DataProductColumnNames.is_tax) == F.lit(False))

    return df
