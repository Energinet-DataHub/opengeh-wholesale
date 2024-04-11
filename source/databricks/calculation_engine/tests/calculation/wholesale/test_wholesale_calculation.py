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

from package.calculation.calculator_args import CalculatorArgs
from package.calculation.preparation.data_structures.prepared_charges import (
    PreparedChargesContainer,
)
from package.calculation.wholesale import execute
from package.codelists import ChargeResolution

import calculation.wholesale.factories.prepared_tariffs_factory as tariffs_factory
import calculation.wholesale.factories.prepared_subscriptions_factory as subscriptions_factory
import calculation.wholesale.factories.prepared_fees_factory as fees_factory


def test__execute__when_tariff_schema_is_valid__does_not_raise(
    spark: SparkSession, any_calculator_args_for_wholesale: CalculatorArgs
) -> None:
    # Arrange
    tariffs_hourly_df = tariffs_factory.create(
        spark, data=[tariffs_factory.create_row()]
    )
    tariffs_daily_df = tariffs_factory.create(
        spark,
        data=[tariffs_factory.create_row(resolution=ChargeResolution.DAY)],
    )
    prepared_subscriptions = subscriptions_factory.create(spark)
    prepared_fees = fees_factory.create(spark)

    prepared_charges = PreparedChargesContainer(
        fees=prepared_fees,
        subscriptions=prepared_subscriptions,
        hourly_tariffs=tariffs_hourly_df,
        daily_tariffs=tariffs_daily_df,
    )

    # Act
    execute(
        any_calculator_args_for_wholesale,
        prepared_charges,
    )

    # Assert
    # If execute raises an exception, the test fails automatically
