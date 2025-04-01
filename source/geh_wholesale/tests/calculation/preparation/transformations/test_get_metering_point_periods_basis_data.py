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

from geh_common.testing.dataframes.assert_schemas import assert_schema
from pyspark.sql import Row, SparkSession

from geh_wholesale.codelists import (
    MeteringPointResolution,
)
from geh_wholesale.constants import Colname
from geh_wholesale.databases.table_column_names import TableColumnNames
from geh_wholesale.databases.wholesale_basis_data_internal import (
    get_metering_point_periods_basis_data,
)
from geh_wholesale.databases.wholesale_basis_data_internal.schemas.metering_point_periods_schema import (
    metering_point_periods_schema_basis_data,
)
from tests.calculation.preparation.transformations import metering_point_periods_factory


def test__when_valid_input__returns_df_with_expected_schema(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_period_df = metering_point_periods_factory.create(spark)

    # Act
    actual = get_metering_point_periods_basis_data("some-calculation-id", metering_point_period_df)

    # Assert
    assert_schema(actual.schema, metering_point_periods_schema_basis_data)


def test__each_metering_point_has_a_row(spark: SparkSession) -> None:
    # Arrange
    expected_number_of_metering_points = 3

    rows = [
        metering_point_periods_factory.create_row(metering_point_id="1"),
        metering_point_periods_factory.create_row(metering_point_id="2"),
        metering_point_periods_factory.create_row(metering_point_id="3"),
    ]

    metering_point_period_df = metering_point_periods_factory.create(spark, rows)

    # Act
    master_basis_data = get_metering_point_periods_basis_data("some-calculation-id", metering_point_period_df)

    # Assert
    assert master_basis_data.count() == expected_number_of_metering_points


def test__columns_have_expected_values(spark: SparkSession) -> None:
    # Arrange
    expected_dict = {
        TableColumnNames.calculation_id: "some-calculation-id",
        TableColumnNames.metering_point_id: metering_point_periods_factory.DEFAULT_METERING_POINT_ID,
        TableColumnNames.metering_point_type: metering_point_periods_factory.DEFAULT_METERING_POINT_TYPE.value,
        TableColumnNames.settlement_method: metering_point_periods_factory.DEFAULT_SETTLEMENT_METHOD.value,
        TableColumnNames.grid_area_code: metering_point_periods_factory.DEFAULT_GRID_AREA,
        TableColumnNames.resolution: metering_point_periods_factory.DEFAULT_RESOLUTION.value,
        TableColumnNames.from_grid_area_code: metering_point_periods_factory.DEFAULT_FROM_GRID_AREA,
        TableColumnNames.to_grid_area_code: metering_point_periods_factory.DEFAULT_TO_GRID_AREA,
        TableColumnNames.parent_metering_point_id: metering_point_periods_factory.DEFAULT_PARENT_METERING_POINT_ID,
        TableColumnNames.energy_supplier_id: metering_point_periods_factory.DEFAULT_ENERGY_SUPPLIER_ID,
        TableColumnNames.balance_responsible_party_id: metering_point_periods_factory.DEFAULT_BALANCE_RESPONSIBLE_PARTY_ID,
        TableColumnNames.from_date: metering_point_periods_factory.DEFAULT_FROM_DATE,
        TableColumnNames.to_date: metering_point_periods_factory.DEFAULT_TO_DATE,
    }
    expected = Row(**expected_dict)

    row = metering_point_periods_factory.create_row()
    metering_point_period_df = metering_point_periods_factory.create(spark, [row])

    # Act
    master_basis_data_df = get_metering_point_periods_basis_data(
        expected[Colname.calculation_id], metering_point_period_df
    )

    # Assert
    actual = master_basis_data_df.first()
    assert actual == expected


def test__both_hour_and_quarterly_resolution_data_are_in_basis_data(
    spark: SparkSession,
) -> None:
    # Arrange
    expected_number_of_metering_points = 2
    rows = [
        metering_point_periods_factory.create_row(metering_point_id="1", resolution=MeteringPointResolution.QUARTER),
        metering_point_periods_factory.create_row(metering_point_id="2", resolution=MeteringPointResolution.HOUR),
    ]

    metering_point_period_df = metering_point_periods_factory.create(spark, rows)

    # Act
    master_basis_data = get_metering_point_periods_basis_data("some-calculation-id", metering_point_period_df)

    # Assert
    assert master_basis_data.count() == expected_number_of_metering_points
