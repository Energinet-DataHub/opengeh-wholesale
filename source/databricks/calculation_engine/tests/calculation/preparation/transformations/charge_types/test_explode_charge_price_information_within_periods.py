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
from datetime import datetime, timedelta

import pytest
from pyspark.sql import Row, SparkSession

import package.codelists as e
import tests.calculation.charges_factory as factory

from package.calculation.preparation.transformations.charge_types.explode_charge_price_information_within_periods import (
    explode_charge_price_information_within_periods,
)
from package.codelists import ChargeResolution
from package.constants import Colname

DEFAULT_TIME_ZONE = "Europe/Copenhagen"
FEB_1ST = datetime(2020, 1, 31, 23)
FEB_2ND = datetime(2020, 2, 1, 23)
FEB_3RD = datetime(2020, 2, 2, 23)
FEB_4TH = datetime(2020, 2, 3, 23)


def get_delta_time(charge_resolution: e.ChargeResolution) -> timedelta:
    return (
        timedelta(hours=1)
        if charge_resolution == e.ChargeResolution.HOUR
        else timedelta(days=1)
    )


class TestWhenChargePeriodStopsAndStartsOnSameDay:
    """
    When the charge period stops and starts on the same day, the resulting quantities should behave as if there were just one period that crossed that day
    """

    def test__explode_charge_price_information_within_periods__when_hourly_resolution__returns_expected(
        self, spark: SparkSession
    ) -> None:

        # Arrange
        expected_charge_times = 49  # start and end times are included
        charge_price_information_rows = [
            factory.create_charge_price_information_row(
                from_date=FEB_1ST, to_date=FEB_2ND, resolution=ChargeResolution.HOUR
            ),
            factory.create_charge_price_information_row(
                from_date=FEB_2ND, to_date=FEB_3RD, resolution=ChargeResolution.HOUR
            ),
        ]

        charge_price_information = factory.create_charge_price_information(
            spark, charge_price_information_rows
        )

        # Act
        actual = explode_charge_price_information_within_periods(
            charge_price_information,
            ChargeResolution.HOUR,
            DEFAULT_TIME_ZONE,
        )

        # Assert
        assert actual.count() == expected_charge_times
        actual_rows = actual.orderBy(Colname.charge_time).collect()
        for i, row in enumerate(actual_rows):
            assert row[Colname.charge_time] == FEB_1ST + i * timedelta(hours=1)

    def test__explode_charge_price_information_within_periods__when_daily_resolution__returns_expected(
        self, spark: SparkSession
    ) -> None:

        # Arrange
        expected_charge_times = 3  # start and end times are included
        charge_price_information_rows = [
            factory.create_charge_price_information_row(
                from_date=FEB_1ST, to_date=FEB_2ND, resolution=ChargeResolution.DAY
            ),
            factory.create_charge_price_information_row(
                from_date=FEB_2ND, to_date=FEB_3RD, resolution=ChargeResolution.DAY
            ),
        ]

        charge_price_information = factory.create_charge_price_information(
            spark, charge_price_information_rows
        )

        # Act
        actual = explode_charge_price_information_within_periods(
            charge_price_information,
            ChargeResolution.DAY,
            DEFAULT_TIME_ZONE,
        )

        # Assert
        assert actual.count() == expected_charge_times
        actual_rows = actual.orderBy(Colname.charge_time).collect()
        for i, row in enumerate(actual_rows):
            assert row[Colname.charge_time] == FEB_1ST + i * timedelta(days=1)


class TestWhenChargeStopsForOneDay:
    """
    When the charge stops and then starts one day later, then there should not be any result on the missing day.
    """

    def test__explode_charge_price_information_within_periods__when_hourly_resolution__returns_expected(
        self, spark: SparkSession
    ) -> None:

        # Arrange
        expected_charge_times_day_1 = 25  # start and end times are included
        expected_charge_times_day_2 = 25  # start and end times are included
        charge_price_information_rows = [
            factory.create_charge_price_information_row(
                from_date=FEB_1ST, to_date=FEB_2ND, resolution=ChargeResolution.HOUR
            ),
            factory.create_charge_price_information_row(
                from_date=FEB_3RD, to_date=FEB_4TH, resolution=ChargeResolution.HOUR
            ),
        ]

        charge_price_information = factory.create_charge_price_information(
            spark, charge_price_information_rows
        )

        # Act
        actual = explode_charge_price_information_within_periods(
            charge_price_information,
            ChargeResolution.HOUR,
            DEFAULT_TIME_ZONE,
        )

        # Assert
        assert (
            actual.count() == expected_charge_times_day_1 + expected_charge_times_day_2
        )
        actual_rows = actual.orderBy(Colname.charge_time).collect()
        for i in range(expected_charge_times_day_1):
            assert actual_rows[i][Colname.charge_time] == FEB_1ST + i * timedelta(
                hours=1
            )

        for i in range(expected_charge_times_day_2):
            assert actual_rows[i + expected_charge_times_day_1][
                Colname.charge_time
            ] == FEB_3RD + i * timedelta(hours=1)

    def test__explode_charge_price_information_within_periods__when_daily_resolution__returns_expected(
        self, spark: SparkSession
    ) -> None:

        # Arrange
        expected_charge_times = 4
        charge_price_information_rows = [
            factory.create_charge_price_information_row(
                from_date=FEB_1ST, to_date=FEB_2ND, resolution=ChargeResolution.DAY
            ),
            factory.create_charge_price_information_row(
                from_date=FEB_3RD, to_date=FEB_4TH, resolution=ChargeResolution.DAY
            ),
        ]

        charge_price_information = factory.create_charge_price_information(
            spark, charge_price_information_rows
        )

        # Act
        actual = explode_charge_price_information_within_periods(
            charge_price_information,
            ChargeResolution.DAY,
            DEFAULT_TIME_ZONE,
        )

        # Assert
        assert actual.count() == expected_charge_times
        actual_rows = actual.orderBy(Colname.charge_time).collect()
        assert actual_rows[0][Colname.charge_time] == FEB_1ST
        assert actual_rows[1][Colname.charge_time] == FEB_2ND
        assert actual_rows[2][Colname.charge_time] == FEB_3RD
        assert actual_rows[3][Colname.charge_time] == FEB_4TH
