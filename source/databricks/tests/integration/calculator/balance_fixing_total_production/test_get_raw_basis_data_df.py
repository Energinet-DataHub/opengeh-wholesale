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
from pyspark.sql import DataFrame
import os
from typing import Callable

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
june_1th = datetime.strptime("31/05/2022 22:00", "%d/%m/%Y %H:%M")
june_2th = june_1th + timedelta(days=1)
june_3th = june_1th + timedelta(days=2)
june_4th = june_1th + timedelta(days=3)
june_5th = june_1th + timedelta(days=4)
june_6th = june_1th + timedelta(days=5)
june_7th = june_1th + timedelta(days=6)
june_8th = june_1th + timedelta(days=7)
june_9th = june_1th + timedelta(days=8)
june_10th = june_1th + timedelta(days=9)


@pytest.fixture
def grid_area_df(spark) -> DataFrame:
    row = {
        "GridAreaCode": grid_area_code,
        "GridAreaLinkId": grid_area_link_id,
    }
    return spark.createDataFrame([row])


@pytest.fixture(scope="module")
def market_roles_period_df_factory(spark):
    def factory(
        EnergySupplierId="1",
        MeteringPointId=metering_point_id,
        FromDate=june_1th,
        ToDate=june_3th,
        GridArea=grid_area_code,
        periods=None,
    ) -> DataFrame:
        df_array = []
        if periods:
            for period in periods:
                df_array.append(
                    {
                        "EnergySupplierId": period["EnergySupplierId"]
                        if ("EnergySupplierId" in period)
                        else EnergySupplierId,
                        "MeteringPointId": period["MeteringPointId"]
                        if ("MeteringPointId" in period)
                        else MeteringPointId,
                        "FromDate": period["FromDate"]
                        if ("FromDate" in period)
                        else FromDate,
                        "ToDate": period["ToDate"] if ("ToDate" in period) else ToDate,
                        "GridArea": period["GridArea"]
                        if ("GridArea" in period)
                        else GridArea,
                    }
                )
        else:
            df_array.append(
                {
                    "EnergySupplierId": EnergySupplierId,
                    "MeteringPointId": MeteringPointId,
                    "FromDate": FromDate,
                    "ToDate": ToDate,
                    "GridArea": GridArea,
                }
            )
        return spark.createDataFrame(df_array)

    return factory


@pytest.fixture(scope="module")
def metering_points_periods_df_factory(spark) -> Callable[[], DataFrame]:
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
        FromDate=june_1th,
        ToDate=june_3th,
        NumberOfTimeseries="1",
        EnergyQuantity="1",
        periods=None,
    ) -> DataFrame:
        df_array = []
        if periods:
            for period in periods:
                df_array.append(
                    {
                        "MeteringPointId": period["MeteringPointId"]
                        if ("MeteringPointId" in period)
                        else MeteringPointId,
                        "MeteringPointType": period["MeteringPointType"]
                        if ("MeteringPointType" in period)
                        else MeteringPointType,
                        "SettlementMethod": period["SettlementMethod"]
                        if ("SettlementMethod" in period)
                        else SettlementMethod,
                        "GridArea": period["GridArea"]
                        if ("GridArea" in period)
                        else GridArea,
                        "ConnectionState": period["ConnectionState"]
                        if ("ConnectionState" in period)
                        else ConnectionState,
                        "Resolution": period["Resolution"]
                        if ("Resolution" in period)
                        else Resolution,
                        "InGridArea": period["InGridArea"]
                        if ("InGridArea" in period)
                        else InGridArea,
                        "OutGridArea": period["OutGridArea"]
                        if ("OutGridArea" in period)
                        else OutGridArea,
                        "MeteringMethod": period["MeteringMethod"]
                        if ("MeteringMethod" in period)
                        else MeteringMethod,
                        "NetSettlementGroup": period["NetSettlementGroup"]
                        if ("NetSettlementGroup" in period)
                        else NetSettlementGroup,
                        "ParentMeteringPointId": period["ParentMeteringPointId"]
                        if ("ParentMeteringPointId" in period)
                        else ParentMeteringPointId,
                        "Unit": period["Unit"] if ("Unit" in period) else Unit,
                        "Product": period["Product"]
                        if ("Product" in period)
                        else Product,
                        "FromDate": period["FromDate"]
                        if ("FromDate" in period)
                        else FromDate,
                        "ToDate": period["ToDate"] if ("ToDate" in period) else ToDate,
                        "NumberOfTimeseries": period["NumberOfTimeseries"]
                        if ("NumberOfTimeseries" in period)
                        else NumberOfTimeseries,
                        "EnergyQuantity": period["EnergyQuantity"]
                        if ("EnergyQuantity" in period)
                        else EnergyQuantity,
                    }
                )
        else:
            df_array.append(
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
            )
        return spark.createDataFrame(df_array)

    return factory


