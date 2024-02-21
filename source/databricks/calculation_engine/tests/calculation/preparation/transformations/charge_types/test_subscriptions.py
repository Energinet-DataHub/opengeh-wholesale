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

import pytest
from datetime import datetime
from pyspark.sql import SparkSession

from package.calculation.preparation.transformations import (
    get_subscription_charges,
)
from package.constants import Colname
import package.codelists as e

import tests.calculation.charges_factory as factory


def test__get_subscription_charges__filters_on_subscription_charge_type(
    spark: SparkSession,
) -> None:
    # Arrange
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.FEE
        ),
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.SUBSCRIPTION
        ),
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.FEE
        ),
    ]
    charge_period_prices_rows = [
        factory.create_subscription_or_fee_charge_period_prices_row(
            charge_type=e.ChargeType.FEE,
        ),
        factory.create_subscription_or_fee_charge_period_prices_row(
            charge_type=e.ChargeType.SUBSCRIPTION,
        ),
        factory.create_tariff_charge_period_prices_row(),
    ]

    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )
    charge_period_prices = factory.create_charge_period_prices(
        spark, charge_period_prices_rows
    )

    # Act
    actual_subscription = get_subscription_charges(
        charge_period_prices, charge_link_metering_point_periods
    )

    # Assert
    assert (
        actual_subscription.collect()[0][Colname.charge_type]
        == e.ChargeType.SUBSCRIPTION.value
    )


class TestWhenChargePeriodStopsAndStartsAgain:
    def test__returns_expected_charge_times(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        first_period_from_date = datetime(2022, 1, 1, 23)
        first_period_to_date = datetime(2022, 1, 3, 23)
        second_period_from_date = datetime(2022, 1, 4, 23)
        second_period_to_date = datetime(2022, 1, 6, 23)
        expected_charge_times = [
            datetime(2022, 1, 1, 23),
            datetime(2022, 1, 2, 23),
            datetime(2022, 1, 4, 23),
            datetime(2022, 1, 5, 23),
        ]

        charge_link_metering_point_periods = (
            factory.create_charge_link_metering_point_periods(
                spark,
                [
                    factory.create_charge_link_metering_point_periods_row(
                        charge_type=e.ChargeType.SUBSCRIPTION,
                        from_date=first_period_from_date,
                        to_date=first_period_to_date,
                    ),
                    factory.create_charge_link_metering_point_periods_row(
                        charge_type=e.ChargeType.SUBSCRIPTION,
                        from_date=second_period_from_date,
                        to_date=second_period_to_date,
                    ),
                ],
            )
        )
        charge_period_prices = factory.create_charge_period_prices(
            spark,
            [
                factory.create_subscription_or_fee_charge_period_prices_row(
                    charge_time=first_period_from_date,
                    from_date=first_period_from_date,
                    to_date=first_period_to_date,
                    charge_type=e.ChargeType.SUBSCRIPTION,
                ),
                factory.create_subscription_or_fee_charge_period_prices_row(
                    charge_time=second_period_from_date,
                    from_date=second_period_from_date,
                    to_date=second_period_to_date,
                    charge_type=e.ChargeType.SUBSCRIPTION,
                ),
            ],
        )

        # Act
        actual_subscription = get_subscription_charges(
            charge_period_prices,
            charge_link_metering_point_periods,
            time_zone="Europe/Copenhagen",
        )

        # Assert
        actual_charge_times = (
            (actual_subscription.select(Colname.charge_time).distinct()).orderBy(
                Colname.charge_time
            )
        ).collect()
        assert len(actual_charge_times) == len(expected_charge_times)
        for charge_time_col in range(len(expected_charge_times)):
            assert (
                actual_charge_times[charge_time_col][0]
                == expected_charge_times[charge_time_col]
            )


@pytest.mark.parametrize(
    "charge_time, from_date, to_date, expected_day_count",
    [
        # leap year
        (datetime(2020, 2, 1, 0), datetime(2020, 2, 1, 0), datetime(2020, 3, 1, 0), 29),
        # non-leap year
        (datetime(2021, 2, 1, 0), datetime(2021, 2, 1, 0), datetime(2021, 3, 1, 0), 28),
    ],
)
def test__get_subscription_charges__split_into_days_between_from_and_to_date(
    spark: SparkSession,
    charge_time: datetime,
    from_date: datetime,
    to_date: datetime,
    expected_day_count: int,
) -> None:
    # Arrange
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.SUBSCRIPTION, from_date=from_date, to_date=to_date
        ),
    ]
    charge_period_prices_rows = [
        factory.create_subscription_or_fee_charge_period_prices_row(
            charge_time=charge_time,
            from_date=from_date,
            to_date=to_date,
            charge_type=e.ChargeType.SUBSCRIPTION,
        ),
    ]

    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )
    charge_period_prices = factory.create_charge_period_prices(
        spark, charge_period_prices_rows
    )

    # Act
    actual_subscription = get_subscription_charges(
        charge_period_prices, charge_link_metering_point_periods
    )

    # Assert
    assert actual_subscription.count() == expected_day_count
