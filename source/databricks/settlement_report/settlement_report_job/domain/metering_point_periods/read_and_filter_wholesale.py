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
from settlement_report_job.domain.metering_point_periods.clamp_period import (
    clamp_to_selected_period,
)
from settlement_report_job.infrastructure.repository import WholesaleRepository
from settlement_report_job.domain.utils.repository_filtering import (
    read_metering_point_periods_by_calculation_ids,
    read_charge_link_periods,
)

log = Logger(__name__)


@use_span()
def read_and_filter(
    period_start: datetime,
    period_end: datetime,
    calculation_id_by_grid_area: dict[str, UUID],
    energy_supplier_ids: list[str] | None,
    requesting_actor_market_role: MarketRole,
    requesting_actor_id: str,
    select_columns: list[str],
    repository: WholesaleRepository,
) -> DataFrame:

    metering_point_periods = read_metering_point_periods_by_calculation_ids(
        repository=repository,
        period_start=period_start,
        period_end=period_end,
        calculation_id_by_grid_area=calculation_id_by_grid_area,
        energy_supplier_ids=energy_supplier_ids,
    )

    if requesting_actor_market_role == MarketRole.SYSTEM_OPERATOR:
        metering_point_periods = _filter_by_charge_owner(
            metering_point_periods=metering_point_periods,
            period_start=period_start,
            period_end=period_end,
            requesting_actor_market_role=requesting_actor_market_role,
            requesting_actor_id=requesting_actor_id,
            repository=repository,
        )

    metering_point_periods = metering_point_periods.select(*select_columns)

    metering_point_periods = merge_connected_periods(metering_point_periods)

    metering_point_periods = clamp_to_selected_period(
        metering_point_periods, period_start, period_end
    )

    return metering_point_periods


def _filter_by_charge_owner(
    metering_point_periods: DataFrame,
    period_start: datetime,
    period_end: datetime,
    requesting_actor_market_role: MarketRole,
    requesting_actor_id: str,
    repository: WholesaleRepository,
) -> DataFrame:
    charge_link_periods = read_charge_link_periods(
        repository=repository,
        period_start=period_start,
        period_end=period_end,
        charge_owner_id=requesting_actor_id,
        requesting_actor_market_role=requesting_actor_market_role,
    )
    metering_point_periods = join_metering_points_periods_and_charge_link_periods(
        charge_link_periods=charge_link_periods,
        metering_point_periods=metering_point_periods,
    )

    return metering_point_periods
