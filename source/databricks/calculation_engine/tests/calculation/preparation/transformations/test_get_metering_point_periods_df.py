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
from typing import Callable

from pyspark.sql.functions import col
from pyspark.sql import DataFrame, SparkSession

import pytest
from unittest.mock import patch, Mock

from package import calculation_input
from package.calculation_input.table_reader import TableReader
from package.calculation.preparation.transformations import (
    get_metering_point_periods_df,
)
from package.codelists import (
    MeteringPointType,
    SettlementMethod,
    MeteringPointResolution,
)
from package.constants import Colname
import tests.calculation.preparation.transformations.metering_point_periods_factory as factory

# Factory defaults
grid_area = "805"
grid_area_link_id = "the-grid-area-link-id"
metering_point_id = "the-metering-point-id"
energy_supplier_id = "the-energy-supplier-id"
# metering_point_type = MeteringPointType.PRODUCTION.value
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


class TestWhenValidInput:
    @pytest.mark.parametrize("metering_point_type", list(MeteringPointType))
    @patch.object(calculation_input, TableReader.__name__)
    def test__returns_metering_point_period_for_(
        self,
        mock_calculation_input_reader: Mock,
        spark: SparkSession,
        metering_point_type: MeteringPointType,
    ) -> None:
        # Arrange
        row = factory.create_row(
            metering_point_type=metering_point_type,
        )
        mock_calculation_input_reader.read_metering_point_periods.return_value = (
            factory.create(spark, row)
        )

        # Act
        actual = get_metering_point_periods_df(
            mock_calculation_input_reader,
            factory.DEFAULT_FROM_DATE,
            factory.DEFAULT_TO_DATE,
            [factory.DEFAULT_GRID_AREA],
        )

        # Assert
        actual_rows = actual.collect()
        assert len(actual_rows) == 1
        assert actual_rows[0][Colname.metering_point_type] == metering_point_type.value


@patch.object(calculation_input, TableReader.__name__)
def test__metering_points_have_expected_columns(
    mock_calculation_input_reader: Mock,
    metering_points_periods_df_factory: Callable[..., DataFrame],
) -> None:
    # Arrange

    # Act
    metering_points_periods = get_metering_point_periods_df(
        mock_calculation_input_reader,
        june_1th,
        june_2th,
        [grid_area],
    )

    # Assert
    assert (
        metering_points_periods.where(
            (col(Colname.metering_point_id) == metering_point_id)
            & (col(Colname.grid_area) == grid_area)
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


@patch.object(calculation_input, TableReader.__name__)
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
        [grid_area],
    )

    # Assert
    assert raw_master_basis_data.count() == 1
    assert raw_master_basis_data.where(col(Colname.to_date) == period_end).count() == 1


@patch.object(calculation_input, TableReader.__name__)
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
        [grid_area],
    )

    # Assert
    assert master_basis_data.collect()[0][Colname.from_date] == period_start


@patch.object(calculation_input, TableReader.__name__)
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
        [grid_area],
    )

    # Assert
    assert master_basis_data.collect()[0][Colname.to_date] == period_end
