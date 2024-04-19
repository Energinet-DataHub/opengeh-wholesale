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
from decimal import Decimal

from pyspark.sql import SparkSession
from pyspark.sql.types import Row

from package.constants import Colname
from package.codelists import MeteringPointResolution, QuantityQuality
from package.calculation.energy.quarter_to_hour import (
    transform_quarter_to_hour,
)
from package.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    prepared_metering_point_time_series_schema,
    PreparedMeteringPointTimeSeries,
)


def basis_data_time_series_points_row(
    grid_area: str = "805",
    to_grid_area: str = "805",
    from_grid_area: str = "806",
    metering_point_id: str = "the_metering_point_id",
    metering_point_type: str = "the_metering_point_type",
    resolution: MeteringPointResolution = MeteringPointResolution.HOUR,
    observation_time: datetime = datetime(2020, 1, 1, 0, 0),
    quantity: Decimal = Decimal("4.444000"),
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


def test__transform_quarter_to_hour__when_valid_input__split_basis_data_time_series(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        basis_data_time_series_points_row(resolution=MeteringPointResolution.HOUR),
        basis_data_time_series_points_row(resolution=MeteringPointResolution.QUARTER),
        basis_data_time_series_points_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 0, 15),
        ),
        basis_data_time_series_points_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 0, 30),
        ),
        basis_data_time_series_points_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 0, 45),
        ),
        basis_data_time_series_points_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 1, 00),
        ),
        basis_data_time_series_points_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 1, 15),
        ),
        basis_data_time_series_points_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 1, 30),
        ),
        basis_data_time_series_points_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 1, 45),
        ),
    ]
    basis_data_time_series_points = spark.createDataFrame(
        rows, prepared_metering_point_time_series_schema
    )
    metering_point_time_series = PreparedMeteringPointTimeSeries(
        basis_data_time_series_points
    )

    # Act
    actual = transform_quarter_to_hour(metering_point_time_series)

    # Assert
    assert actual.df.count() == 3
    assert actual.df.collect()[0][Colname.quantity] == Decimal("4.444000")
    # Check that quarterly quantity is not divided by 4
    assert actual.df.collect()[1][Colname.quantity] == Decimal("17.776000")
    # Check that quarterly quantity is not divided by 4
    assert actual.df.collect()[2][Colname.quantity] == Decimal("17.776000")
