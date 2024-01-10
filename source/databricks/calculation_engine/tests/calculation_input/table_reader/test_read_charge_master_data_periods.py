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
import pytest
from pyspark.sql import SparkSession, DataFrame
import pyspark.sql.functions as f


from package.calculation_input.table_reader import TableReader
from package.calculation_input.schemas import charge_master_data_periods_schema
from package.constants import Colname
from tests.helpers.delta_table_utils import write_dataframe_to_table
from tests.helpers.data_frame_utils import assert_dataframes_equal

DEFAULT_FROM_DATE = datetime(2022, 6, 8, 22, 0, 0)
DEFAULT_TO_DATE = datetime(2022, 6, 8, 22, 0, 0)


def _create_charge_master_period_row() -> dict:
    return {
        Colname.charge_code: "foo",
        Colname.charge_type: "foo",
        Colname.charge_owner: "foo",
        Colname.resolution: "foo",
        Colname.charge_tax: False,
        Colname.from_date: DEFAULT_FROM_DATE,
        Colname.to_date: DEFAULT_TO_DATE,
    }


def _add_charge_key(df: DataFrame) -> DataFrame:
    return df.withColumn(Colname.charge_key, f.lit("foo-foo-foo"))


class TestWhenSchemaMismatch:
    def test__raises_assertion_error(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        row = _create_charge_master_period_row()
        reader = TableReader(mock.Mock(), "dummy_calculation_input_path")
        df = spark.createDataFrame(data=[row], schema=charge_master_data_periods_schema)
        df = df.withColumn("test", f.lit("test"))

        # Act & Assert
        with mock.patch.object(
            reader._spark.read.format("delta"), "load", return_value=df
        ):
            with pytest.raises(AssertionError) as exc_info:
                reader.read_charge_master_data_periods()

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
        table_location = f"{calculation_input_path}/charge_masterdata_periods"
        row = _create_charge_master_period_row()
        df = spark.createDataFrame(data=[row], schema=charge_master_data_periods_schema)
        write_dataframe_to_table(
            spark,
            df,
            "test_database",
            "charge_master_data_periods",
            table_location,
            charge_master_data_periods_schema,
        )
        expected = _add_charge_key(df)
        reader = TableReader(spark, calculation_input_path)

        # Act
        actual = reader.read_charge_master_data_periods()

        # Assert
        assert_dataframes_equal(actual, expected)
