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

import package.codelists as E
from pyspark.sql import SparkSession
from package.calculation.energy.energy_calculation import execute


def test__execute__raises_value_error_when_time_series_quarter_points_has_wrong_schema(
    spark: SparkSession,
) -> None:
    # Arrange
    invalid_time_series_quarter_points = spark.createDataFrame(
        data=[{"Hello": "World"}]
    )

    # Act
    with pytest.raises(ValueError) as excinfo:
        execute(
            batch_id="batch_id",
            batch_process_type=E.ProcessType.BALANCE_FIXING,
            batch_execution_time_start=datetime(2020, 1, 1),
            time_series_quarter_points_df=invalid_time_series_quarter_points,
            grid_loss_responsible_df=spark.createDataFrame(data=[{"Hello": "World"}]),
        )

    # Assert
    assert "Schema mismatch" in str(excinfo.value)
