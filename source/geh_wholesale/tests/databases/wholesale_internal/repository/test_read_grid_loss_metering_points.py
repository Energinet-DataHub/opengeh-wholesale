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
from unittest import mock

import pyspark.sql.functions as f
import pytest
from pyspark.sql import SparkSession

from geh_wholesale.constants import Colname
from geh_wholesale.databases import wholesale_internal
from geh_wholesale.databases.wholesale_internal.schemas import (
    grid_loss_metering_point_ids_schema,
)
from geh_wholesale.infrastructure.paths import WholesaleInternalDatabase
from tests.helpers.data_frame_utils import assert_dataframes_equal
from tests.helpers.delta_table_utils import write_dataframe_to_table

DEFAULT_OBSERVATION_TIME = datetime(2022, 6, 8, 22, 0, 0)
DEFAULT_FROM_DATE = datetime(2022, 6, 8, 22, 0, 0)
DEFAULT_TO_DATE = datetime(2022, 6, 9, 22, 0, 0)


def _create_grid_loss_metering_point_row() -> dict:
    return {
        Colname.metering_point_id: "570715000000682292",
    }


class TestWhenContractMismatch:
    def test_raises_assertion_error(self, spark: SparkSession) -> None:
        # Arrange
        row = _create_grid_loss_metering_point_row()
        reader = wholesale_internal.WholesaleInternalRepository(mock.Mock(), "dummy_catalog_name")
        df = spark.createDataFrame(data=[row], schema=grid_loss_metering_point_ids_schema)
        df = df.drop(Colname.metering_point_id)
        df = df.withColumn("test", f.lit("test"))

        # Act & Assert
        with mock.patch.object(reader._spark.read.format("delta"), "table", return_value=df):
            with pytest.raises(AssertionError) as exc_info:
                reader.read_grid_loss_metering_point_ids()

            assert "Schema mismatch" in str(exc_info.value)


class TestWhenValidInput:
    def test_returns_expected_df(
        self,
        spark: SparkSession,
        tmp_path: pathlib.Path,
    ) -> None:
        # Arrange
        calculation_input_path = f"{str(tmp_path)}/calculation_input"
        table_location = f"{calculation_input_path}/{WholesaleInternalDatabase.GRID_LOSS_METERING_POINT_IDS_TABLE_NAME}"
        row = _create_grid_loss_metering_point_row()
        df = spark.createDataFrame(data=[row], schema=grid_loss_metering_point_ids_schema)
        write_dataframe_to_table(
            spark,
            df,
            WholesaleInternalDatabase.DATABASE_NAME,
            WholesaleInternalDatabase.GRID_LOSS_METERING_POINT_IDS_TABLE_NAME,
            table_location,
            grid_loss_metering_point_ids_schema,
        )
        expected = df
        reader = wholesale_internal.WholesaleInternalRepository(spark, "spark_catalog")

        # Act
        actual = reader.read_grid_loss_metering_point_ids()

        # Assert
        assert_dataframes_equal(actual, expected)


# TODO BJM: Doc that the tests is about being resilient to extra columns (non-breaking change)
#           What about different ordering of columns?
class TestWhenValidInputAndExtraColumns:
    def test_returns_expected_df(self, spark: SparkSession) -> None:
        # Arrange
        row = _create_grid_loss_metering_point_row()
        reader = wholesale_internal.WholesaleInternalRepository(mock.Mock(), "spark_catalog")
        df = spark.createDataFrame(data=[row], schema=grid_loss_metering_point_ids_schema)
        df = df.withColumn("test", f.lit("test"))

        # Act & Assert
        with mock.patch.object(reader._spark.read.format("delta"), "table", return_value=df):
            reader.read_grid_loss_metering_point_ids()
