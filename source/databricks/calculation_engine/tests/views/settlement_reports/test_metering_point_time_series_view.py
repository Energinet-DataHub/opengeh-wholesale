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

from pyspark.sql import SparkSession

from helpers.data_frame_utils import assert_dataframes_equal
from package.infrastructure.paths import (
    SETTLEMENT_REPORT_DATABASE_NAME,
)
from views.settlement_reports.factories.settlement_report_metering_point_time_series_view_test_factory import (
    SettlementReportMeteringPointTimeSeriesViewTestFactory,
)
from views.view_reader import ViewReader


def test_read_metering_point_time_series_returns_expected_from_settlement_report_metering_point_time_series_view(
    spark: SparkSession,
    migrations_executed,
    setup_test_data,
) -> None:
    """
    The test verifies that the view "metering_point_time_series" is updated when the underlying
    tables basis_data.metering_point_periods and basis_data.time_series_points table are updated
    (and that the view exists in the wholesale schema (database) settlement_report).
    """
    # Arrange
    factory = SettlementReportMeteringPointTimeSeriesViewTestFactory(spark)
    expected = factory.create_dataframe()
    reader = ViewReader(spark, SETTLEMENT_REPORT_DATABASE_NAME)

    # Act
    actual = reader.read_metering_point_time_series()

    # Assert
    actual.show()
    expected.show()
    assert_dataframes_equal(actual, expected)
