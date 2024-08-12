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
from pyspark.sql.types import StructType

from databases.wholesale_basis_data_internal.internal_test_factory import (
    create_internal_factory,
)
from package.databases.wholesale_basis_data_internal.schemas import (
    hive_charge_price_information_periods_schema,
    charge_price_points_schema,
    grid_loss_metering_points_schema,
    hive_metering_point_period_schema,
    time_series_points_schema,
)
from package.databases.wholesale_basis_data_internal.schemas.charge_link_periods_schema import (
    hive_charge_link_periods_schema,
)


@pytest.mark.parametrize(
    "basis_data_table_property_name, expected_schema",
    [
        (
            "metering_point_periods",
            hive_metering_point_period_schema,
        ),
        (
            "time_series_points",
            time_series_points_schema,
        ),
        ("charge_link_periods", hive_charge_link_periods_schema),
        (
            "charge_price_information_periods",
            hive_charge_price_information_periods_schema,
        ),
        ("charge_price_points", charge_price_points_schema),
        (
            "grid_loss_metering_points",
            grid_loss_metering_points_schema,
        ),
    ],
)
def test__internal_uses_correct_schema(
    spark: SparkSession,
    internal_table_property_name: str,
    expected_schema: StructType,
):
    # Arrange
    internal_container = create_internal_factory(spark)

    # Act
    # Refer to the property so we can use parameterization
    internal_container_property = getattr(
        internal_container, internal_table_property_name
    )

    # Assert
    assert internal_container_property.schema == expected_schema
