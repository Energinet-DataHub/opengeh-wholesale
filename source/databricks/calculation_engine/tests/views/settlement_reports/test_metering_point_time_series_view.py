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
from decimal import Decimal

import pytest
from pyspark.sql import SparkSession

from helpers.data_frame_utils import assert_dataframes_equal
from package.infrastructure.paths import (
    SETTLEMENT_REPORT_DATABASE_NAME,
    BASIS_DATA_DATABASE_NAME,
    TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME,
    METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME,
)
from views.settlement_reports.column_names.metering_point_time_series_colname import (
    MeteringPointTimeSeriesColname,
)
from views.settlement_reports.factories.basis_data_metering_point_periods_factory import (
    BasisDataMeteringPointPeriodsFactory,
)
from views.settlement_reports.factories.basis_data_time_series_points_factory import (
    BasisDataTimeSeriesPointsFactory,
)
from views.settlement_reports.factories.settlement_report_metering_point_time_series_view_test_factory import (
    SettlementReportMeteringPointTimeSeriesViewTestFactory,
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
    row1 = factory2.create_row(observation_time=datetime(2021, 1, 1, 23, 0, 0))
    row2 = factory2.create_row(
        observation_time=datetime(2021, 1, 1, 23, 15, 0), quantity=Decimal(1.500)
    )
    # Not use calculation id
    row3 = factory1.create_row(calculation_id="999999999999999999")

    time_series_points = factory2.create_dataframe([row1, row2])
    time_series_points.write.format("delta").mode("overwrite").saveAsTable(
        f"{BASIS_DATA_DATABASE_NAME}.{TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME}"
    )


def test_read_metering_point_time_series_returns_expected_from_settlement_report_metering_point_time_series_view(
    spark: SparkSession,
    migrations_executed: None,
    setup_test_data: None,
) -> None:
    """
    The test verifies that the view "metering_point_time_series" is updated correctly when the underlying
    tables metering_point_periods and basis_data.time_series_points from the basis_data database
    are updated (and that the view exists in the settlement_report database).
    """
    # Arrange
    expected = create_expected(spark)
    reader = ViewReader(spark, SETTLEMENT_REPORT_DATABASE_NAME)

    # Act
    actual = reader.read_metering_point_time_series()

    # Assert
    actual.show(100, False)
    expected.show(100, False)
    assert_dataframes_equal(actual, expected)


def create_expected(spark):
    factory = SettlementReportMeteringPointTimeSeriesViewTestFactory(spark)
    quantities = [
        {
            MeteringPointTimeSeriesColname.observation_time: datetime(
                2021, 1, 1, 23, 0, 0
            ),
            MeteringPointTimeSeriesColname.quantity: Decimal(1.123),
        },
        {
            MeteringPointTimeSeriesColname.observation_time: datetime(
                2021, 1, 1, 23, 15, 0
            ),
            MeteringPointTimeSeriesColname.quantity: Decimal(1.456),
        },
        # Add more dictionaries for more quantity observations
    ]
    row = factory.create_row(quantities=quantities)
    expected = factory.create_dataframe([row])
    return expected
