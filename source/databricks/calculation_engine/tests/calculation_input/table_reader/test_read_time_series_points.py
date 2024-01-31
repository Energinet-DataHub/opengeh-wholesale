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
from datetime import datetime
from decimal import Decimal
from pyspark.sql import SparkSession

from package.calculation_input.table_reader import TableReader
from package.calculation_input.schemas import time_series_point_schema
from package.constants import Colname
from package.infrastructure import paths
from tests.helpers.delta_table_utils import write_dataframe_to_table
from tests.helpers.data_frame_utils import assert_dataframes_equal


def _create_time_series_point_row(
    metering_point_id: str = "some-metering-point-id",
) -> dict:
    return {
        Colname.metering_point_id: metering_point_id,
        Colname.quantity: Decimal("1.123456"),
        Colname.quality: "foo",
        Colname.observation_time: datetime(2022, 6, 8, 22, 0, 0),
    }


class TestWhenValidInput:
    def test_returns_expected_df(
        self,
        spark: SparkSession,
        tmp_path: pathlib.Path,
        calculation_input_folder: str,
    ) -> None:
        # Arrange
        calculation_input_path = f"{str(tmp_path)}/{calculation_input_folder}"
        time_series_points_table_location = (
            f"{calculation_input_path}/{paths.TIME_SERIES_POINTS_TABLE_NAME}"
        )
        row = _create_time_series_point_row()
        df = spark.createDataFrame(data=[row], schema=time_series_point_schema)
        write_dataframe_to_table(
            spark,
            df,
            "the_test_database",
            paths.TIME_SERIES_POINTS_TABLE_NAME,
            time_series_points_table_location,
            time_series_point_schema,
        )
        expected = df

        reader = TableReader(spark, calculation_input_path)

        # Act
        actual = reader.read_time_series_points()

        # Assert
        assert_dataframes_equal(actual, expected)
