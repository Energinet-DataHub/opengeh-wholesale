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
from package.constants import Colname


def test__get_fee_charges__filters_on_fee_charge_type(
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
            charge_type=e.ChargeType.TARIFF
        ),
    ]
    charge_master_data_rows = [
        factory.create_charge_master_data_row(
            charge_type=e.ChargeType.FEE, resolution=e.ChargeResolution.MONTH
        ),
        factory.create_charge_master_data_row(
            charge_type=e.ChargeType.SUBSCRIPTION, resolution=e.ChargeResolution.MONTH
        ),
        factory.create_charge_master_data_row(),
    ]
    charge_prices_rows = [
        factory.create_charge_prices_row(
            charge_type=e.ChargeType.FEE,
        ),
        factory.create_charge_prices_row(
            charge_type=e.ChargeType.SUBSCRIPTION,
        ),
        factory.create_charge_prices_row(),
    ]

    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )
    charge_master_data = spark.createDataFrame(charge_master_data_rows)
    charge_prices = spark.createDataFrame(charge_prices_rows)

    # Act
    actual_fee = get_fee_charges(
        charge_master_data, charge_prices, charge_link_metering_point_periods
    )

    # Assert
    assert actual_fee.collect()[0][Colname.charge_type] == e.ChargeType.FEE.value
