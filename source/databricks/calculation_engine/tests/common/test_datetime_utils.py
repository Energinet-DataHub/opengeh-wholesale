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

import pytest

from package.common.datetime_utils import (
    is_exactly_one_calendar_month,
    get_number_of_days_in_period,
)

COPENHAGEN_TIME_ZONE = "Europe/Copenhagen"
LONDON_TIME_ZONE = "Europe/London"


@pytest.mark.parametrize(
    "period_start, period_end",
    [
        (  # Missing one day in the beginning
            datetime(2022, 6, 1, 22),
            datetime(2022, 6, 30, 22),
        ),
        (  # Missing one day at the end
            datetime(2022, 5, 31, 22),
            datetime(2022, 6, 29, 22),
        ),
        (  # Missing one hour in the beginning
            datetime(2022, 5, 31, 23),
            datetime(2022, 6, 30, 22),
        ),
        (  # Missing one hour at the end
            datetime(2022, 5, 31, 22),
            datetime(2022, 6, 30, 21),
        ),
        (  # One hour too much in the beginning
            datetime(2022, 5, 31, 21),
            datetime(2022, 6, 30, 22),
        ),
        (  # One hour too much at the end
            datetime(2022, 5, 31, 22),
            datetime(2022, 6, 30, 23),
        ),
        (  # Two months
            datetime(2022, 5, 31, 22),
            datetime(2022, 7, 31, 22),
        ),
        (  # Entering daylights saving time - not ending at midnight
            datetime(2022, 2, 28, 23, 0),
            datetime(2022, 3, 31, 23, 0),
        ),
        (  # Exiting daylights saving time - not ending at midnight
            datetime(2022, 9, 30, 22, 0),
            datetime(2022, 10, 31, 22, 0),
        ),
    ],
)
def test__is_exactly_one_calendar_month__when_not_one_month__returns_false(
    period_start: datetime, period_end: datetime
) -> None:
    # Arrange
    time_zone = "Europe/Copenhagen"

    # Act
    actual = is_exactly_one_calendar_month(period_start, period_end, time_zone)

    # Assert
    assert actual is False


@pytest.mark.parametrize(
    "period_start, period_end",
    [
        (  # Summer in Copenhagen
            datetime(2022, 5, 31, 22),
            datetime(2022, 6, 30, 22),
        ),
        (  # Crossing year boundary
            datetime(2021, 12, 31, 23),
            datetime(2022, 1, 31, 23),
        ),
        (  # Entering daylights saving time - not ending at midnight
            datetime(2020, 2, 29, 23),
            datetime(2020, 3, 31, 22),
        ),
        (  # Exiting daylights saving time - not ending at midnight
            datetime(2020, 9, 30, 22),
            datetime(2020, 10, 31, 23),
        ),
    ],
)
def test__is_exactly_one_calendar_month__when_exactly_one_month__returns_true(
    period_start: datetime, period_end: datetime
) -> None:

    # Act
    actual = is_exactly_one_calendar_month(
        period_start, period_end, COPENHAGEN_TIME_ZONE
    )

    # Assert
    assert actual is True


@pytest.mark.parametrize(
    "period_start, period_end",
    [
        (  # Summer in London
            datetime(2022, 5, 31, 23),
            datetime(2022, 6, 30, 23),
        ),
        (  # Winter in London
            datetime(2021, 1, 1, 0),
            datetime(2022, 2, 1, 0),
        ),
    ],
)
def test__is_exactly_one_calendar_month__when_london_time_and_exactly_one_month__returns_true(
    period_start: datetime, period_end: datetime
) -> None:
    # Act
    actual = is_exactly_one_calendar_month(period_start, period_end, LONDON_TIME_ZONE)

    # Assert
    assert actual is True


@pytest.mark.parametrize(
    "period_start, period_end",
    [
        (  # Starts and ends at 22:00 (UTC) instead of 23:00 (UTC)
            datetime(2022, 5, 31, 22),
            datetime(2022, 6, 30, 22),
        ),
        (  # Missing one hour in the beginning
            datetime(2022, 1, 1, 1),
            datetime(2022, 2, 1, 0),
        ),
        (  # Two months
            datetime(2022, 1, 1, 1),
            datetime(2022, 3, 1, 1),
        ),
    ],
)
def test__is_exactly_one_calendar_month__when_london_time_and_not_exactly_one_month__returns_false(
    period_start: datetime, period_end: datetime
) -> None:
    # Act
    actual = is_exactly_one_calendar_month(period_start, period_end, LONDON_TIME_ZONE)

    # Assert
    assert actual is False


@pytest.mark.parametrize(
    "period_start, period_end, time_zone",
    [
        (  # start time later than end time
            datetime(2022, 5, 31, 23),
            datetime(2022, 6, 30, 22),
            COPENHAGEN_TIME_ZONE,
        ),
        (  # start time earlier than end time
            datetime(2022, 5, 31, 22),
            datetime(2022, 6, 30, 23),
            COPENHAGEN_TIME_ZONE,
        ),
        (  # Daylight saving time
            datetime(2022, 1, 29, 23),
            datetime(2022, 3, 31, 23),
            COPENHAGEN_TIME_ZONE,
        ),
        (  # Other time zone
            datetime(2021, 1, 1, 0),
            datetime(2022, 2, 1, 1),
            LONDON_TIME_ZONE,
        ),
    ],
)
def test__get_number_of_days_in_period__when_time_of_day_differs__raise_exception(
    period_start: datetime, period_end: datetime, time_zone: str
) -> None:
    # Act
    with pytest.raises(Exception) as exc_info:
        get_number_of_days_in_period(
            period_start,
            period_end,
            time_zone,
        )

    # Assert
    assert str(exc_info.value) == "Period must start and end on the same time of day."
