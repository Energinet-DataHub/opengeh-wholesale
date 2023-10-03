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
from pyspark.sql.functions import col
from pyspark.sql import DataFrame
from pyspark.sql.types import StructType, StructField, StringType, TimestampType
import pytest
from typing import Callable
from unittest.mock import patch, Mock

from package.calculation_input.delta_table_reader import DeltaTableReader
from package.calculation_input.metering_point_periods import (
    get_metering_point_periods_df,
)
from package.codelists import (
    MeteringPointType,
    SettlementMethod,
    MeteringPointResolution,
)
from package.constants import Colname

from tests.helpers.type_utils import qualname

# Factory defaults
grid_area_code = "805"
grid_area_link_id = "the-grid-area-link-id"
metering_point_id = "the-metering-point-id"
energy_supplier_id = "the-energy-supplier-id"
metering_point_type = MeteringPointType.PRODUCTION.value
settlement_method = SettlementMethod.FLEX.value
resolution = MeteringPointResolution.HOUR.value
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


@pytest.fixture(scope="module")
def metering_points_periods_df_factory(spark) -> Callable[..., DataFrame]:
    def factory(
        MeteringPointId=metering_point_id,
        MeteringPointType=metering_point_type,
        SettlementMethod=settlement_method,
        GridAreaCode=grid_area_code,
        Resolution=resolution,
        FromGridArea="some-to-grid-area",
        ToGridArea="some-from-grid-area",
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
                        Colname.balance_responsible_id: period[
                            Colname.balance_responsible_id
                        ]
                        if (Colname.balance_responsible_id in period)
                        else BalanceResponsibleId,
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
                        Colname.to_grid_area: period[Colname.to_grid_area]
                        if (Colname.to_grid_area in period)
                        else FromGridArea,
                        Colname.from_grid_area: period[Colname.from_grid_area]
                        if (Colname.from_grid_area in period)
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
                    Colname.balance_responsible_id: BalanceResponsibleId,
                    Colname.metering_point_id: MeteringPointId,
                    Colname.metering_point_type: MeteringPointType,
                    Colname.settlement_method: SettlementMethod,
                    Colname.grid_area: GridAreaCode,
                    Colname.resolution: Resolution,
                    Colname.to_grid_area: FromGridArea,
                    Colname.from_grid_area: ToGridArea,
                    Colname.parent_metering_point_id: ParentMeteringPointId,
                    Colname.from_date: FromDate,
                    Colname.to_date: ToDate,
                    Colname.energy_supplier_id: EnergySupplierId,
                }
            )

        schema = StructType(
            [
                StructField(Colname.balance_responsible_id, StringType(), False),
                StructField(Colname.metering_point_id, StringType(), False),
                StructField(Colname.metering_point_type, StringType(), False),
                StructField(Colname.settlement_method, StringType(), False),
                StructField(Colname.grid_area, StringType(), False),
                StructField(Colname.resolution, StringType(), False),
                StructField(Colname.to_grid_area, StringType(), False),
                StructField(Colname.from_grid_area, StringType(), False),
                StructField(Colname.parent_metering_point_id, StringType(), False),
                StructField(Colname.from_date, TimestampType(), False),
                StructField(Colname.to_date, TimestampType(), True),
                StructField(Colname.energy_supplier_id, StringType(), False),
            ]
        )

        return spark.createDataFrame(df_array, schema=schema)

    return factory


