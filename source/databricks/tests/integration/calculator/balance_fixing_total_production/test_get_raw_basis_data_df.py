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
from package.codelists import (
    NewConnectionState,
    NewMeteringPointType,
    NewSettlementMethod,
    NewMeteringPointResolution,
)

from pyspark.sql.functions import col
from pyspark.sql import DataFrame
import os
from typing import Callable

# Factory defaults
grid_area_code = "805"
grid_area_link_id = "the-grid-area-link-id"
metering_point_id = "the-metering-point-id"
energy_supplier_id = "the-energy-supplier-id"
metering_point_type = NewMeteringPointType.production.value
settlement_method = NewSettlementMethod.flex.value
connection_state = NewConnectionState.connected.value
resolution = NewMeteringPointResolution.hour.value
date_time_formatting_string = "%Y-%m-%dT%H:%M:%S.%fZ"
june_1th = datetime.strptime("2022-05-31T22:00:00.000Z", date_time_formatting_string)
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
        EnergySupplierId=energy_supplier_id,
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
def metering_points_periods_df_factory(spark) -> Callable[..., DataFrame]:
    def factory(
        MeteringPointId=metering_point_id,
        MeteringPointType=metering_point_type,
        SettlementMethod=settlement_method,
        GridArea=grid_area_code,
        ConnectionState=connection_state,
        Resolution=resolution,
        FromGridArea="some-in-gride-area",
        ToGridArea="some-out-gride-area",
        NetSettlementGroup=0,
        ParentMeteringPointId="some-parent-metering-point-id",
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
                        "FromGridArea": period["FromGridArea"]
                        if ("FromGridArea" in period)
                        else FromGridArea,
                        "ToGridArea": period["ToGridArea"]
                        if ("ToGridArea" in period)
                        else ToGridArea,
                        "NetSettlementGroup": period["NetSettlementGroup"]
                        if ("NetSettlementGroup" in period)
                        else NetSettlementGroup,
                        "ParentMeteringPointId": period["ParentMeteringPointId"]
                        if ("ParentMeteringPointId" in period)
                        else ParentMeteringPointId,
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
                    "FromGridArea": FromGridArea,
                    "ToGridArea": ToGridArea,
                    "NetSettlementGroup": NetSettlementGroup,
                    "ParentMeteringPointId": ParentMeteringPointId,
                    "FromDate": FromDate,
                    "ToDate": ToDate,
                    "NumberOfTimeseries": NumberOfTimeseries,
                    "EnergyQuantity": EnergyQuantity,
                }
            )
        return spark.createDataFrame(df_array)

    return factory


