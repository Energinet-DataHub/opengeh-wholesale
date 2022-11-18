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
import pytest
from package.balance_fixing_total_production import (
    _get_raw_basis_data_df,
)
from pyspark.sql.functions import col
import os


# Factory defaults
grid_area_code = "805"
grid_area_link_id = "the-grid-area-link-id"


@pytest.fixture
def grid_area_df(spark):
    row = {
        "GridAreaCode": grid_area_code,
        "GridAreaLinkId": grid_area_link_id,
    }
    return spark.createDataFrame([row])


# def test_metering_points_have_energy_supplier(
#     databricks_path,
#     spark,
#     grid_area_df,
# ):
#     metering_points_periods_df = spark.read.option("header", "true").csv(
#         f"{databricks_path}/package/datasources/MeteringPointsPeriods.csv"
#     )
#     market_roles_periods_df = spark.read.option("header", "true").csv(
#         f"{databricks_path}/package/datasources/MarketRolesPeriods.csv"
#     )
#     period_start_datetime = datetime.strptime("01/01/2018 00:00", "%d/%m/%Y %H:%M")
#     period_end_datetime = datetime.strptime("01/03/2018 22:00", "%d/%m/%Y %H:%M")

#     actual = _get_raw_basis_data_df(
#         metering_points_periods_df,
#         market_roles_periods_df,
#         grid_area_df,
#         period_start_datetime,
#         period_end_datetime,
#     )

#     actual_mps_without_energy_supplier = actual.filter(
#         col("EnergySupplierGln").isNull()
#     )

#     assert actual_mps_without_energy_supplier.count() == 0


def test__when_metering_point_period_is_in_grid_areas__returns_metering_point_period():
    pass


# What about market participant periods outside the selected period?
def test__when_energy_supplier_changes_in_batch_period__returns_two_periods_with_expected_energy_supplier_and_dates():
    pass


# Both periodized energy supplier and metering point
# Test combinations with 0, 1 and 2 periods
def test__returns_expected_periods():
    pass


def test__when_connection_state_is_E22__returns_metering_point_period():
    pass


def test__when_connection_state_is_not_E22__returns_none():
    pass


def test__when_type_is_E18__returns_metering_point_period():
    pass


def test__when_type_is_not_E18__returns_metering_point_period():
    pass


def test__metering_points_have_expected_columns():  # expected_column_name, expected_value):
    pass


def test__when_in_period__returns_metering_point_period():  # mp_start, mp_end):
    # <= and >= combinations
    # period_start = xx
    # period_end = xx
    pass


# def test_meteringpoints_start_date_is_included():
#     assert 1 == 1


# def test_meteringpoints_end_date_is_included():
#     assert 1 == 1


# def test__when_market_participant_has_periods__metering_point_period_is_split():
#     pass


# def test__when_in_period__metering_point_is_returned():
#     pass
