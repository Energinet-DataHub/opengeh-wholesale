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
from zoneinfo import ZoneInfo
import pytest
from package.calculation_input import get_metering_point_periods_df
from package.codelists import (
    MeteringPointType,
    SettlementMethod,
    MeteringPointResolution,
)
from package.codelists import ConnectionState
from pyspark.sql.functions import col
from pyspark.sql import DataFrame
from typing import Callable

# Factory defaults
grid_area_code = "805"
grid_area_link_id = "the-grid-area-link-id"
metering_point_id = "the-metering-point-id"
energy_supplier_id = "the-energy-supplier-id"
metering_point_type = MeteringPointType.production.value
settlement_method = SettlementMethod.flex.value
connection_state = ConnectionState.connected.value
resolution = MeteringPointResolution.hour.value
date_time_formatting_string = "%Y-%m-%dT%H:%M:%S.%f"
june_1th = datetime.strptime(
    "2022-06-01T00:00:00.000", date_time_formatting_string
).replace(tzinfo=ZoneInfo("Europe/Copenhagen"))
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
def batch_grid_areas_df(spark) -> DataFrame:
    row = {"GridAreaCode": grid_area_code}
    return spark.createDataFrame([row])


@pytest.fixture(scope="module")
def metering_points_periods_df_factory(spark) -> Callable[..., DataFrame]:
    def factory(
        MeteringPointId=metering_point_id,
        MeteringPointType=metering_point_type,
        SettlementMethod=settlement_method,
        GridAreaCode=grid_area_code,
        ConnectionState=connection_state,
        Resolution=resolution,
        FromGridArea="some-in-gride-area",
        ToGridArea="some-out-gride-area",
        ParentMeteringPointId="some-parent-metering-point-id",
        FromDate=june_1th,
        ToDate=june_3th,
        periods=None,
        EnergySupplierId=energy_supplier_id,
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
                        "GridAreaCode": period["GridAreaCode"]
                        if ("GridAreaCode" in period)
                        else GridAreaCode,
                        "ConnectionState": period["ConnectionState"]
                        if ("ConnectionState" in period)
                        else ConnectionState,
                        "Resolution": period["Resolution"]
                        if ("Resolution" in period)
                        else Resolution,
                        "FromGridAreaCode": period["FromGridAreaCode"]
                        if ("FromGridAreaCode" in period)
                        else FromGridArea,
                        "ToGridAreaCode": period["ToGridAreaCode"]
                        if ("ToGridAreaCode" in period)
                        else ToGridArea,
                        "ParentMeteringPointId": period["ParentMeteringPointId"]
                        if ("ParentMeteringPointId" in period)
                        else ParentMeteringPointId,
                        "FromDate": period["FromDate"]
                        if ("FromDate" in period)
                        else FromDate,
                        "ToDate": period["ToDate"] if ("ToDate" in period) else ToDate,
                    }
                )
        else:
            df_array.append(
                {
                    "MeteringPointId": MeteringPointId,
                    "Type": MeteringPointType,
                    "SettlementMethod": SettlementMethod,
                    "GridAreaCode": GridAreaCode,
                    "ConnectionState": ConnectionState,
                    "Resolution": Resolution,
                    "FromGridAreaCode": FromGridArea,
                    "ToGridAreaCode": ToGridArea,
                    "ParentMeteringPointId": ParentMeteringPointId,
                    "FromDate": FromDate,
                    "ToDate": ToDate,
                    "EnergySupplierId": EnergySupplierId,
                }
            )
        return spark.createDataFrame(df_array)

    return factory


# def test__when_metering_point_period_is_in_grid_areas__returns_metering_point_period(
#     batch_grid_areas_df: DataFrame,
#     metering_points_periods_df_factory: Callable[..., DataFrame],
# ):
#     metering_points_periods_df = metering_points_periods_df_factory()

#     raw_master_basis_data = get_metering_point_periods_df(
#         metering_points_periods_df,
#         batch_grid_areas_df,
#         june_1th,
#         june_2th,
#     )
#     assert raw_master_basis_data.count() == 1


# def test__when_type_is_production__returns_metering_point_period(
#     batch_grid_areas_df,
#     metering_points_periods_df_factory,
# ):
#     metering_points_periods_df = metering_points_periods_df_factory(
#         MeteringPointType=MeteringPointType.production.value
#     )

#     raw_master_basis_data = get_metering_point_periods_df(
#         metering_points_periods_df,
#         batch_grid_areas_df,
#         june_1th,
#         june_2th,
#     )
#     assert raw_master_basis_data.count() == 1


# def test__when_type_is_not_E18__does_not_returns_metering_point_period(
#     batch_grid_areas_df,
#     metering_points_periods_df_factory,
# ):
#     metering_points_periods_df = metering_points_periods_df_factory(
#         MeteringPointType=MeteringPointType.consumption.value
#     )

#     raw_master_basis_data = get_metering_point_periods_df(
#         metering_points_periods_df,
#         batch_grid_areas_df,
#         june_1th,
#         june_2th,
#     )
#     assert raw_master_basis_data.count() == 0


# def test__metering_points_have_expected_columns(
#     batch_grid_areas_df: DataFrame,
#     metering_points_periods_df_factory: Callable[..., DataFrame],
# ):
#     metering_points_periods_df = metering_points_periods_df_factory()

#     raw_master_basis_data = get_metering_point_periods_df(
#         metering_points_periods_df,
#         batch_grid_areas_df,
#         june_1th,
#         june_2th,
#     )

#     assert (
#         raw_master_basis_data.where(
#             (col("MeteringPointId") == metering_point_id)
#             & (col("GridAreaCode") == grid_area_code)
#             & (col("EffectiveDate") == june_1th)
#             & (col("toEffectiveDate") == june_2th)
#             & (col("Type") == metering_point_type)
#             & (col("SettlementMethod") == settlement_method)
#             & (col("FromGridAreaCode") == "some-in-gride-area")
#             & (col("ToGridAreaCode") == "some-out-gride-area")
#             & (col("Resolution") == MeteringPointResolution.hour.value)
#             & (col("EnergySupplierId") == energy_supplier_id)
#         ).count()
#         == 1
#     )
