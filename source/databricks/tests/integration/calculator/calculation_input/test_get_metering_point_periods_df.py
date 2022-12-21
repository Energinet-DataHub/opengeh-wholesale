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
from package.calculation_input import get_metering_point_periods_df
from package.codelists import (
    MeteringPointType,
    SettlementMethod,
    MeteringPointResolution,
)
from package.constants import Colname
from pyspark.sql.functions import col
from pyspark.sql import DataFrame
from pyspark.sql.types import StructType, StructField, StringType, TimestampType
from typing import Callable

# Factory defaults
grid_area_code = "805"
grid_area_link_id = "the-grid-area-link-id"
metering_point_id = "the-metering-point-id"
energy_supplier_id = "the-energy-supplier-id"
metering_point_type = MeteringPointType.production.value
settlement_method = SettlementMethod.flex.value
resolution = MeteringPointResolution.hour.value
date_time_formatting_string = "%Y-%m-%dT%H:%M"
# +02:00 dates (e.g. danish DST)
june_1th = datetime.strptime("2022-05-31T22:00", date_time_formatting_string)
june_2th = june_1th + timedelta(days=1)
june_3th = june_1th + timedelta(days=2)
june_4th = june_1th + timedelta(days=3)
june_5th = june_1th + timedelta(days=4)
june_6th = june_1th + timedelta(days=5)
june_7th = june_1th + timedelta(days=6)
june_8th = june_1th + timedelta(days=7)
june_9th = june_1th + timedelta(days=8)
june_10th = june_1th + timedelta(days=9)
balance_responsible_id = "someBalanceResponsibleId"


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
        Resolution=resolution,
        FromGridArea="some-in-gride-area",
        ToGridArea="some-out-gride-area",
        ParentMeteringPointId="some-parent-metering-point-id",
        FromDate=june_1th,
        ToDate=june_3th,
        periods=None,
        EnergySupplierId=energy_supplier_id,
        BalanceResponsibleId=balance_responsible_id,
    ) -> DataFrame:
        df_array = []
        if periods:
            for period in periods:
                df_array.append(
                    {
                        Colname.metering_point_id: period[Colname.metering_point_id]
                        if (Colname.metering_point_id in period)
                        else MeteringPointId,
                        Colname.metering_point_type: period[Colname.metering_point_type]
                        if (Colname.metering_point_type in period)
                        else MeteringPointType,
                        Colname.settlement_method: period[Colname.settlement_method]
                        if (Colname.settlement_method in period)
                        else SettlementMethod,
                        Colname.grid_area: period[Colname.grid_area]
                        if (Colname.grid_area in period)
                        else GridAreaCode,
                        Colname.resolution: period[Colname.resolution]
                        if (Colname.resolution in period)
                        else Resolution,
                        Colname.in_grid_area: period[Colname.in_grid_area]
                        if (Colname.in_grid_area in period)
                        else FromGridArea,
                        Colname.out_grid_area: period[Colname.out_grid_area]
                        if (Colname.out_grid_area in period)
                        else ToGridArea,
                        Colname.parent_metering_point_id: period[
                            Colname.parent_metering_point_id
                        ]
                        if (Colname.parent_metering_point_id in period)
                        else ParentMeteringPointId,
                        Colname.from_date: period[Colname.from_date]
                        if (Colname.from_date in period)
                        else FromDate,
                        Colname.to_date: period[Colname.to_date]
                        if (Colname.to_date in period)
                        else ToDate,
                    }
                )
        else:
            df_array.append(
                {
                    Colname.metering_point_id: MeteringPointId,
                    Colname.metering_point_type: MeteringPointType,
                    Colname.settlement_method: SettlementMethod,
                    Colname.grid_area: GridAreaCode,
                    Colname.resolution: Resolution,
                    Colname.in_grid_area: FromGridArea,
                    Colname.out_grid_area: ToGridArea,
                    Colname.parent_metering_point_id: ParentMeteringPointId,
                    Colname.from_date: FromDate,
                    Colname.to_date: ToDate,
                    Colname.energy_supplier_id: EnergySupplierId,
                }
            )

        schema = StructType(
            [
                StructField(Colname.metering_point_id, StringType(), False),
                StructField(Colname.metering_point_type, StringType(), False),
                StructField(Colname.settlement_method, StringType(), False),
                StructField(Colname.grid_area, StringType(), False),
                StructField(Colname.resolution, StringType(), False),
                StructField(Colname.in_grid_area, StringType(), False),
                StructField(Colname.out_grid_area, StringType(), False),
                StructField(Colname.parent_metering_point_id, StringType(), False),
                StructField(Colname.from_date, TimestampType(), False),
                StructField(Colname.to_date, TimestampType(), True),
                StructField(Colname.energy_supplier_id, StringType(), False),
            ]
        )

        return spark.createDataFrame(df_array, schema=schema)

    return factory


