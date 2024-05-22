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
from pyspark.sql import SparkSession

from package.infrastructure import paths


@pytest.mark.parametrize(
    "view_name, has_data",
    [
        (
            f"{paths.EdiResults.DATABASE_NAME}.energy_result_points_per_ga_v1",
            True,
        )
    ],
)
def test__when_balance_fixing__view_has_data_if_expected(
    spark: SparkSession, executed_balance_fixing: None, view_name: str, has_data: bool
) -> None:
    actual = spark.sql(f"SELECT * FROM {view_name}")
    assert actual.count() > 0 if has_data else actual.count() == 0


@pytest.mark.parametrize(
    "view_name, has_data",
    [
        (
            f"{paths.EdiResults.DATABASE_NAME}.energy_result_points_per_ga_v1",
            True,
        )
    ],
)
def test__when_wholesale_fixing__view_has_data_if_expected(
    spark: SparkSession, executed_wholesale_fixing: None, view_name: str, has_data: bool
) -> None:
    actual = spark.sql(f"SELECT * FROM {view_name}")
    assert actual.count() > 0 if has_data else actual.count() == 0
