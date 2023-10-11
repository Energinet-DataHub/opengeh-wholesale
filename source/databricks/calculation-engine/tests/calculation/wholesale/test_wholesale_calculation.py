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
from unittest.mock import patch

import pytest
from pyspark.sql import DataFrame

from calculation.wholesale.test_tariff_calculators import _create_tariff_hour_row
from package import calculation_output
from package.calculation.wholesale import execute
from package.calculation.wholesale.schemas.tariffs_schema import tariff_schema
from package.calculation_output import WholesaleCalculationResultWriter


@patch.object(calculation_output, WholesaleCalculationResultWriter.__name__)
def test__execute__asserts_tariff_schema__is_valid(
    wholesale_calculation_result_writer_mock: WholesaleCalculationResultWriter, spark
):
    # Arrange
    tariffs_hourly_df: DataFrame = spark.createDataFrame(
        data=[_create_tariff_hour_row()], schema=tariff_schema.tariff_schema
    )
    period_start_datetime: datetime = datetime(2020, 1, 1, 0, 0)

    # Act
    try:
        execute(
            wholesale_calculation_result_writer_mock,
            tariffs_hourly_df,
            period_start_datetime,
        )
    except ValueError:
        raise Exception("Test failed. Exception raised!")


@patch.object(calculation_output, WholesaleCalculationResultWriter.__name__)
def test__execute__asserts_tariff_schema__throws_exception(
    wholesale_calculation_result_writer_mock: WholesaleCalculationResultWriter, spark
):
    # Arrange
    data = [("John", "Dow")]
    tariffs_hourly_df: DataFrame = spark.createDataFrame(data)
    period_start_datetime: datetime = datetime(2020, 1, 1, 0, 0)

    # Act & Assert
    with pytest.raises(Exception):
        execute(
            wholesale_calculation_result_writer_mock,
            tariffs_hourly_df,
            period_start_datetime,
        )
