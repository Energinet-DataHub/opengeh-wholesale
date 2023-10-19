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
from package.codelists import MeteringPointType, MeteringPointResolution
from package.calculation.preparation.transformations.basis_data import (
    get_master_basis_data_df,
)
from datetime import datetime

from package.constants import Colname


@pytest.fixture(scope="module")
def metering_point_period_df_factory(spark, timestamp_factory):
    def factory(
        metering_point_id="the-metering-point",
        grid_area_code="some-grid-area",
        from_date: datetime = timestamp_factory("2022-06-08T22:00:00.000Z"),
        to_date: datetime = timestamp_factory("2022-06-10T22:00:00.000Z"),
        metering_point_type=MeteringPointType.PRODUCTION.value,
        from_grid_area="some-from-grid-area",
        to_grid_area="some-to-grid-area",
        settlement_method="some-settlement-method",
        resolution=MeteringPointResolution.HOUR.value,
        energy_supplier_id="some-energy-supplier-id",
    ):
        row = {
            Colname.metering_point_id: metering_point_id,
            Colname.grid_area: grid_area_code,
            Colname.metering_point_type: metering_point_type,
            Colname.from_date: from_date,
            Colname.to_date: to_date,
            Colname.from_grid_area: from_grid_area,
            Colname.to_grid_area: to_grid_area,
            Colname.settlement_method: settlement_method,
            Colname.resolution: resolution,
            Colname.energy_supplier_id: energy_supplier_id,
        }
        return spark.createDataFrame([row])

    return factory


def test__get_master_basis_data_has_expected_columns(
    metering_point_period_df_factory, timestamp_factory
) -> None:
    # Arrange
    metering_point_period_df = metering_point_period_df_factory().union(
        metering_point_period_df_factory(
            from_date=timestamp_factory("2022-06-10T22:00:00.000Z"),
            to_date=timestamp_factory("2022-06-12T22:00:00.000Z"),
        )
    )

    # Act
    master_basis_data = get_master_basis_data_df(metering_point_period_df)

    # Assert
    assert master_basis_data.columns == [
        "METERINGPOINTID",
        "VALIDFROM",
        "VALIDTO",
        "GRIDAREA",
        "TOGRIDAREA",
        "FROMGRIDAREA",
        "TYPEOFMP",
        "SETTLEMENTMETHOD",
        "ENERGYSUPPLIERID",
    ]


def test__each_meteringpoint_has_a_row(metering_point_period_df_factory):
    # Arrange
    expected_number_of_metering_points = 3
    metering_point_period_df = (
        metering_point_period_df_factory(metering_point_id="1")
        .union(metering_point_period_df_factory(metering_point_id="2"))
        .union(metering_point_period_df_factory(metering_point_id="3"))
    )

    # Act
    master_basis_data = get_master_basis_data_df(metering_point_period_df)

    # Assert
    assert master_basis_data.count() == expected_number_of_metering_points


def test__columns_have_expected_values(
    metering_point_period_df_factory, timestamp_factory
):
    # Arrange
    expected_meteringpoint_id = "the-metering-point"
    expected_grid_area_code = "some-grid-area"
    expected_from_date = timestamp_factory("2022-06-08T22:00:00.000Z")
    expected_to_date = timestamp_factory("2022-06-09T22:00:00.000Z")
    expected_meteringpoint_type = MeteringPointType.PRODUCTION.value
    expected_from_grid_area = "some-from-grid-area"
    expected_to_grid_area = "some-to-grid-area"
    expected_settlement_method = "some-settlement-method"
    expected_energy_supplier_id = "the-energy-supplier-id"

    metering_point_period_df = metering_point_period_df_factory(
        metering_point_id=expected_meteringpoint_id,
        grid_area_code=expected_grid_area_code,
        from_date=expected_from_date,
        to_date=expected_to_date,
        metering_point_type=MeteringPointType.PRODUCTION.value,
        from_grid_area=expected_from_grid_area,
        to_grid_area=expected_to_grid_area,
        settlement_method=expected_settlement_method,
        energy_supplier_id=expected_energy_supplier_id,
    )

    # Act
    master_basis_data_df = get_master_basis_data_df(metering_point_period_df)

    # Assert
    actual = master_basis_data_df.first()

    assert actual.GRIDAREA == expected_grid_area_code
    assert actual.METERINGPOINTID == expected_meteringpoint_id
    assert actual.VALIDFROM == expected_from_date
    assert actual.VALIDTO == expected_to_date
    assert actual.GRIDAREA == expected_grid_area_code
    assert actual.TOGRIDAREA == expected_to_grid_area
    assert actual.FROMGRIDAREA == expected_from_grid_area
    assert actual.TYPEOFMP == expected_meteringpoint_type
    assert actual.SETTLEMENTMETHOD == expected_settlement_method
    assert actual.ENERGYSUPPLIERID == expected_energy_supplier_id


def test__both_hour_and_quarterly_resolution_data_are_in_basis_data(
    metering_point_period_df_factory,
) -> None:
    # Arrange
    expected_number_of_metering_points = 2
    metering_point_period_df = metering_point_period_df_factory(
        metering_point_id="1", resolution=MeteringPointResolution.QUARTER.value
    ).union(
        metering_point_period_df_factory(
            metering_point_id="2", resolution=MeteringPointResolution.HOUR.value
        )
    )

    # Act
    master_basis_data = get_master_basis_data_df(metering_point_period_df)

    # Assert
    assert master_basis_data.count() == expected_number_of_metering_points
