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
from typing import Tuple

from pyspark.sql import SparkSession, dataframe

from features.utils.factories.basis_data import BasisDataMeteringPointPeriodsFactory
from features.utils.factories.basis_data.basis_data_time_series_points_factory import (
    BasisDataTimeSeriesPointsFactory,
)
from helpers.data_frame_utils import assert_dataframes_equal
from package.constants import TimeSeriesColname, MeteringPointPeriodColname
from package.infrastructure.paths import (
    BASIS_DATA_DATABASE_NAME,
    SETTLEMENT_REPORT_DATABASE_NAME,
    TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME,
    METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME,
)
from views.settlement_reports.factories.settlement_report_metering_point_time_series_view_test_factory import (
    SettlementReportMeteringPointTimeSeriesViewTestFactory,
)
from views.view_reader import ViewReader


def create_expected(
    spark: SparkSession,
    metering_point_periods: dataframe,
    time_series_points: dataframe,
) -> dataframe:
    factory = SettlementReportMeteringPointTimeSeriesViewTestFactory(spark)
    metering_point_period = metering_point_periods.first()
    time_series_point = time_series_points.first()

    row = factory.create_row(
        calculation_id=metering_point_period[MeteringPointPeriodColname.calculation_id],
        metering_point_id=metering_point_period[
            MeteringPointPeriodColname.metering_point_id
        ],
        metering_point_type=metering_point_period[
            MeteringPointPeriodColname.metering_point_type
        ],
        resolution=metering_point_period[MeteringPointPeriodColname.resolution],
        grid_area=metering_point_period[MeteringPointPeriodColname.grid_area],
        energy_supplier_id=metering_point_period[
            MeteringPointPeriodColname.energy_supplier_id
        ],
        observation_day=time_series_point[TimeSeriesColname.observation_time],
        quantities=time_series_point[TimeSeriesColname.quantity],
    )

    return factory.create_dataframe([row])


def test_read_metering_point_time_series_returns_expected_from_settlement_report_metering_point_time_series_view(
    spark: SparkSession,
    migrations_executed: None,
) -> None:
    """
    The test verifies that the view "metering_point_time_series" is updated when the underlying
    tables basis_data.metering_point_periods and basis_data.time_series_points table are updated
    (and that the view exists in the wholesale schema (database) settlement_report).
    """
    # Arrange
    (time_series_points, metering_point_periods) = setup_test_data(spark)
    expected = create_expected(spark, metering_point_periods, time_series_points)
    reader = ViewReader(spark, SETTLEMENT_REPORT_DATABASE_NAME)

    # Act
    actual = reader.read_metering_point_time_series()

    # Assert
    expected.show()
    actual.show()
    assert_dataframes_equal(actual, expected)


def setup_test_data(spark: SparkSession) -> Tuple[dataframe, dataframe]:
    factory1 = BasisDataMeteringPointPeriodsFactory(spark)
    row = factory1.create_row()
    metering_point_periods = factory1.create_dataframe([row])
    metering_point_periods.show()
    metering_point_periods.write.format("delta").mode("overwrite").saveAsTable(
        f"{BASIS_DATA_DATABASE_NAME}.{METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME}"
    )

    factory = BasisDataTimeSeriesPointsFactory(spark)
    row = factory.create_row(observation_time=datetime(2020, 1, 1, 23, 0, 0))
    time_series_points = factory.create_dataframe([row])
    time_series_points.show()
    time_series_points.write.format("delta").mode("overwrite").saveAsTable(
        f"{BASIS_DATA_DATABASE_NAME}.{TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME}"
    )
    return time_series_points, metering_point_periods
