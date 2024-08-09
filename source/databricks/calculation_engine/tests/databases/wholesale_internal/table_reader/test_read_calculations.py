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

from package.codelists import CalculationType
from package.databases import wholesale_internal
from package.databases.table_column_names import TableColumnNames
from package.databases.wholesale_basis_data_internal.basis_data_colname import (
    CalculationsColumnName,
)
from package.databases.wholesale_internal.schemas import (
    calculations_schema,
)
from package.infrastructure.paths import WholesaleInternalDatabase
from tests.helpers.data_frame_utils import assert_dataframes_equal
from tests.helpers.delta_table_utils import write_dataframe_to_table


def _create_calculation_row() -> dict:
    return {
        TableColumnNames.calculation_id: str(uuid.uuid4()),
        TableColumnNames.calculation_type: CalculationType.BALANCE_FIXING.value,
        TableColumnNames.calculation_period_start: datetime(2022, 6, 8, 22, 0, 0),
        TableColumnNames.calculation_period_end: datetime(2022, 6, 9, 22, 0, 0),
        TableColumnNames.calculation_execution_time_start: datetime(
            2022, 6, 8, 22, 0, 0
        ),
        TableColumnNames.created_by_user_id: str(uuid.uuid4()),
        TableColumnNames.calculation_version: 1,
    }


class TestWhenContractMismatch:
    def test_raises_assertion_error(self, spark: SparkSession) -> None:
        # Arrange
        row = _create_calculation_row()
        table_reader = wholesale_internal.WholesaleInternalRepository(
            mock.Mock(), "dummy_catalog_name"
        )
        df = spark.createDataFrame(data=[row], schema=calculations_schema)
        df = df.drop(CalculationsColumnName.calculation_id)
        df = df.withColumn("test", f.lit("test"))

        # Act & Assert
        with mock.patch.object(
            table_reader._spark.read.format("delta"), "table", return_value=df
        ):
            with pytest.raises(AssertionError) as exc_info:
                table_reader.read_calculations()

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
        calculations_table_location = f"{calculation_input_path}/{WholesaleInternalDatabase.CALCULATIONS_TABLE_NAME}"
        row = _create_calculation_row()
        df = spark.createDataFrame(data=[row], schema=calculations_schema)
        write_dataframe_to_table(
            spark,
            df,
            WholesaleInternalDatabase.DATABASE_NAME,
            WholesaleInternalDatabase.CALCULATIONS_TABLE_NAME,
            calculations_table_location,
            calculations_schema,
        )
        expected = df

        table_reader = wholesale_internal.WholesaleInternalRepository(
            spark, "spark_catalog"
        )

        # Act
        actual = table_reader.read_calculations()

        # Assert
        assert_dataframes_equal(actual, expected)