@patch(qualname(DeltaTableReader))
def test__when_metering_point_period_is_in_grid_areas__returns_metering_point_period(
    mock_calculation_input_reader: Mock,
    metering_points_periods_df_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    mock_calculation_input_reader.read_metering_point_periods.return_value = (
        metering_points_periods_df_factory()
    )

    # Act
    raw_master_basis_data = get_metering_point_periods_df(
        mock_calculation_input_reader,
        june_1th,
        june_2th,
        [grid_area_code],
    )

    # Assert
    assert raw_master_basis_data.count() == 1


@patch(qualname(DeltaTableReader))
def test__when_type_is_production__returns_metering_point_period(
    mock_calculation_input_reader: Mock,
    metering_points_periods_df_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    metering_points_periods_df = metering_points_periods_df_factory(
        MeteringPointType=MeteringPointType.PRODUCTION.value
    )
    mock_calculation_input_reader.read_metering_point_periods.return_value = (
        metering_points_periods_df
    )

    # Act
    raw_master_basis_data = get_metering_point_periods_df(
        mock_calculation_input_reader,
        june_1th,
        june_2th,
        [grid_area_code],
    )

    # Assert
    assert raw_master_basis_data.count() == 1


@patch(qualname(DeltaTableReader))
def test__metering_points_have_expected_columns(
    mock_calculation_input_reader: Mock,
    metering_points_periods_df_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    metering_points_periods_df = metering_points_periods_df_factory()
    mock_calculation_input_reader.read_metering_point_periods.return_value = (
        metering_points_periods_df
    )

    # Act
    raw_master_basis_data = get_metering_point_periods_df(
        mock_calculation_input_reader,
        june_1th,
        june_2th,
        [grid_area_code],
    )

    # Assert
    assert (
        raw_master_basis_data.where(
            (col(Colname.metering_point_id) == metering_point_id)
            & (col(Colname.grid_area) == grid_area_code)
            & (col(Colname.from_date) == june_1th)
            & (col(Colname.to_date) == june_2th)
            & (col(Colname.metering_point_type) == metering_point_type)
            & (col(Colname.settlement_method) == settlement_method)
            & (col(Colname.to_grid_area) == "some-to-grid-area")
            & (col(Colname.from_grid_area) == "some-from-grid-area")
            & (col(Colname.resolution) == MeteringPointResolution.HOUR.value)
            & (col(Colname.energy_supplier_id) == energy_supplier_id)
        ).count()
        == 1
    )


@patch(qualname(DeltaTableReader))
def test__when_period_to_date_is_null__returns_metering_point_period_with_to_date_equal_to_period_end(
    mock_calculation_input_reader: Mock,
    metering_points_periods_df_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    metering_points_periods_df = metering_points_periods_df_factory(ToDate=None)
    mock_calculation_input_reader.read_metering_point_periods.return_value = (
        metering_points_periods_df
    )
    period_end = june_2th

    # Act
    raw_master_basis_data = get_metering_point_periods_df(
        mock_calculation_input_reader,
        june_1th,
        period_end,
        [grid_area_code],
    )

    # Assert
    assert raw_master_basis_data.count() == 1
    assert raw_master_basis_data.where(col(Colname.to_date) == period_end).count() == 1


@patch(qualname(DeltaTableReader))
def test__get_metering_point_periods_df__from_date_must_not_be_earlier_than_period_start(
    mock_calculation_input_reader: Mock,
    metering_points_periods_df_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    period_start = june_4th
    period_end = june_6th
    metering_point_period_df = metering_points_periods_df_factory(
        FromDate=june_1th, ToDate=june_10th
    )
    mock_calculation_input_reader.read_metering_point_periods.return_value = (
        metering_point_period_df
    )

    # Act
    master_basis_data = get_metering_point_periods_df(
        mock_calculation_input_reader,
        period_start,
        period_end,
        [grid_area_code],
    )

    # Assert
    assert master_basis_data.collect()[0][Colname.from_date] == period_start


@patch(qualname(DeltaTableReader))
def test__get_metering_point_periods_df__to_date_must_not_be_after_period_end(
    mock_calculation_input_reader: Mock,
    metering_points_periods_df_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    period_start = june_4th
    period_end = june_6th
    metering_point_period_df = metering_points_periods_df_factory(
        FromDate=june_1th, ToDate=june_10th
    )
    mock_calculation_input_reader.read_metering_point_periods.return_value = (
        metering_point_period_df
    )

    # Act
    master_basis_data = get_metering_point_periods_df(
        mock_calculation_input_reader,
        period_start,
        period_end,
        [grid_area_code],
    )

    # Assert
    assert master_basis_data.collect()[0][Colname.to_date] == period_end