def test__when_metering_point_period_is_in_grid_areas__returns_metering_point_period(
    grid_area_df: DataFrame,
    market_roles_period_df_factory: Callable[..., DataFrame],
    metering_points_periods_df_factory: Callable[..., DataFrame],
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
    grid_area_df: DataFrame,
    market_roles_period_df_factory: Callable[[], DataFrame],
    metering_points_periods_df_factory: Callable[[], DataFrame],
):
    metering_points_periods_df = metering_points_periods_df_factory()
    market_roles_periods_df = market_roles_period_df_factory(
        periods=[
            {"FromDate": june_1th, "ToDate": june_2th, "EnergySupplierId": "1"},
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
        (col("EnergySupplierId") == 1)
        & (col("EffectiveDate") == june_1th)
        & (col("toEffectiveDate") == june_2th)
    )

    period_with_energy_suplier_2 = raw_master_basis_data.where(
        (col("EnergySupplierId") == 2)
        & (col("EffectiveDate") == june_2th)
        & (col("toEffectiveDate") == june_3th)
    )
    raw_master_basis_data.show()
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
                {
                    "EffectiveDate": june_2th,
                    "toEffectiveDate": june_3th,
                    "EnergySupplierId": "1",
                    "SettlementMethod": NewSettlementMethod.flex.value,
                },
                {
                    "EffectiveDate": june_3th,
                    "toEffectiveDate": june_5th,
                    "EnergySupplierId": "2",
                    "SettlementMethod": NewSettlementMethod.flex.value,
                },
                {
                    "EffectiveDate": june_5th,
                    "toEffectiveDate": june_6th,
                    "EnergySupplierId": "2",
                    "SettlementMethod": NewSettlementMethod.flex.value,
                },
                {
                    "EffectiveDate": june_6th,
                    "toEffectiveDate": june_7th,
                    "EnergySupplierId": "3",
                    "SettlementMethod": NewSettlementMethod.flex.value,
                },
            ],
        ),
        # When a market role period ends same day as batch period starts does return correct periods
        (
            {"start": june_2th, "end": june_7th},
            [
                {"FromDate": june_1th, "ToDate": june_2th, "EnergySupplierId": "1"},
                {"FromDate": june_2th, "ToDate": june_7th, "EnergySupplierId": "2"},
            ],
            [{"FromDate": june_1th, "ToDate": june_7th}],
            [
                {
                    "EffectiveDate": june_2th,
                    "toEffectiveDate": june_7th,
                    "EnergySupplierId": "2",
                    "SettlementMethod": NewSettlementMethod.flex.value,
                }
            ],
        ),
        # When a mettering point period ends same day as batch period starts does return correct periods
        (
            {"start": june_2th, "end": june_7th},
            [{"FromDate": june_1th, "ToDate": june_7th, "EnergySupplierId": "1"}],
            [
                {"FromDate": june_1th, "ToDate": june_2th},
                {"FromDate": june_2th, "ToDate": june_7th},
            ],
            [
                {
                    "EffectiveDate": june_2th,
                    "toEffectiveDate": june_7th,
                    "EnergySupplierId": "1",
                    "SettlementMethod": NewSettlementMethod.flex.value,
                }
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
            [
                {
                    "EffectiveDate": june_3th,
                    "toEffectiveDate": june_7th,
                    "EnergySupplierId": "1",
                    "SettlementMethod": NewSettlementMethod.flex.value,
                }
            ],
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
                {
                    "FromDate": june_5th,
                    "ToDate": june_6th,
                    "SettlementMethod": NewSettlementMethod.flex.value,
                },
                {
                    "FromDate": june_6th,
                    "ToDate": june_7th,
                    "SettlementMethod": NewSettlementMethod.nonprofiled.value,
                },
            ],
            [
                {
                    "EffectiveDate": june_5th,
                    "toEffectiveDate": june_6th,
                    "EnergySupplierId": "2",
                    "SettlementMethod": NewSettlementMethod.flex.value,
                },
                {
                    "EffectiveDate": june_6th,
                    "toEffectiveDate": june_7th,
                    "EnergySupplierId": "3",
                    "SettlementMethod": NewSettlementMethod.nonprofiled.value,
                },
            ],
        ),
        # Only_return_periods_when_connection_state_is_either_connected_or_disconected
        (
            {"start": june_1th, "end": june_10th},
            [
                {"FromDate": june_1th, "ToDate": june_3th, "EnergySupplierId": "1"},
                {"FromDate": june_3th, "ToDate": june_6th, "EnergySupplierId": "2"},
                {"FromDate": june_6th, "ToDate": june_10th, "EnergySupplierId": "3"},
            ],
            [
                # not_used
                {
                    "FromDate": june_2th,
                    "ToDate": june_3th,
                    "ConnectionState": NewConnectionState.not_used.value,
                },
                # closed_down
                {
                    "FromDate": june_3th,
                    "ToDate": june_4th,
                    "ConnectionState": NewConnectionState.closed_down.value,
                },
                # new
                {
                    "FromDate": june_4th,
                    "ToDate": june_5th,
                    "ConnectionState": NewConnectionState.new.value,
                },
                # connected
                {
                    "FromDate": june_5th,
                    "ToDate": june_6th,
                    "ConnectionState": NewConnectionState.connected.value,
                },
                # disconnected TODO: LRN den vi have inkluderet.
                {
                    "FromDate": june_6th,
                    "ToDate": june_7th,
                    "ConnectionState": NewConnectionState.disconnected.value,
                },
            ],
            [
                {
                    "EffectiveDate": june_5th,
                    "toEffectiveDate": june_6th,
                    "EnergySupplierId": "2",
                    "SettlementMethod": NewSettlementMethod.flex.value,
                },
                {
                    "EffectiveDate": june_6th,
                    "toEffectiveDate": june_7th,
                    "EnergySupplierId": "3",
                    "SettlementMethod": NewSettlementMethod.flex.value,
                },
            ],
        ),
        # when_periods_connection_state_is_closed_down_period_is_not_includded
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
            (col("EnergySupplierId") == expected_period["EnergySupplierId"])
            & (col("EffectiveDate") == expected_period["EffectiveDate"])
            & (col("toEffectiveDate") == expected_period["toEffectiveDate"])
            & (col("SettlementMethod") == expected_period["SettlementMethod"])
        )
        assert period.count() == 1


def test__when_type_is_production__returns_metering_point_period(
    grid_area_df, market_roles_period_df_factory, metering_points_periods_df_factory
):
    metering_points_periods_df = metering_points_periods_df_factory(
        MeteringPointType=NewMeteringPointType.production.value
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
        MeteringPointType=NewMeteringPointType.consumption.value
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

    assert (
        raw_master_basis_data.where(
            (col("MeteringPointId") == metering_point_id)
            & (col("GridAreaCode") == grid_area_code)
            & (col("EffectiveDate") == june_1th)
            & (col("toEffectiveDate") == june_2th)
            & (col("MeteringPointType") == metering_point_type)
            & (col("SettlementMethod") == settlement_method)
            & (col("FromGridAreaCode") == "some-in-gride-area")
            & (col("ToGridAreaCode") == "some-out-gride-area")
            & (col("Resolution") == NewMeteringPointResolution.hour.value)
            & (col("EnergySupplierId") == energy_supplier_id)
        ).count()
        == 1
    )
