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
import pathlib

from pyspark.sql import SparkSession

from features.utils.factories.basis_data.basis_data_metering_point_periods_factory import (
    BasisDataMeteringPointPeriodsFactory as Factory,
)
from helpers.data_frame_utils import assert_dataframes_equal
from package.calculation.basis_data.schemas import metering_point_period_schema
from package.infrastructure.paths import (
    BASIS_DATA_DATABASE_NAME,
    METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME,
    SETTLEMENT_REPORT_DATABASE_NAME,
)
from views.view_reader import ViewReader


def test_read_metering_point_periods_returns_expected_from_settlement_report_metering_point_periods_view(
    spark: SparkSession,
    migrations_executed: None,
    tmp_path: pathlib.Path,
) -> None:
    """
    This test verifies that the view metering_point_periods exists in the schema/database settlement_report,
    and that the view is updated when the basis_data.metering_point_periods table is updated.
    """
    # Arrange
    row = Factory.create_row()
    expected = spark.createDataFrame(data=[row], schema=metering_point_period_schema)
    expected.write.format("delta").mode("overwrite").saveAsTable(
        f"{BASIS_DATA_DATABASE_NAME}.{METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME}"
    )

    reader = ViewReader(spark, SETTLEMENT_REPORT_DATABASE_NAME)

    # Act
    actual = reader.read_metering_point_periods()

    # Assert
    actual.show()
    assert_dataframes_equal(actual, expected)
