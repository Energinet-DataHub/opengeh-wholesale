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
from pyspark.sql import DataFrame, SparkSession

from package.calculation.calculator_args import CalculatorArgs
from package.calculation.preparation.prepared_tariffs import prepared_tariffs_schema
from package.calculation.wholesale import execute
from package.codelists import ChargeResolution

import tests.calculation.wholesale.prepared_tariff_factory as factory


def test__execute__when_tariff_schema_is_valid__does_not_raise(
    spark: SparkSession, any_calculator_args: CalculatorArgs
) -> None:
    # Arrange
    tariffs_hourly_df = factory.create_prepared_tariffs(
        spark, data=[factory.create_prepared_tariffs_row()]
    )
    tariffs_daily_df = factory.create_prepared_tariffs(
        spark,
        data=[factory.create_prepared_tariffs_row(resolution=ChargeResolution.DAY)],
    )

    # Act
    execute(
        any_calculator_args,
        tariffs_hourly_df,
        tariffs_daily_df,
    )

    # Assert
    # If execute raises an exception, the test fails automatically
