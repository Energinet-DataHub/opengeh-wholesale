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

# from package.steps.aggregation.aggregation_result_formatter import (
#     create_dataframe_from_aggregation_result_schema,
# )
from pyspark.sql import DataFrame

# import pandas as pd
from package.constants import Colname
from package.constants.time_series_type import TimeSeriesType

# from package.shared.data_classes import Metadata
from source.databricks.package.result_writer import (
    _get_actors_df,
)
from pyspark.sql.types import (
    IntegerType,
    StructType,
    StructField,
    StringType,
    TimestampType,
)
from pyspark.sql import DataFrame, SparkSession


def test_get_actor_list__has_required_columns(spark: SparkSession) -> None:

    # Arrange
    test_data = [
        ("505", TimeSeriesType.NON_PROFILED_CONSUMPTION.value, "123456", "dummy"),
        ("506", TimeSeriesType.NON_PROFILED_CONSUMPTION.value, "123456", "dummy"),
    ]

    schema = StructType(
        [
            StructField(Colname.grid_area, StringType(), False),
            StructField(Colname.time_series_type, StringType(), False),
            StructField(Colname.gln, StringType(), False),
            StructField("Dummy", StringType(), False),
        ]
    )

    required_column_names = {Colname.grid_area, Colname.time_series_type, Colname.gln}
    result_df = spark.createDataFrame(data=test_data, schema=schema)

    # Act
    actors = _get_actors_df(result_df)

    # Assert
    actual_column_names = actors.columns
    assert required_column_names.issubset(actual_column_names)


def test__get_actors__returns_distinct_rows(spark: SparkSession) -> None:

    # Arrange
    row1 = ("505", TimeSeriesType.NON_PROFILED_CONSUMPTION.value, "123456", "dummy")
    row2 = ("506", TimeSeriesType.NON_PROFILED_CONSUMPTION.value, "123456", "dummy")

    test_data = [row1, row2, row1, row2]

    schema = StructType(
        [
            StructField(Colname.grid_area, StringType(), False),
            StructField(Colname.time_series_type, StringType(), False),
            StructField(Colname.gln, StringType(), False),
            StructField("Dummy", StringType(), False),
        ]
    )
    result_df = spark.createDataFrame(data=test_data, schema=schema)

    # Act
    actors_df = _get_actors_df(result_df)

    # Assert
    assert actors_df.count() == 2


# def test_get_actor_list__(spark: SparkSession) -> None:

#     # Arrange
#     test_data = [
#         ("505", TimeSeriesType.NON_PROFILED_CONSUMPTION.value, "123456", "dummy1"),
#         ("506", TimeSeriesType.NON_PROFILED_CONSUMPTION.value, "123456", "dummy2"),
#         ("505", TimeSeriesType.NON_PROFILED_CONSUMPTION.value, "123456", "dummy3"),
#         ("506", TimeSeriesType.NON_PROFILED_CONSUMPTION.value, "123456", "dummy4"),
#     ]

#     schema = StructType(
#         [
#             StructField(Colname.grid_area, StringType(), False),
#             StructField(Colname.time_series_type, StringType(), False),
#             StructField(Colname.gln, StringType(), False),
#             StructField("Dummy", StringType(), False),
#         ]
#     )

#     required_column_names = {Colname.grid_area, Colname.time_series_type, Colname.gln}
#     result_df = spark.createDataFrame(data=test_data, schema=schema)

#     # Act
#     actors = _get_actors_df(result_df)

#     # Assert
#     actors.show()
#     actual_column_names = actors.columns
#     assert required_column_names.issubset(actual_column_names)