def test__when_metering_point_period_is_in_grid_areas__returns_metering_point_period(
    grid_area_df, market_roles_period_df_factory, metering_points_periods_df_factory
):
    metering_points_periods_df = metering_points_periods_df_factory()
    market_roles_periods_df = market_roles_period_df_factory()

    raw_master_basis_data = _get_master_basis_data_df(
        metering_points_periods_df,
        market_roles_periods_df,
        grid_area_df,
        june_1th,
        june_2th,
    )
    assert raw_master_basis_data.count() == 1


# What about market participant periods outside the selected period?
def test__when_energy_supplier_changes_in_batch_period__returns_two_periods_with_expected_energy_supplier_and_dates(
    grid_area_df, market_roles_period_df_factory, metering_points_periods_df_factory
):
    metering_points_periods_df = metering_points_periods_df_factory()
    metering_points_periods_df
    market_roles_periods_df = market_roles_period_df_factory(
        periods=[
            {"FromDate": june_1th, "ToDate": june_2th},
            {"FromDate": june_2th, "ToDate": june_3th, "EnergySupplierId": "2"},
        ]
    )
    raw_master_basis_data = _get_master_basis_data_df(
        metering_points_periods_df,
        market_roles_periods_df,
        grid_area_df,
        june_1th,
        june_3th,
    )
    period_with_energy_suplier_1 = raw_master_basis_data.where(
        (col("EnergySupplierGln") == 1)
        & (col("EffectiveDate") == june_1th)
        & (col("toEffectiveDate") == june_2th)
    )

    period_with_energy_suplier_2 = raw_master_basis_data.where(
        (col("EnergySupplierGln") == 2)
        & (col("EffectiveDate") == june_2th)
        & (col("toEffectiveDate") == june_3th)
    )

    assert period_with_energy_suplier_1.count() == 1
    assert period_with_energy_suplier_2.count() == 1


# Both periodized energy supplier and metering point
# Test combinations with 0, 1 and 2 periods
@pytest.mark.parametrize(
    "batch_period, market_roles_periods, metering_points_periods, expected_periods",
    [
        # 2 meteringpoint periods and 3 overlaping market roles periods returns 4 periods with correct dates and Energy suplier
        (
            {"start": june_2th, "end": june_7th},
            [
                {"FromDate": june_1th, "ToDate": june_3th, "EnergySupplierId": "1"},
                {"FromDate": june_3th, "ToDate": june_6th, "EnergySupplierId": "2"},
                {"FromDate": june_6th, "ToDate": june_10th, "EnergySupplierId": "3"},
            ],
            [
                {"FromDate": june_2th, "ToDate": june_5th},
                {"FromDate": june_5th, "ToDate": june_7th},
            ],
            [
                {"EDate": june_2th, "toEDate": june_3th, "ESGln": "1", "SetM": "D01"},
                {"EDate": june_3th, "toEDate": june_5th, "ESGln": "2", "SetM": "D01"},
                {"EDate": june_5th, "toEDate": june_6th, "ESGln": "2", "SetM": "D01"},
                {"EDate": june_6th, "toEDate": june_7th, "ESGln": "3", "SetM": "D01"},
            ],
        ),
        # when there is no energy suplier within batch period there is no period
        (
            {"start": june_2th, "end": june_7th},
            [
                {"FromDate": june_1th, "ToDate": june_3th, "EnergySupplierId": "1"},
                {"FromDate": june_3th, "ToDate": june_6th, "EnergySupplierId": "2"},
                {"FromDate": june_6th, "ToDate": june_10th, "EnergySupplierId": "3"},
            ],
            [
                {"FromDate": june_8th, "ToDate": june_10th},
            ],
            [],
        ),
        # when metering point periods is starting and ending outside batch period
        # EffectiveDate and toEffectiveDate is adjust to match batch period dates
        (
            {"start": june_3th, "end": june_7th},
            [{"FromDate": june_1th, "ToDate": june_9th, "EnergySupplierId": "1"}],
            [{"FromDate": june_2th, "ToDate": june_10th}],
            [{"EDate": june_3th, "toEDate": june_7th, "ESGln": "1", "SetM": "D01"}],
        ),
        # when meteringpoint and market roles change on same day returns corect periods
        (
            {"start": june_1th, "end": june_10th},
            [
                {"FromDate": june_3th, "ToDate": june_5th, "EnergySupplierId": "1"},
                {"FromDate": june_5th, "ToDate": june_6th, "EnergySupplierId": "2"},
                {"FromDate": june_6th, "ToDate": june_10th, "EnergySupplierId": "3"},
            ],
            [
                {"FromDate": june_5th, "ToDate": june_6th, "SettlementMethod": "D01"},
                {"FromDate": june_6th, "ToDate": june_7th, "SettlementMethod": "D02"},
            ],
            [
                {"EDate": june_5th, "toEDate": june_6th, "ESGln": "2", "SetM": "D01"},
                {"EDate": june_6th, "toEDate": june_7th, "ESGln": "3", "SetM": "D02"},
            ],
        ),
        # when_connection_state_is_not_E22__returns_no period
        (
            {"start": june_1th, "end": june_10th},
            [
                {"FromDate": june_1th, "ToDate": june_3th, "EnergySupplierId": "1"},
                {"FromDate": june_3th, "ToDate": june_6th, "EnergySupplierId": "2"},
                {"FromDate": june_6th, "ToDate": june_10th, "EnergySupplierId": "3"},
            ],
            [
                # not_used
                {"FromDate": june_2th, "ToDate": june_3th, "ConnectionState": "D01"},
                # closed_down
                {"FromDate": june_3th, "ToDate": june_4th, "ConnectionState": "D02"},
                # new
                {"FromDate": june_4th, "ToDate": june_5th, "ConnectionState": "D03"},
                # connected
                {"FromDate": june_5th, "ToDate": june_6th, "ConnectionState": "E22"},
                # disconnected
                {"FromDate": june_6th, "ToDate": june_7th, "ConnectionState": "E23"},
            ],
            [
                {"EDate": june_5th, "toEDate": june_6th, "ESGln": "2", "SetM": "D01"},
            ],
        ),
    ],
)
def test__returns_expected_periods(
    batch_period,
    market_roles_periods,
    metering_points_periods,
    expected_periods,
    grid_area_df,
    market_roles_period_df_factory,
    metering_points_periods_df_factory,
):

    metering_points_periods_df = metering_points_periods_df_factory(
        periods=metering_points_periods
    )
    market_roles_periods_df = market_roles_period_df_factory(
        periods=market_roles_periods
    )

    raw_master_basis_data = _get_master_basis_data_df(
        metering_points_periods_df,
        market_roles_periods_df,
        grid_area_df,
        batch_period["start"],
        batch_period["end"],
    )
    raw_master_basis_data.show()
    assert raw_master_basis_data.count() == len(expected_periods)
    for expected_period in expected_periods:
        period = raw_master_basis_data.where(
            (col("EnergySupplierGln") == expected_period["ESGln"])
            & (col("EffectiveDate") == expected_period["EDate"])
            & (col("toEffectiveDate") == expected_period["toEDate"])
            & (col("SettlementMethod") == expected_period["SetM"])
        )

        assert period.count() == 1


