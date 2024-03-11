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
from decimal import Decimal

import pytest
from pyspark.sql import SparkSession, Row

from calculation.basis_data_time_series_points_factories import (
    basis_data_time_series_points_row,
)
from package.calculation.energy.hour_to_quarter import (
    transform_hour_to_quarter,
    metering_point_time_series_schema,
)
from package.codelists import MeteringPointResolution
from package.constants import Colname


def test__transform_hour_to_quarter__when_invalid_input_schema__raise_assertion_error(
    spark: SparkSession,
) -> None:
    # Arrange
    basis_data_time_series_points = spark.createDataFrame(
        data=[Row(**({"Hello": "World"}))]
    )

    # Act & Assert
    with pytest.raises(AssertionError):
        transform_hour_to_quarter(basis_data_time_series_points)


def test__transform_hour_to_quarter__when_valid_input__split_basis_data_time_series(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        basis_data_time_series_points_row(resolution=MeteringPointResolution.HOUR),
        basis_data_time_series_points_row(resolution=MeteringPointResolution.QUARTER),
    ]
    basis_data_time_series_points = spark.createDataFrame(
        rows, metering_point_time_series_schema
    )

    # Act
    actual = transform_hour_to_quarter(basis_data_time_series_points)

    # Assert
    assert actual.df.count() == 5
    # Check that hourly quantity is divided by 4
    assert actual.df.collect()[0][Colname.quantity] == Decimal("1.111111")
    # Check that quarterly quantity is not divided by 4
    assert actual.df.collect()[4][Colname.quantity] == Decimal("4.444444")
