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
from datetime import timedelta, datetime
from zoneinfo import ZoneInfo

from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.report_name_factory import MarketRoleInFileName

DEFAULT_TIME_ZONE = "Europe/Copenhagen"


def get_start_date(period_start: datetime) -> str:
    time_zone_info = ZoneInfo(DEFAULT_TIME_ZONE)
    return period_start.astimezone(time_zone_info).strftime("%d-%m-%Y")


def get_end_date(period_end: datetime) -> str:
    time_zone_info = ZoneInfo(DEFAULT_TIME_ZONE)
    return (period_end.astimezone(time_zone_info) - timedelta(days=1)).strftime(
        "%d-%m-%Y"
    )


def get_market_role_in_file_name(
    requesting_actor_market_role: MarketRole,
) -> str | None:
    if requesting_actor_market_role == MarketRole.ENERGY_SUPPLIER:
        return MarketRoleInFileName.ENERGY_SUPPLIER
    elif requesting_actor_market_role == MarketRole.GRID_ACCESS_PROVIDER:
        return MarketRoleInFileName.GRID_ACCESS_PROVIDER

    return None