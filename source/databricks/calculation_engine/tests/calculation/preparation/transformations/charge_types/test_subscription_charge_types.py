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

from calculation.preparation.transformations.charge_types.charge_type_factories import (
    create_metering_point_row,
    create_subscription_or_fee_charges_row,
)
from package.calculation.preparation.transformations import (
    get_subscription_charges,
)
import package.codelists as e
from package.calculation_input.schemas import (
    metering_point_period_schema,
)
from package.calculation.wholesale.schemas.charges_schema import charges_schema
from package.constants import Colname


def test__get_subscription_charges__filters_on_subscription_charge_type(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_rows = [create_metering_point_row()]
    charges_rows = [
        create_subscription_or_fee_charges_row(
            charge_type=e.ChargeType.FEE,
        ),
        create_subscription_or_fee_charges_row(
            charge_type=e.ChargeType.SUBSCRIPTION,
        ),
    ]

    metering_point = spark.createDataFrame(
        metering_point_rows, metering_point_period_schema
    )
    charges = spark.createDataFrame(charges_rows, charges_schema)

    # Act
    actual_subscription = get_subscription_charges(charges, metering_point)

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
    metering_point_rows = [
        create_metering_point_row(from_date=from_date, to_date=to_date)
    ]
    charges_rows = [
        create_subscription_or_fee_charges_row(
            charge_time=charge_time,
            from_date=from_date,
            to_date=to_date,
            charge_type=e.ChargeType.SUBSCRIPTION,
        ),
    ]

    metering_point = spark.createDataFrame(
        metering_point_rows, metering_point_period_schema
    )
    charges = spark.createDataFrame(charges_rows, charges_schema)

    # Act
    actual_subscription = get_subscription_charges(charges, metering_point)

    # Assert
    assert actual_subscription.count() == expected_day_count
