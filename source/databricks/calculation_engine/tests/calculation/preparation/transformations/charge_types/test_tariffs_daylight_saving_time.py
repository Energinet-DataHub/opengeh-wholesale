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

from pyspark.sql import SparkSession

import package.codelists as e
import tests.calculation.charges_factory as factory
from calculation.preparation.transformations import (
    prepared_metering_point_time_series_factory,
)
from package.calculation.preparation.transformations import (
    get_prepared_tariffs,
)
from package.constants import Colname

DEFAULT_TIME_ZONE = "Europe/Copenhagen"


class TestWhenEnteringDaylightSavingTime:
    def test__get_prepared_tariffs__when_hourly_resolution__returns_expected_number_rows(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        time_series_rows = [
            factory.create_time_series_row(observation_time=datetime(2021, 3, 27, 23)),
            factory.create_time_series_row(observation_time=datetime(2021, 3, 28, 0)),
            factory.create_time_series_row(observation_time=datetime(2021, 3, 28, 1)),
            factory.create_time_series_row(observation_time=datetime(2021, 3, 28, 2)),
            factory.create_time_series_row(observation_time=datetime(2021, 3, 28, 3)),
        ]
        from_date = datetime(2021, 3, 26, 23)
        to_date = datetime(2021, 4, 2, 22)
        charge_price_information_rows = [
            factory.create_charge_price_information_row(
                from_date=from_date,
                to_date=to_date,
                resolution=e.ChargeResolution.HOUR,
            ),
        ]
        charge_link_metering_points_rows = [
            factory.create_charge_link_metering_point_periods_row(
                from_date=from_date,
                to_date=to_date,
            ),
        ]

        charge_link_metering_point_periods = (
            factory.create_charge_link_metering_point_periods(
                spark, charge_link_metering_points_rows
            )
        )
        time_series = prepared_metering_point_time_series_factory.create(
            spark, time_series_rows
        )
        charge_price_information = factory.create_charge_price_information(
            spark, charge_price_information_rows
        )
        charge_prices = factory.create_charge_prices(spark, [])

        # Act
        actual = get_prepared_tariffs(
            time_series,
            charge_price_information,
            charge_prices,
            charge_link_metering_point_periods,
            e.ChargeResolution.HOUR,
            DEFAULT_TIME_ZONE,
        )

        # Assert
        assert actual.df.count() == 5

    def test__get_prepared_tariffs__when_daily_resolution__returns_expected_number_rows(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        time_series_rows = []

        for i in range(0, 47):
            # create 47 hours of time series data corresponding to 2 full days when entering daylight saving time
            time_series_rows.append(
                factory.create_time_series_row(
                    observation_time=datetime(2021, 3, 27, 23) + timedelta(hours=i)
                )
            )

        from_date = datetime(2021, 3, 26, 23)
        to_date = datetime(2021, 4, 2, 22)
        charge_price_information_rows = [
            factory.create_charge_price_information_row(
                from_date=from_date,
                to_date=to_date,
                resolution=e.ChargeResolution.DAY,
            ),
        ]
        charge_link_metering_points_rows = [
            factory.create_charge_link_metering_point_periods_row(
                from_date=from_date,
                to_date=to_date,
            ),
        ]

        charge_link_metering_point_periods = (
            factory.create_charge_link_metering_point_periods(
                spark, charge_link_metering_points_rows
            )
        )
        time_series = prepared_metering_point_time_series_factory.create(
            spark, time_series_rows
        )
        charge_price_information = factory.create_charge_price_information(
            spark, charge_price_information_rows
        )
        charge_prices = factory.create_charge_prices(spark, [])

        # Act
        actual = get_prepared_tariffs(
            time_series,
            charge_price_information,
            charge_prices,
            charge_link_metering_point_periods,
            e.ChargeResolution.DAY,
            DEFAULT_TIME_ZONE,
        )

        # Assert
        assert actual.df.count() == 2
        actual_df = actual.df.orderBy(Colname.charge_time).collect()
        assert actual_df[0][Colname.quantity] == 23 * factory.DefaultValues.QUANTITY
        assert actual_df[1][Colname.quantity] == 24 * factory.DefaultValues.QUANTITY
        assert actual_df[0][Colname.charge_time] == datetime(2021, 3, 27, 23)
        assert actual_df[1][Colname.charge_time] == datetime(2021, 3, 28, 22)


class TestWhenExitingDaylightSavingTime:
    def test__get_prepared_tariffs__when_hourly_resolution__returns_expected_number_rows(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        time_series_rows = [
            factory.create_time_series_row(observation_time=datetime(2021, 10, 30, 22)),
            factory.create_time_series_row(observation_time=datetime(2021, 10, 30, 23)),
            factory.create_time_series_row(observation_time=datetime(2021, 10, 31, 0)),
            factory.create_time_series_row(observation_time=datetime(2021, 10, 31, 1)),
            factory.create_time_series_row(observation_time=datetime(2021, 10, 31, 2)),
        ]
        charge_price_information_rows = [
            factory.create_charge_price_information_row(
                from_date=datetime(2021, 10, 29, 22),
                to_date=datetime(2021, 11, 1, 23),
                resolution=e.ChargeResolution.HOUR,
            ),
        ]

        charge_link_metering_points_rows = [
            factory.create_charge_link_metering_point_periods_row(
                from_date=datetime(2021, 10, 29, 22),
                to_date=datetime(2021, 11, 1, 23),
            ),
        ]

        charge_link_metering_point_periods = (
            factory.create_charge_link_metering_point_periods(
                spark, charge_link_metering_points_rows
            )
        )
        time_series = prepared_metering_point_time_series_factory.create(
            spark, time_series_rows
        )
        charge_price_information = factory.create_charge_price_information(
            spark, charge_price_information_rows
        )
        charge_prices = factory.create_charge_prices(spark, [])

        # Act
        actual = get_prepared_tariffs(
            time_series,
            charge_price_information,
            charge_prices,
            charge_link_metering_point_periods,
            e.ChargeResolution.HOUR,
            DEFAULT_TIME_ZONE,
        )

        # Assert
        assert actual.df.count() == 5

    def test__get_prepared_tariffs__when_daily_resolution__returns_expected_number_rows(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        time_series_rows = []

        for i in range(0, 49):
            # create 49 hours of time series data corresponding to 2 full days when exiting daylight saving time
            time_series_rows.append(
                factory.create_time_series_row(
                    observation_time=datetime(2021, 10, 30, 22) + timedelta(hours=i)
                )
            )
        from_date = datetime(2021, 10, 29, 22)
        to_date = datetime(2021, 11, 1, 23)

        charge_price_information_rows = [
            factory.create_charge_price_information_row(
                from_date=from_date,
                to_date=to_date,
                resolution=e.ChargeResolution.DAY,
            ),
        ]
        charge_link_metering_points_rows = [
            factory.create_charge_link_metering_point_periods_row(
                from_date=from_date,
                to_date=to_date,
            ),
        ]

        charge_link_metering_point_periods = (
            factory.create_charge_link_metering_point_periods(
                spark, charge_link_metering_points_rows
            )
        )
        time_series = prepared_metering_point_time_series_factory.create(
            spark, time_series_rows
        )
        charge_price_information = factory.create_charge_price_information(
            spark, charge_price_information_rows
        )
        charge_prices = factory.create_charge_prices(spark, [])

        # Act
        actual = get_prepared_tariffs(
            time_series,
            charge_price_information,
            charge_prices,
            charge_link_metering_point_periods,
            e.ChargeResolution.DAY,
            DEFAULT_TIME_ZONE,
        )

        # Assert
        assert actual.df.count() == 2
        actual_df = actual.df.orderBy(Colname.charge_time).collect()
        assert actual_df[0][Colname.quantity] == 25 * factory.DefaultValues.QUANTITY
        assert actual_df[1][Colname.quantity] == 24 * factory.DefaultValues.QUANTITY
        assert actual_df[0][Colname.charge_time] == datetime(2021, 10, 30, 22)
        assert actual_df[1][Colname.charge_time] == datetime(2021, 10, 31, 23)
