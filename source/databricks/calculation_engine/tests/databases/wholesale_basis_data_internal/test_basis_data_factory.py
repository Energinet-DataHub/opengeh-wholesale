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
from pyspark.sql.types import (
    StructType,
    StructField,
    StringType,
    DecimalType,
    TimestampType,
)

from package.databases.table_column_names import TableColumnNames
from package.databases.wholesale_basis_data_internal.schemas import (
    charge_price_information_periods_schema,
    charge_price_points_schema,
    grid_loss_metering_point_ids_schema,
)
from package.databases.wholesale_basis_data_internal.schemas.charge_link_periods_schema import (
    charge_link_periods_schema,
)
from package.databases.wholesale_basis_data_internal.schemas.metering_point_periods_schema import (
    metering_point_periods_schema_basis_data,
)
from tests.databases.wholesale_basis_data_internal.basis_data_test_factory import (
    create_basis_data_factory,
)

# TODO JVM: Schema need for a test and need to be removed when delta table is fixed
time_series_points_schema_temp = StructType(
    [
        StructField(TableColumnNames.calculation_id, StringType(), False),
        StructField(TableColumnNames.metering_point_id, StringType(), False),
        StructField(TableColumnNames.quantity, DecimalType(18, 3), False),
        StructField(TableColumnNames.quality, StringType(), False),
        StructField(TableColumnNames.observation_time, TimestampType(), False),
        StructField(TableColumnNames.metering_point_type, StringType(), False),
        StructField(TableColumnNames.resolution, StringType(), False),
        StructField(TableColumnNames.grid_area_code, StringType(), False),
        StructField(TableColumnNames.energy_supplier_id, StringType(), True),
    ]
)


@pytest.mark.parametrize(
    "basis_data_table_property_name, expected_schema",
    [
        (
            "metering_point_periods",
            metering_point_periods_schema_basis_data,
        ),
        (
            "time_series_points",
            time_series_points_schema_temp,  # TODO JVM: replace with real schema from time_series_points_schema, when delta table and code has the same nullability
        ),
        ("charge_link_periods", charge_link_periods_schema),
        (
            "charge_price_information_periods",
            charge_price_information_periods_schema,
        ),
        ("charge_price_points", charge_price_points_schema),
        (
            "grid_loss_metering_points",
            grid_loss_metering_point_ids_schema,
        ),
    ],
)
def test__basis_data_uses_correct_schema(
    spark: SparkSession,
    basis_data_table_property_name: str,
    expected_schema: StructType,
) -> None:
    # Arrange
    basis_data_container = create_basis_data_factory(spark)

    # Act
    # Refer to the property so we can use parameterization
    basis_data_container_property = getattr(
        basis_data_container, basis_data_table_property_name
    )

    # Assert
    assert basis_data_container_property.schema == expected_schema
