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
import pyspark.sql.functions as f
import pytest
from pyspark.sql import SparkSession

from package.infrastructure import paths
from . import configuration as c

BASIS_DATA_TABLE_NAMES = [
    paths.METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME,
    paths.TIME_SERIES_BASIS_DATA_TABLE_NAME,
]


@pytest.mark.parametrize(
    "basis_data_table_name",
    BASIS_DATA_TABLE_NAMES,
)
def test__when_energy_calculation__data_is_stored(
    spark: SparkSession,
    executed_balance_fixing: None,
    basis_data_table_name: str,
) -> None:
    # Arrange
    actual = spark.read.table(
        f"{paths.BASIS_DATA_DATABASE_NAME}.{basis_data_table_name}"
    ).where(f.col("calculation_id") == c.executed_balance_fixing_calculation_id)

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert actual.count() > 0


@pytest.mark.parametrize(
    "basis_data_table_name",
    BASIS_DATA_TABLE_NAMES,
)
def test__when_wholesale_calculation__data_is_stored(
    spark: SparkSession,
    executed_wholesale_fixing: None,
    basis_data_table_name: str,
) -> None:
    # Arrange
    actual = spark.read.table(
        f"{paths.BASIS_DATA_DATABASE_NAME}.{basis_data_table_name}"
    ).where(f.col("calculation_id") == c.executed_wholesale_calculation_id)

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert actual.count() > 0
