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
from zoneinfo import ZoneInfo


def is_exactly_one_calendar_month(
    period_start: datetime, period_end: datetime, time_zone: str
) -> bool:
    time_zone_info = ZoneInfo(time_zone)
    period_start_local_time = period_start.astimezone(time_zone_info)
    period_end_local_time = period_end.astimezone(time_zone_info)

    return (
        period_start_local_time.time()
        == period_end_local_time.time()
        == datetime.min.time()
        and period_start_local_time.day == 1
        and period_end_local_time.day == 1
        and period_end_local_time.month == (period_start_local_time.month % 12) + 1
    )


def get_number_of_days_in_period(
    period_start: datetime, period_end: datetime, time_zone: str
) -> int:
    time_zone_info = ZoneInfo(time_zone)
    period_start_local_time = period_start.astimezone(time_zone_info)
    period_end_local_time = period_end.astimezone(time_zone_info)

    return (period_end_local_time - period_start_local_time).days
