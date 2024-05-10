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
import pytest
from package.calculation.preparation.prepared_data_reader import PreparedDataReader
from package.calculation.input import TableReader
from unittest.mock import MagicMock, Mock
from pyspark.sql import SparkSession, DataFrame
from package.calculation.input.schemas.grid_loss_metering_points_schema import (
    grid_loss_metering_points_schema,
)
from typing import List


def _get_metering_point_dataframe(spark: SparkSession, data: List[str]) -> DataFrame:
    return spark.createDataFrame(data, grid_loss_metering_points_schema)


def _init_test(df_table_reader_mock_dataframe: DataFrame) -> PreparedDataReader:
    table_reader = Mock()
    # Mock should return sample dataframe when called
    table_reader.read_grid_loss_metering_points = MagicMock(
        return_value=df_table_reader_mock_dataframe
    )
    return PreparedDataReader(table_reader)


@pytest.mark.parametrize(
    "table_reader_grid_loss_mock_data, calculation_metering_point_data, expected_count_after_join",
    [
        (
            [
                ('A',),
                ('B',),
                ('C',),
                ('D',),
                ('E',),
                ('F',),
            ],
            [
                ('C',),
                ('D',),
            ],
            2,
        ),
        (
            [
                ('A',),
                ('B',),
                ('C',),
                ('D',),
                ('E',),
                ('F',),
            ],
            [
                ('A',),
                ('B',),
                ('C',),
                ('D',),
                ('E',),
                ('F',),
            ],
            6,
        ),
        (
            [
                ('A',),
                ('B',),
                ('C',),
                ('D',),
                ('E',),
                ('F',),
            ],
            [
                ('G',),
                ('H',),
            ],
            0,
        ),
        (
            [
                ('A',),
                ('B',),
                ('C',),
                ('C',),
                ('D',),
                ('E',),
                ('F',),
            ],
            [
                ('C',),
                ('D',),
            ],
            2,
        ),
        (
            [
                ('A',),
                ('B',),
                ('C',),
                ('C',),
                ('D',),
                ('E',),
                ('F',),
            ],
            [
                ('C',),
                ('C',),
                ('D',),
            ],
            2,
        ),
    ],
)
def test__when_get_grid_loss_metering_points__count_is_correct(
    spark: SparkSession,
    table_reader_grid_loss_mock_data: List[str],
    calculation_metering_point_data: List[str],
    expected_count_after_join: int,
):
    df_table_reader_mock_dataframe = _get_metering_point_dataframe(
        spark, table_reader_grid_loss_mock_data
    )
    prepared_data_reader = _init_test(df_table_reader_mock_dataframe)
    
    df_calculation_metering_point_data = _get_metering_point_dataframe(
        spark, calculation_metering_point_data
    )

    result = prepared_data_reader.get_grid_loss_metering_points(
        df_calculation_metering_point_data
    )

    assert result.df.count() == expected_count_after_join
