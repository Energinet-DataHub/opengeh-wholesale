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
import datetime

from pyspark.sql import SparkSession, Row

from calculation.preparation.transformations import metering_point_periods_factory
from package.calculation.preparation.transformations.basis_data import (
    get_metering_point_periods_basis_data,
)
from package.codelists import (
    MeteringPointResolution,
)
from package.constants import Colname, MeteringPointPeriodColname


def test__get_master_basis_data_has_expected_columns(spark: SparkSession) -> None:
    # Arrange
    rows = [
        metering_point_periods_factory.create_row(),
        metering_point_periods_factory.create_row(
            from_date=datetime.datetime(2022, 6, 10, 22),
            to_date=datetime.datetime(2022, 6, 12, 22),
        ),
    ]

    metering_point_period_df = metering_point_periods_factory.create(spark, rows)

    # Act
    master_basis_data = get_metering_point_periods_basis_data(
        "some-calculation-id", metering_point_period_df
    )

    # Assert
    assert master_basis_data.columns == [
        "calculation_id",
        "resolution",
        "metering_point_id",
        "from_date",
        "to_date",
        "grid_area",
        "to_grid_area",
        "from_grid_area",
        "metering_point_type",
        "settlement_method",
        "energy_supplier_id",
        "balance_responsible_id",
    ]


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
    master_basis_data = get_metering_point_periods_basis_data(
        "some-calculation-id", metering_point_period_df
    )

    # Assert
    assert master_basis_data.count() == expected_number_of_metering_points


def test__columns_have_expected_values(spark: SparkSession) -> None:
    # Arrange
    expected_dict = {
        MeteringPointPeriodColname.calculation_id: "foo",
        MeteringPointPeriodColname.resolution: metering_point_periods_factory.DEFAULT_RESOLUTION.value,
        MeteringPointPeriodColname.metering_point_id: metering_point_periods_factory.DEFAULT_METERING_POINT_ID,
        MeteringPointPeriodColname.from_date: metering_point_periods_factory.DEFAULT_FROM_DATE,
        MeteringPointPeriodColname.to_date: metering_point_periods_factory.DEFAULT_TO_DATE,
        MeteringPointPeriodColname.grid_area: metering_point_periods_factory.DEFAULT_GRID_AREA,
        MeteringPointPeriodColname.to_grid_area: metering_point_periods_factory.DEFAULT_TO_GRID_AREA,
        MeteringPointPeriodColname.from_grid_area: metering_point_periods_factory.DEFAULT_FROM_GRID_AREA,
        MeteringPointPeriodColname.metering_point_type: metering_point_periods_factory.DEFAULT_METERING_POINT_TYPE.value,
        MeteringPointPeriodColname.settlement_method: metering_point_periods_factory.DEFAULT_SETTLEMENT_METHOD.value,
        MeteringPointPeriodColname.energy_supplier_id: metering_point_periods_factory.DEFAULT_ENERGY_SUPPLIER_ID,
        MeteringPointPeriodColname.balance_responsible_id: metering_point_periods_factory.DEFAULT_BALANCE_RESPONSIBLE_ID,
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
        metering_point_periods_factory.create_row(
            metering_point_id="1", resolution=MeteringPointResolution.QUARTER
        ),
        metering_point_periods_factory.create_row(
            metering_point_id="2", resolution=MeteringPointResolution.HOUR
        ),
    ]

    metering_point_period_df = metering_point_periods_factory.create(spark, rows)

    # Act
    master_basis_data = get_metering_point_periods_basis_data(
        "some-calculation-id", metering_point_period_df
    )

    # Assert
    assert master_basis_data.count() == expected_number_of_metering_points
