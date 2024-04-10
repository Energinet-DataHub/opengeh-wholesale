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

from calculation.preparation.transformations import (
    prepared_metering_point_time_series_factory,
)
from package.calculation.basis_data.schemas import time_series_point_schema
from package.calculation.basis_data.basis_data import (
    get_time_series_basis_data,
)
from package.common import assert_schema


def test__when_valid_input__returns_df_with_expected_schema(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_time_series = prepared_metering_point_time_series_factory.create(
        spark
    )

    # Act
    actual = get_time_series_basis_data(
        "some-calculation-id", metering_point_time_series
    )

    # Assert
    assert_schema(actual.schema, time_series_point_schema, ignore_decimal_scale=True)
