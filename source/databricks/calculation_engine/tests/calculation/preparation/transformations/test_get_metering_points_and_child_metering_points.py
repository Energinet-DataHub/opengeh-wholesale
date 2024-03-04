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
import pytest
import tests.calculation.preparation.transformations.metering_point_periods_factory as factory
from package.calculation.preparation.transformations import (
    get_metering_points_and_child_metering_points,
)
from package.codelists import MeteringPointType
from package.constants import Colname


@pytest.mark.parametrize(
    "metering_point_type, expected_count",
    [
        (MeteringPointType.CONSUMPTION, 1),
        (MeteringPointType.PRODUCTION, 1),
        (MeteringPointType.EXCHANGE, 0),
    ],
)
def test__filters_on_metering_point_type(
    metering_point_type: MeteringPointType,
    expected_count: int,
    spark: SparkSession,
):
    # Arrange
    row = factory.create_row(
        metering_point_type=metering_point_type,
    )
    metering_point_periods = factory.create(spark, row)

    # Act
    actual = get_metering_points_and_child_metering_points(
        metering_point_periods,
    )

    # Assert
    assert actual.count() == expected_count


@pytest.mark.parametrize(
    "metering_point_type",
    [
        MeteringPointType.VE_PRODUCTION,
        MeteringPointType.NET_PRODUCTION,
        MeteringPointType.SUPPLY_TO_GRID,
        MeteringPointType.CONSUMPTION_FROM_GRID,
        MeteringPointType.WHOLESALE_SERVICES_INFORMATION,
        MeteringPointType.OWN_PRODUCTION,
        MeteringPointType.NET_FROM_GRID,
        MeteringPointType.NET_TO_GRID,
        MeteringPointType.TOTAL_CONSUMPTION,
        MeteringPointType.ELECTRICAL_HEATING,
        MeteringPointType.NET_CONSUMPTION,
        MeteringPointType.EFFECT_SETTLEMENT,
    ],
)
def test__filters_on_child_metering_point_type(
    metering_point_type: MeteringPointType,
    spark: SparkSession,
):
    # Arrange
    row = factory.create_row(
        metering_point_type=metering_point_type,
    )
    metering_point_periods = factory.create(spark, row)

    # Act
    actual = get_metering_points_and_child_metering_points(
        metering_point_periods,
    )

    # Assert
    assert actual.count() == 1


def test__child_metering_point_get_energy_supplier_from_parent_metering_point(
    spark: SparkSession,
):
    # Arrange
    rows = [
        factory.create_row(
            metering_point_id="parent_metering_point_id",
            metering_point_type=MeteringPointType.CONSUMPTION,
            energy_supplier_id="energy_supplier_id_from_parent",
        ),
        factory.create_row(
            parent_metering_point_id="parent_metering_point_id",
            metering_point_type=MeteringPointType.NET_CONSUMPTION,
            energy_supplier_id=None,
        ),
    ]
    metering_point_periods = factory.create(spark, rows)

    # Act
    actual = get_metering_points_and_child_metering_points(
        metering_point_periods,
    )

    # Assert
    assert actual.count() == 2
    actual = actual.collect()
    assert actual[0][Colname.energy_supplier_id] == "energy_supplier_id_from_parent"
    assert actual[1][Colname.energy_supplier_id] == "energy_supplier_id_from_parent"
