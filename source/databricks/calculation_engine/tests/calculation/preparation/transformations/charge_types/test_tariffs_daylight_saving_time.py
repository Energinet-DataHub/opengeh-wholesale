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

from pyspark.sql import SparkSession

import package.codelists as e
import tests.calculation.charges_factory as factory
from calculation.preparation.transformations import (
    prepared_metering_point_time_series_factory,
)
from package.calculation.preparation.transformations import (
    get_prepared_tariffs,
)

DEFAULT_TIME_ZONE = "Europe/Copenhagen"


def test__get_prepared_tariffs__when_summer_time_and_hourly_resolution__returns_expected(
    spark: SparkSession,
) -> None:
    """
    TODO JMG: When transitioning to daylight saving time the returned number of rows .
    """
    # Arrange
    time_series_rows = [
        factory.create_time_series_row(observation_time=datetime(2021, 3, 27, 23)),
        factory.create_time_series_row(observation_time=datetime(2021, 3, 28, 0)),
        factory.create_time_series_row(observation_time=datetime(2021, 3, 28, 1)),
        factory.create_time_series_row(observation_time=datetime(2021, 3, 28, 2)),
        factory.create_time_series_row(observation_time=datetime(2021, 3, 28, 3)),
    ]
    charge_price_information_rows = [
        factory.create_charge_price_information_row(
            from_date=datetime(2021, 3, 26, 23),
            to_date=datetime(2021, 4, 2, 0),
            resolution=e.ChargeResolution.HOUR,
        ),
    ]
    charge_prices_rows = [
        factory.create_charge_prices_row(),
    ]
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            from_date=datetime(2021, 3, 26, 23),
            to_date=datetime(2021, 4, 2, 0),
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
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)

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


def test__get_prepared_tariffs__when_winter_time_and_hourly_resolution__returns_expected(
    spark: SparkSession,
) -> None:
    """
    TODO JMG: When transitioning to daylight saving time the returned number of rows .
    """
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
    charge_prices_rows = [
        factory.create_charge_prices_row(),
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
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)

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