def test__when_metering_point_period_is_in_grid_areas__returns_metering_point_period(
    batch_grid_areas_df: DataFrame,
    metering_points_periods_df_factory: Callable[..., DataFrame],
):
    metering_points_periods_df = metering_points_periods_df_factory()

    raw_master_basis_data = get_metering_point_periods_df(
        metering_points_periods_df,
        batch_grid_areas_df,
        june_1th,
        june_2th,
    )
    assert raw_master_basis_data.count() == 1


def test__when_type_is_production__returns_metering_point_period(
    batch_grid_areas_df,
    metering_points_periods_df_factory,
):
    metering_points_periods_df = metering_points_periods_df_factory(
        MeteringPointType=MeteringPointType.production.value
    )

    raw_master_basis_data = get_metering_point_periods_df(
        metering_points_periods_df,
        batch_grid_areas_df,
        june_1th,
        june_2th,
    )
    assert raw_master_basis_data.count() == 1


def test__when_type_is_not_E18__does_not_returns_metering_point_period(
        batch_grid_areas_df,
        metering_points_periods_df_factory,
):
    metering_points_periods_df = metering_points_periods_df_factory(
        MeteringPointType=MeteringPointType.consumption.value
    )
    raw_master_basis_data = get_metering_point_periods_df(
        metering_points_periods_df,
        batch_grid_areas_df,
        june_1th,
        june_2th,
    )
    assert raw_master_basis_data.count() == 0


def test__metering_points_have_expected_columns(
    batch_grid_areas_df: DataFrame,
    metering_points_periods_df_factory: Callable[..., DataFrame],
):
    metering_points_periods_df = metering_points_periods_df_factory()

    raw_master_basis_data = get_metering_point_periods_df(
        metering_points_periods_df,
        batch_grid_areas_df,
        june_1th,
        june_2th,
    )

    assert (
        raw_master_basis_data.where(
            (col(Colname.metering_point_id) == metering_point_id)
            & (col(Colname.grid_area) == grid_area_code)
            & (col(Colname.from_date) == june_1th)
            & (col(Colname.to_date) == june_2th)
            & (col(Colname.metering_point_type) == metering_point_type)
            & (col(Colname.settlement_method) == settlement_method)
            & (col(Colname.in_grid_area) == "some-in-gride-area")
            & (col(Colname.out_grid_area) == "some-out-gride-area")
            & (col(Colname.resolution) == MeteringPointResolution.hour.value)
            & (col(Colname.energy_supplier_id) == energy_supplier_id)
        ).count()
        == 1
    )


def test__when_period_to_date_is_null__returns_metering_point_period_with_to_date_equal_to_period_end(
    batch_grid_areas_df,
    metering_points_periods_df_factory,
):
    # Arrange
    metering_points_periods_df = metering_points_periods_df_factory(ToDate=None)
    period_end = june_2th

    raw_master_basis_data = get_metering_point_periods_df(
        metering_points_periods_df,
        batch_grid_areas_df,
        june_1th,
        period_end_datetime=period_end,
    )

    assert raw_master_basis_data.count() == 1
    assert raw_master_basis_data.where(col(Colname.to_date) == period_end).count() == 1
