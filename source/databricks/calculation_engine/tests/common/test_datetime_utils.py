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

from package.common.datetime_utils import is_exactly_one_calendar_month

DEFAULT_TIME_ZONE = "Europe/Copenhagen"


class TestWhenPeriodIsNotOneCalendarMonth:
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
            ( # Two months
                    datetime(2022, 5, 31, 22),
                    datetime(2022, 7, 31, 22),
            ),
            (  # Entering daylights saving time - not ending at midnight
                    datetime(2020, 2, 29, 23, 0),
                    datetime(2020, 3, 31, 23, 0),
            ),
            (  # Exiting daylights saving time - not ending at midnight
                    datetime(2020, 9, 30, 22, 0),
                    datetime(2020, 10, 31, 22, 0),
            ),
        ],
    )
    def test__is_exactly_one_calendar_month__returns_false(self,
        period_start: datetime, period_end: datetime) -> None:
        # Arrange
        time_zone = "Europe/Copenhagen"

        # Act
        actual = is_exactly_one_calendar_month(period_start, period_end, time_zone)

        # Assert
        assert actual is False






class TestWhenPeriodOneCalendarMonth:
    @pytest.mark.parametrize(
        "period_start, period_end",
        [
            (
                    datetime(2022, 5, 31, 22),
                    datetime(2022, 6, 30, 22),
                    DEFAULT_TIME_ZONE,
            ),
            (
                    datetime(2022, 5, 31, 22),
                    datetime(2022, 6, 29, 22),
                    DEFAULT_TIME_ZONE,
            ),
            (
                    datetime(2022, 5, 31, 23),
                    datetime(2022, 6, 30, 22),
                    DEFAULT_TIME_ZONE,
            ),
            (  # Entering daylights saving time - not ending at midnight
                    datetime(2020, 2, 29, 23, 0),
                    datetime(2020, 3, 31, 22, 0),
                
            ),
            (  # Exiting daylights saving time - not ending at midnight
                    datetime(2020, 9, 30, 22, 0),
                    datetime(2020, 10, 31, 23, 0),
            ),
        ],
    )
    def test__is_exactly_one_calendar_month__returns_true(self,
        period_start: datetime, period_end: datetime, time_zone: str) -> None:

        # Act
        actual = is_exactly_one_calendar_month(period_start, period_end, time_zone)

        # Assert
        assert actual is True