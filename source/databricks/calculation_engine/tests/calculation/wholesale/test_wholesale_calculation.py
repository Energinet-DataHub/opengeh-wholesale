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

from calculation.wholesale.test_tariff_calculators import _create_tariff_row
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.wholesale import execute
from package.calculation.wholesale.schemas.tariffs_schema import tariff_schema
from package.codelists import ChargeResolution


def test__execute__when_tariff_schema_is_valid__does_not_raise(
    spark: SparkSession, any_calculator_args: CalculatorArgs
) -> None:
    # Arrange
    tariffs_hourly_df = spark.createDataFrame(
        data=[_create_tariff_row()], schema=tariff_schema
    )
    tariffs_daily_df = spark.createDataFrame(
        data=[_create_tariff_row(resolution=ChargeResolution.DAY)], schema=tariff_schema
    )

    # Act
    execute(
        any_calculator_args,
        tariffs_hourly_df,
        tariffs_daily_df,
    )

    # Assert
    # If execute raises an exception, the test fails automatically


def test__execute__when_tariff_schema_is_invalid__raises_assertion_error(
    spark: SparkSession, any_calculator_args: CalculatorArgs
) -> None:
    # Arrange
    data = [("John", "Dow")]
    tariffs_hourly_df: DataFrame = spark.createDataFrame(data)
    tariffs_daily_df: DataFrame = spark.createDataFrame(data)

    # Act & Assert
    with pytest.raises(AssertionError):
        execute(
            any_calculator_args,
            tariffs_hourly_df,
            tariffs_daily_df,
        )
