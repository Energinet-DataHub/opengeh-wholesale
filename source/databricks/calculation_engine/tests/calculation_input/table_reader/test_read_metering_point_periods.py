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
from unittest import mock
import pytest
from pyspark.sql import SparkSession
import pyspark.sql.functions as f

from package.calculation_input.table_reader import TableReader
from package.calculation_input.schemas import metering_point_period_schema
import calculation_input.factories.input_metering_point_periods_factory as factory
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
        row = factory.create_row()
        df = factory.create_dataframe(spark, row)
        write_dataframe_to_table(
            spark,
            df,
            "test_database",
            "metering_point_periods",
            table_location,
            metering_point_period_schema,
        )
        reader = TableReader(spark, calculation_input_path)

        # Act
        actual = reader.read_metering_point_periods()

        # Assert
        assert_dataframes_equal(actual, df)


class TestWhenSchemaMismatch:
    def test_raises_assertion_error(self, spark: SparkSession) -> None:
        # Arrange
        reader = TableReader(mock.Mock(), "dummy_calculation_input_path")
        row = factory.create_row()
        df = factory.create_dataframe(spark, row)
        df = df.withColumn("test", f.lit("test"))

        # Act & Assert
        with mock.patch.object(
            reader._spark.read.format("delta"), "load", return_value=df
        ):
            with pytest.raises(AssertionError) as exc_info:
                reader.read_metering_point_periods()

            assert "Schema mismatch" in str(exc_info.value)
