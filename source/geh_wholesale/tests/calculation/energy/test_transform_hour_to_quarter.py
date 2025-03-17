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

from pyspark.sql import SparkSession

from package.constants import Colname
from package.codelists import MeteringPointResolution
from package.calculation.energy.hour_to_quarter import (
    transform_hour_to_quarter,
)
import tests.calculation.preparation.transformations.prepared_metering_point_time_series_factory as factory

DEFAULT_QUANTITY = Decimal("4.444000")


def test__transform_hour_to_quarter__when_valid_input__split_basis_data_time_series(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        factory.create_row(
            resolution=MeteringPointResolution.HOUR, quantity=DEFAULT_QUANTITY
        ),
        factory.create_row(
            resolution=MeteringPointResolution.QUARTER, quantity=DEFAULT_QUANTITY
        ),
    ]

    prepared_metering_point_time_series = factory.create(spark, rows)

    # Act
    actual = transform_hour_to_quarter(prepared_metering_point_time_series)

    # Assert
    assert actual.df.count() == 5
    # Check that hourly quantity is divided by 4
    assert actual.df.collect()[0][Colname.quantity] == DEFAULT_QUANTITY / 4
    # Check that quarterly quantity is not divided by 4
    assert actual.df.collect()[4][Colname.quantity] == DEFAULT_QUANTITY
