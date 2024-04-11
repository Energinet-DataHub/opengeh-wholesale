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

from pyspark.sql import SparkSession, dataframe

from features.utils.factories.basis_data import BasisDataMeteringPointPeriodsFactory
from features.utils.factories.basis_data.basis_data_time_series_points_factory import BasisDataTimeSeriesPointsFactory
from helpers.data_frame_utils import assert_dataframes_equal
from package.infrastructure.paths import (
    BASIS_DATA_DATABASE_NAME,
    SETTLEMENT_REPORT_DATABASE_NAME,
    TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME,
    METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME,
)
from views.factories.metering_point_time_series_colname import (
    MeteringPointTimeSeriesColname,
)
from views.factories.settlement_report_metering_point_time_series_view_test_factory import (
    SettlementReportMeteringPointTimeSeriesViewTestFactory,
)
from views.view_reader import ViewReader


def create_expected(spark: SparkSession, df: dataframe) -> dataframe:
    view_factory = SettlementReportMeteringPointTimeSeriesViewTestFactory(spark)
    first = df.first()

    row = view_factory.create_row(
        first[MeteringPointTimeSeriesColname.calculation_id],
        first[MeteringPointTimeSeriesColname.metering_point_id],
        first[MeteringPointTimeSeriesColname.metering_point_type],
        first[MeteringPointTimeSeriesColname.resolution],
        first[MeteringPointTimeSeriesColname.grid_area],
        first[MeteringPointTimeSeriesColname.energy_supplier_id],
        first[MeteringPointTimeSeriesColname.observation_day],
        first[MeteringPointTimeSeriesColname.quantities],
    )

    return view_factory.create_dataframe([row])


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
    factory1 = BasisDataMeteringPointPeriodsFactory(spark)
    row = factory1.create_row()
    df1 = factory1.create_dataframe([row])
    df1.write.format("delta").mode("overwrite").saveAsTable(
        f"{BASIS_DATA_DATABASE_NAME}.{METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME}"
    )
    factory = BasisDataTimeSeriesPointsFactory(spark)
    row = factory.create_row()
    df = factory.create_dataframe([row])
    df.write.format("delta").mode("overwrite").saveAsTable(
        f"{BASIS_DATA_DATABASE_NAME}.{TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME}"
    )
    expected = create_expected(spark, df, df1)
    sut = ViewReader(spark, SETTLEMENT_REPORT_DATABASE_NAME)

    # Act
    actual = sut.read_metering_point_periods()

    # Assert
    assert_dataframes_equal(actual, expected)
