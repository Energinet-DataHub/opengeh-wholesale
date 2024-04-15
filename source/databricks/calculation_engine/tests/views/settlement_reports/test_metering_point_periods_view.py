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

import pytest
from pyspark.sql import SparkSession

from helpers.data_frame_utils import assert_dataframes_equal
from package.infrastructure.paths import (
    SETTLEMENT_REPORT_DATABASE_NAME,
    BASIS_DATA_DATABASE_NAME,
    TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME,
    METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME,
)
from views.settlement_reports.factories.basis_data_metering_point_periods_factory import (
    BasisDataMeteringPointPeriodsFactory,
)
from views.settlement_reports.factories.basis_data_time_series_points_factory import (
    BasisDataTimeSeriesPointsFactory,
)
from views.settlement_reports.factories.settlement_report_metering_point_periods_view_test_factory import (
    SettlementReportMeteringPointPeriodsViewTestFactory,
)
from views.view_reader import ViewReader


@pytest.fixture(scope="module")
def setup_test_data(spark: SparkSession) -> None:
    factory1 = BasisDataMeteringPointPeriodsFactory(spark)
    row = factory1.create_row()
    metering_point_periods = factory1.create_dataframe([row])
    metering_point_periods.write.format("delta").mode("overwrite").saveAsTable(
        f"{BASIS_DATA_DATABASE_NAME}.{METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME}"
    )

    factory2 = BasisDataTimeSeriesPointsFactory(spark)
    row = factory2.create_row(observation_time=datetime(2021, 1, 1, 23, 0, 0))
    time_series_points = factory2.create_dataframe([row])
    time_series_points.write.format("delta").mode("overwrite").saveAsTable(
        f"{BASIS_DATA_DATABASE_NAME}.{TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME}"
    )


def test_read_metering_point_periods_returns_expected_from_settlement_report_metering_point_periods_view(
    spark: SparkSession,
    migrations_executed: None,
    setup_test_data: None,
) -> None:
    """
    The test verifies that the view "metering_point_periods" is updated when the underlying
    basis_data.metering_point_periods table is updated (and that the view exists in the
    wholesale database settlement_report).
    """
    # Arrange
    factory = SettlementReportMeteringPointPeriodsViewTestFactory(spark)
    expected = factory.create_dataframe()
    reader = ViewReader(spark, SETTLEMENT_REPORT_DATABASE_NAME)

    # Act
    actual = reader.read_metering_point_periods()

    # Assert
    assert_dataframes_equal(actual, expected)
