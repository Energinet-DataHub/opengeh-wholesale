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

from datetime import datetime, timedelta
import pytest
from package.balance_fixing_total_production import (
    _get_master_basis_data_df,
)
from pyspark.sql.functions import col
import os


# Factory defaults
grid_area_code = "805"
grid_area_link_id = "the-grid-area-link-id"
gsrn_number = "the-gsrn-number"
metering_point_id = "the-metering-point-id"
energy_supplier_id = "the-energy-supplier-id"
metering_point_type = "E18"
settlement_method = "D01"
connection_state = "E22"
resolution = "PT1H"
metering_method = "D01"
first_of_june = datetime.strptime("31/05/2022 22:00", "%d/%m/%Y %H:%M")
second_of_june = first_of_june + timedelta(days=1)
third_of_june = first_of_june + timedelta(days=2)


@pytest.fixture
def grid_area_df(spark):
    row = {
        "GridAreaCode": grid_area_code,
        "GridAreaLinkId": grid_area_link_id,
    }
    return spark.createDataFrame([row])


@pytest.fixture(scope="module")
def market_roles_period_df_factory(spark, timestamp_factory):
    def factory(
        EnergySupplierId="1",
        MeteringPointId=metering_point_id,
        FromDate=first_of_june,
        ToDate=third_of_june,
        GridArea=grid_area_code,
    ):
        df = [
            {
                "EnergySupplierId": EnergySupplierId,
                "MeteringPointId": MeteringPointId,
                "FromDate": FromDate,
                "ToDate": ToDate,
                "GridArea": GridArea,
            }
        ]
        return spark.createDataFrame(df)

    return factory


@pytest.fixture(scope="module")
def metering_points_periods_df_factory(spark, timestamp_factory):
    def factory(
        MeteringPointId=metering_point_id,
        MeteringPointType=metering_point_type,
        SettlementMethod=settlement_method,
        GridArea=grid_area_code,
        ConnectionState=connection_state,
        Resolution=resolution,
        InGridArea="some-in-gride-area",
        OutGridArea="some-out-gride-area",
        MeteringMethod=metering_method,
        NetSettlementGroup=0,
        ParentMeteringPointId="some-parent-metering-point-id",
        Unit="KWH",
        Product=1,
        FromDate=first_of_june,
        ToDate=third_of_june,
        NumberOfTimeseries="1",
        EnergyQuantity="1",
    ):

        df = [
            {
                "MeteringPointId": MeteringPointId,
                "MeteringPointType": MeteringPointType,
                "SettlementMethod": SettlementMethod,
                "GridArea": GridArea,
                "ConnectionState": ConnectionState,
                "Resolution": Resolution,
                "InGridArea": InGridArea,
                "OutGridArea": OutGridArea,
                "MeteringMethod": MeteringMethod,
                "NetSettlementGroup": NetSettlementGroup,
                "ParentMeteringPointId": ParentMeteringPointId,
                "Unit": Unit,
                "Product": Product,
                "FromDate": FromDate,
                "ToDate": ToDate,
                "NumberOfTimeseries": NumberOfTimeseries,
                "EnergyQuantity": EnergyQuantity,
            }
        ]
        return spark.createDataFrame(df)

    return factory


# def test_metering_points_have_energy_supplier(
#     databricks_path,
#     spark,
#     grid_area_df,
# ):
#     metering_points_periods_df = spark.read.option("header", "true").csv(
#         f"{databricks_path}/tests/integration/test_files/MeteringPointsPeriods.csv"
#     )
#     market_roles_periods_df = spark.read.option("header", "true").csv(
#         f"{databricks_path}/tests/integration/test_files/MarketRolesPeriods.csv"
#     )
#     period_start_datetime = datetime.strptime("01/01/2018 00:00", "%d/%m/%Y %H:%M")
#     period_end_datetime = datetime.strptime("01/03/2018 22:00", "%d/%m/%Y %H:%M")

#     actual = _get_master_basis_data_df(
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


def test__when_metering_point_period_is_in_grid_areas__returns_metering_point_period(
    grid_area_df, market_roles_period_df_factory, metering_points_periods_df_factory
):
    metering_points_periods_df = metering_points_periods_df_factory()
    market_roles_periods_df = market_roles_period_df_factory()

    master_basis_data = _get_master_basis_data_df(
        metering_points_periods_df,
        market_roles_periods_df,
        grid_area_df,
        first_of_june,
        second_of_june,
    )
    assert master_basis_data.count() == 1


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
