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

import calculation.preparation.transformations.charge_types.charges_factory as factory
from package.calculation.preparation.transformations import (
    get_fee_charges,
)
import package.codelists as e
from package.calculation_input.schemas import (
    metering_point_period_schema,
)
from package.calculation.wholesale.schemas.charges_schema import charges_schema
from package.constants import Colname


def test__get_fee_charges__filters_on_fee_charge_type(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_rows = [factory.create_metering_point_row()]
    charges_rows = [
        factory.create_subscription_or_fee_charges_row(
            charge_type=e.ChargeType.FEE,
        ),
        factory.create_subscription_or_fee_charges_row(
            charge_type=e.ChargeType.SUBSCRIPTION,
        ),
        factory.create_tariff_charges_row(),
    ]

    metering_point = spark.createDataFrame(
        metering_point_rows, metering_point_period_schema
    )
    charges = spark.createDataFrame(charges_rows, charges_schema)

    # Act
    actual_fee = get_fee_charges(charges, metering_point)

    # Assert
    assert actual_fee.collect()[0][Colname.charge_type] == e.ChargeType.FEE.value
