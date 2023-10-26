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
from decimal import Decimal
from datetime import datetime
from pyspark.sql import SparkSession, Row
from pyspark.sql.types import (
    TimestampType,
)

from package.constants import Colname
from package.codelists import MeteringPointResolution, QuantityQuality
from package.calculation.preparation.transformations.hour_to_quarter import (
    transform_hour_to_quarter,
)
from package.calculation.energy.schemas import (
    basis_data_time_series_points_schema,
    time_series_quarter_points_schema,
)


def basis_data_time_series_points_row(
    grid_area: str = "805",
    to_grid_area: str = "805",
    from_grid_area: str = "806",
    metering_point_id: str = "the_metering_point_id",
    metering_point_type: str = "the_metering_point_type",
    resolution: MeteringPointResolution = MeteringPointResolution.HOUR,
    observation_time: TimestampType() = datetime(2020, 1, 1, 0, 0),
    quantity: Decimal = Decimal("4.444444"),
    quality: QuantityQuality = QuantityQuality.ESTIMATED,
    energy_supplier_id: str = "the_energy_supplier_id",
    balance_responsible_id: str = "the_balance_responsible_id",
    settlement_method: str = "the_settlement_method",
) -> Row:
    row = {
        Colname.grid_area: grid_area,
        Colname.to_grid_area: to_grid_area,
        Colname.from_grid_area: from_grid_area,
        Colname.metering_point_id: metering_point_id,
        Colname.metering_point_type: metering_point_type,
        Colname.resolution: resolution.value,
        Colname.observation_time: observation_time,
        Colname.quantity: quantity,
        Colname.quality: quality.value,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.balance_responsible_id: balance_responsible_id,
        Colname.settlement_method: settlement_method,
    }

    return Row(**row)


def test__transform_hour_to_quarter__when_invalid_input_schema__raise_assertion_error(
    spark: SparkSession,
):
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
    rows = [basis_data_time_series_points_row()]
    basis_data_time_series_points = spark.createDataFrame(
        rows, basis_data_time_series_points_schema
    )

    # Act
    actual = transform_hour_to_quarter(basis_data_time_series_points)

    # Assert
    assert actual.count() == 4
    assert actual.collect()[0][Colname.quantity] == Decimal("1.111111")


def test__transform_hour_to_quarter__when_valid_input__returns_expected_schema(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [basis_data_time_series_points_row()]
    basis_data_time_series_points = spark.createDataFrame(
        rows, basis_data_time_series_points_schema
    )

    # Act
    actual = transform_hour_to_quarter(basis_data_time_series_points)

    # Assert
    assert actual.schema == time_series_quarter_points_schema
