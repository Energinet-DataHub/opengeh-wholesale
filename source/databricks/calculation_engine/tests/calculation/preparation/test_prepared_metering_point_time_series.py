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
from pyspark.sql import SparkSession, Row

from package.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)


def test__constructor__when_invalid_input_schema__raise_assertion_error(
    spark: SparkSession,
) -> None:
    # Arrange
    invalid_time_series = spark.createDataFrame(data=[Row(**({"Hello": "World"}))])

    # Act & Assert
    with pytest.raises(Exception):
        PreparedMeteringPointTimeSeries(invalid_time_series)