# these test is done in test__returns_expected_periods. So the can they be removed?
# def test__when_connection_state_is_E22__returns_metering_point_period():
#     pass
#
#
# def test__when_connection_state_is_not_E22__returns_none():
#     pass
#
# def test__when_in_period__returns_metering_point_period():
#     # mp_start, mp_end):
#     # <= and >= combinations
#     # period_start = xx
#     # period_end = xx
#     pass


def test__when_type_is_E18__returns_metering_point_period(
    grid_area_df, market_roles_period_df_factory, metering_points_periods_df_factory
):
    metering_points_periods_df = metering_points_periods_df_factory(
        MeteringPointType="E18"
    )
    market_roles_periods_df = market_roles_period_df_factory()

    raw_master_basis_data = _get_master_basis_data_df(
        metering_points_periods_df,
        market_roles_periods_df,
        grid_area_df,
        june_1th,
        june_2th,
    )
    assert raw_master_basis_data.count() == 1


def test__when_type_is_not_E18__does_not_returns_metering_point_period(
    grid_area_df, market_roles_period_df_factory, metering_points_periods_df_factory
):
    metering_points_periods_df = metering_points_periods_df_factory(
        MeteringPointType="E20"
    )
    market_roles_periods_df = market_roles_period_df_factory()

    raw_master_basis_data = _get_master_basis_data_df(
        metering_points_periods_df,
        market_roles_periods_df,
        grid_area_df,
        june_1th,
        june_2th,
    )
    assert raw_master_basis_data.count() == 0


def test__metering_points_have_expected_columns(  # expected_column_name, expected_value):
    grid_area_df: DataFrame,
    market_roles_period_df_factory: Callable[[], DataFrame],
    metering_points_periods_df_factory: Callable[[], DataFrame],
):
    metering_points_periods_df = metering_points_periods_df_factory()
    market_roles_periods_df = market_roles_period_df_factory()

    raw_master_basis_data = _get_master_basis_data_df(
        metering_points_periods_df,
        market_roles_periods_df,
        grid_area_df,
        june_1th,
        june_2th,
    )

    print(raw_master_basis_data.columns)
    assert raw_master_basis_data.columns == [
        "GsrnNumber",
        "GridAreaCode",
        "EffectiveDate",
        "toEffectiveDate",
        "MeteringPointType",
        "SettlementMethod",
        "FromGridAreaCode",
        "ToGridAreaCode",
        "Resolution",
        "EnergySupplierGln",
    ]
