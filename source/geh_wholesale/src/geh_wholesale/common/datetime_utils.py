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
from dateutil.relativedelta import relativedelta


def is_exactly_one_calendar_month(
    period_start: datetime, period_end: datetime, time_zone: str
) -> bool:
    time_zone_info = ZoneInfo(time_zone)
    period_start_local_time = period_start.astimezone(time_zone_info)
    period_end_local_time = period_end.astimezone(time_zone_info)
    delta = relativedelta(period_end_local_time, period_start_local_time)
    return (
        delta.months == 1
        and delta.days == 0
        and delta.hours == 0
        and delta.minutes == 0
        and delta.seconds == 0
        and delta.microseconds == 0
    )


def get_number_of_days_in_period(
    period_start: datetime, period_end: datetime, time_zone: str
) -> int:
    start_at_midnight, period_start_local_time = is_midnight_in_time_zone(
        period_start, time_zone
    )
    end_at_midnight, period_end_local_time = is_midnight_in_time_zone(
        period_end, time_zone
    )

    if start_at_midnight is False or end_at_midnight is False:
        raise Exception(
            f"Period must start and end at midnight. Got: start={period_start_local_time}, end={period_end_local_time}"
        )

    return (period_end_local_time - period_start_local_time).days


def is_midnight_in_time_zone(time: datetime, time_zone: str) -> tuple[bool, datetime]:
    local_time = time.astimezone(ZoneInfo(time_zone))
    midnight = local_time.replace(hour=0, minute=0, second=0, microsecond=0)
    return (midnight - local_time).total_seconds() == 0, local_time
