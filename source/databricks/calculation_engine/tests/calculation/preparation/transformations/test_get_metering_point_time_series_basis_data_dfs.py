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

# TODO BJM

from decimal import Decimal

import pytest
from pyspark.sql import SparkSession
from pyspark.sql.functions import lit
from pyspark.sql.types import (
    DecimalType,
)

from calculation.preparation.transformations import (
    prepared_metering_point_time_series_factory,
)
from package.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from package.calculation.preparation.transformations.basis_data import (
    get_time_series_basis_data,
)
from package.codelists import MeteringPointResolution

minimum_quantity = Decimal("0.001")

# def test__has_correct_number_of_quantity_columns_according_to_dst(
#     spark: SparkSession,
#     period_start,
#     resolution,
#     number_of_points,
#     expected_number_of_quarter_quantity_columns,
#     expected_number_of_hour_quantity_columns,
# ):
#     # Arrange
#     row = prepared_metering_point_time_series_factory.create_row(
#         observation_time=period_start,
#         resolution=resolution,
#         number_of_points=number_of_points,
#     )
#     metering_point_time_series = prepared_metering_point_time_series_factory.create(
#         spark, [row]
#     )
#
#     # Act
#     quarter_df, hour_df = get_time_series_basis_data(
#         "some-calculation-id", metering_point_time_series
#     )
#
#     # Assert
#     quantity_columns_quarter = list(
#         filter(lambda column: column.startswith("ENERGYQUANTITY"), quarter_df.columns)
#     )
#     quantity_columns_hour = list(
#         filter(lambda column: column.startswith("ENERGYQUANTITY"), hour_df.columns)
#     )
#     assert len(quantity_columns_quarter) == expected_number_of_quarter_quantity_columns
#     assert len(quantity_columns_hour) == expected_number_of_hour_quantity_columns
