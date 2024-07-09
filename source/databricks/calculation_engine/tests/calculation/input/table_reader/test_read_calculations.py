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
import uuid
from datetime import datetime
from unittest import mock

import pyspark.sql.functions as f
import pytest
from pyspark.sql import SparkSession

from package.calculation.output.basis_data.schemas import hive_calculations_schema
from package.calculation.input import TableReader
from package.codelists import CalculationType
from package.constants.basis_data_colname import CalculationsColumnName
from package.infrastructure.paths import HiveBasisDataDatabase
from tests.helpers.data_frame_utils import assert_dataframes_equal
from tests.helpers.delta_table_utils import write_dataframe_to_table


def _create_calculation_row() -> dict:
    return {
        CalculationsColumnName.calculation_id: str(uuid.uuid4()),
        CalculationsColumnName.calculation_type: CalculationType.BALANCE_FIXING.value,
        CalculationsColumnName.period_start: datetime(2022, 6, 8, 22, 0, 0),
        CalculationsColumnName.period_end: datetime(2022, 6, 9, 22, 0, 0),
        CalculationsColumnName.execution_time_start: datetime(2022, 6, 8, 22, 0, 0),
        CalculationsColumnName.created_by_user_id: str(uuid.uuid4()),
        CalculationsColumnName.version: 1,
    }


class TestWhenContractMismatch:
    def test_raises_assertion_error(self, spark: SparkSession) -> None:
        # Arrange
        row = _create_calculation_row()
        reader = TableReader(mock.Mock(), "dummy_calculation_input_path")
        df = spark.createDataFrame(data=[row], schema=hive_calculations_schema)
        df = df.withColumn("test", f.lit("test"))

        # Act & Assert
        with mock.patch.object(
            reader._spark.read.format("delta"), "load", return_value=df
        ):
            with pytest.raises(AssertionError) as exc_info:
                reader.read_calculations()

            assert "Schema mismatch" in str(exc_info.value)


class TestWhenValidInput:
    def test_returns_expected_df(
        self,
        spark: SparkSession,
        tmp_path: pathlib.Path,
        calculation_input_folder: str,
    ) -> None:
        # Arrange
        calculation_input_path = f"{str(tmp_path)}/{calculation_input_folder}"
        calculations_table_location = (
            f"{calculation_input_path}/{HiveBasisDataDatabase.CALCULATIONS_TABLE_NAME}"
        )
        row = _create_calculation_row()
        df = spark.createDataFrame(data=[row], schema=hive_calculations_schema)
        write_dataframe_to_table(
            spark,
            df,
            HiveBasisDataDatabase.DATABASE_NAME,
            HiveBasisDataDatabase.CALCULATIONS_TABLE_NAME,
            calculations_table_location,
            hive_calculations_schema,
        )
        expected = df

        reader = TableReader(spark, calculation_input_path)

        # Act
        actual = reader.read_calculations()

        # Assert
        assert_dataframes_equal(actual, expected)
