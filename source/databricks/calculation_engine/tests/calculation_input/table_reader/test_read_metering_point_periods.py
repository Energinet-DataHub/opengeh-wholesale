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
from pyspark.sql import SparkSession, DataFrame

from package.calculation_input.table_reader import TableReader
from package.calculation_input.schemas import metering_point_period_schema
from tests.helpers.delta_table_utils import write_dataframe_to_table
from tests.helpers.data_frame_utils import assert_dataframes_equal


class TestWhenValidInput:
    def test_returns_expected_df(
        self,
        spark: SparkSession,
        tmp_path: pathlib.Path,
        calculation_input_folder: str,
    ) -> None:
        # Arrange
        calculation_input_path = f"{str(tmp_path)}/{calculation_input_folder}"
        table_location = f"{calculation_input_path}/metering_point_periods"
        row = _create_metering_point_period_row()
        df = spark.createDataFrame(data=[row], schema=metering_point_period_schema)
        write_dataframe_to_table(
            spark,
            df,
            "test_database",
            "metering_point_periods",
            table_location,
            metering_point_period_schema,
        )
        expected = _map_metering_point_type_and_settlement_method(df)
        reader = TableReader(spark, calculation_input_path)

        # Act
        actual = reader.read_metering_point_periods(
            DEFAULT_FROM_DATE,
            DEFAULT_TO_DATE,
            [DEFAULT_GRID_AREA],
        )

        # Assert
        assert_dataframes_equal(actual, expected)
