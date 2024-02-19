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
from package.calculation.wholesale.schemas.charges_schema import (
    charges_schema,
    charge_link_metering_points_schema,
)
from package.constants import Colname
import package.codelists as e

import tests.calculation.charges_factory as factory


def test__get_subscription_charges__filters_on_subscription_charge_type(
    spark: SparkSession,
) -> None:
    # Arrange
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_points_row(charge_type=e.ChargeType.FEE),
        factory.create_charge_link_metering_points_row(
            charge_type=e.ChargeType.SUBSCRIPTION
        ),
        factory.create_charge_link_metering_points_row(charge_type=e.ChargeType.FEE),
    ]
    charges_rows = [
        factory.create_subscription_or_fee_charges_row(
            charge_type=e.ChargeType.FEE,
        ),
        factory.create_subscription_or_fee_charges_row(
            charge_type=e.ChargeType.SUBSCRIPTION,
        ),
        factory.create_tariff_charges_row(),
    ]

    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )
    charges = spark.createDataFrame(charges_rows, charges_schema)

    # Act
    actual_subscription = get_subscription_charges(
        charges, charge_link_metering_point_periods
    )

    # Assert
    assert (
        actual_subscription.collect()[0][Colname.charge_type]
        == e.ChargeType.SUBSCRIPTION.value
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
        factory.create_charge_link_metering_points_row(
            charge_type=e.ChargeType.SUBSCRIPTION, from_date=from_date, to_date=to_date
        ),
    ]
    charges_rows = [
        factory.create_subscription_or_fee_charges_row(
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
    charges = spark.createDataFrame(charges_rows, charges_schema)

    # Act
    actual_subscription = get_subscription_charges(
        charges, charge_link_metering_point_periods
    )

    # Assert
    assert actual_subscription.count() == expected_day_count
