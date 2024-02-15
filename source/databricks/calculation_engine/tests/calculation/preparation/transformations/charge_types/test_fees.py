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

from pyspark.sql import SparkSession

import tests.calculation.charges_factory as factory
from package.calculation.preparation.transformations.charge_types import (
    get_fee_charges,
)
import package.codelists as e

from package.calculation.wholesale.schemas.charges_schema import (
    charges_schema,
    charge_link_metering_points_schema,
)
from package.constants import Colname


def test__get_fee_charges__filters_on_fee_charge_type(
    spark: SparkSession,
) -> None:
    # Arrange
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_points_row(charge_type=e.ChargeType.FEE),
        factory.create_charge_link_metering_points_row(
            charge_type=e.ChargeType.SUBSCRIPTION
        ),
        factory.create_charge_link_metering_points_row(charge_type=e.ChargeType.TARIFF),
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

    charge_link_metering_points = spark.createDataFrame(
        charge_link_metering_points_rows, charge_link_metering_points_schema
    )
    charges = spark.createDataFrame(charges_rows, charges_schema)

    # Act
    actual_fee = get_fee_charges(charges, charge_link_metering_points)

    # Assert
    assert actual_fee.collect()[0][Colname.charge_type] == e.ChargeType.FEE.value
