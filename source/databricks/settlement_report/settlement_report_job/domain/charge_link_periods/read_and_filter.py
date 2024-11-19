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
from datetime import datetime
from uuid import UUID

from pyspark.sql import DataFrame

from telemetry_logging import Logger, use_span
from settlement_report_job.domain.utils.join_metering_points_periods_and_charge_link_periods import (
    join_metering_points_periods_and_charge_link_periods,
)
from settlement_report_job.domain.utils.merge_periods import (
    merge_connected_periods,
)
from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.infrastructure.repository import WholesaleRepository
from settlement_report_job.domain.utils.repository_filtering import (
    read_charge_link_periods,
    read_metering_point_periods_by_calculation_ids,
)
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)

logger = Logger(__name__)


@use_span()
def read_and_filter(
    period_start: datetime,
    period_end: datetime,
    calculation_id_by_grid_area: dict[str, UUID],
    energy_supplier_ids: list[str] | None,
    requesting_actor_market_role: MarketRole,
    requesting_actor_id: str,
    repository: WholesaleRepository,
) -> DataFrame:
    logger.info("Creating charge links")

    charge_link_periods = read_charge_link_periods(
        repository=repository,
        period_start=period_start,
        period_end=period_end,
        charge_owner_id=requesting_actor_id,
        requesting_actor_market_role=requesting_actor_market_role,
    )

    charge_link_periods = _join_with_metering_point_periods(
        charge_link_periods,
        period_start,
        period_end,
        calculation_id_by_grid_area,
        energy_supplier_ids,
        repository,
    )

    charge_link_periods = charge_link_periods.select(
        _get_select_columns(requesting_actor_market_role)
    )

    charge_link_periods = merge_connected_periods(charge_link_periods)

    return charge_link_periods


def _join_with_metering_point_periods(
    charge_link_periods: DataFrame,
    period_start: datetime,
    period_end: datetime,
    calculation_id_by_grid_area: dict[str, UUID],
    energy_supplier_ids: list[str] | None,
    repository: WholesaleRepository,
) -> DataFrame:
    metering_point_periods = read_metering_point_periods_by_calculation_ids(
        repository=repository,
        period_start=period_start,
        period_end=period_end,
        calculation_id_by_grid_area=calculation_id_by_grid_area,
        energy_supplier_ids=energy_supplier_ids,
    )

    charge_link_periods = join_metering_points_periods_and_charge_link_periods(
        charge_link_periods, metering_point_periods
    )

    return charge_link_periods


def _get_select_columns(
    requesting_actor_market_role: MarketRole,
) -> list[str]:
    select_columns = [
        DataProductColumnNames.metering_point_id,
        DataProductColumnNames.metering_point_type,
        DataProductColumnNames.charge_type,
        DataProductColumnNames.charge_code,
        DataProductColumnNames.charge_owner_id,
        DataProductColumnNames.quantity,
        DataProductColumnNames.from_date,
        DataProductColumnNames.to_date,
        DataProductColumnNames.grid_area_code,
    ]
    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        select_columns.append(DataProductColumnNames.energy_supplier_id)

    return select_columns
