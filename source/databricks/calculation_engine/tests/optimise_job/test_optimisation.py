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
import os
from delta.tables import DeltaTable
from package.optimise_job.optimisation import optimise_table
from pyspark.sql import SparkSession
from tests.helpers.delta_table_utils import write_dataframe_to_table
from pyspark.sql.types import StructType, StructField, StringType
import pytest
import time


def get_spark_session() -> SparkSession:
    session = (
        SparkSession.builder.master("local")
        .appName("test_optimisation")
        .config("spark.default.parallelism", 1)
        .config("spark.driver.memory", "2g")
        .config("spark.executor.memory", "2g")
        .config("hive.metastore.disallow.incompatible.col.type.changes", "false")
        .getOrCreate()
    )

    return session


def test__optimise_is_in_history_of_delta_table() -> None:
    # Arrange
    spark = get_spark_session()
    mock_database_name = "test_database"
    mock_table_name = "test_table"
    table_location = "/tmp/test"
    full_table_name = f"{mock_database_name}.{mock_table_name}"

    schema = StructType(
        [
            StructField("name", StringType(), False),
            StructField("row", StringType(), False),
        ]
    )
    df = spark.createDataFrame(
        [("1", "foo"), ("2", "bar"), ("3", "test")], schema=schema
    )

    write_dataframe_to_table(
        spark,
        df,
        mock_database_name,
        mock_table_name,
        table_location,
        schema,
    )

    write_dataframe_to_table(
        spark,
        df,
        mock_database_name,
        mock_table_name,
        table_location,
        schema,
        mode="append",
    )

    # Act
    optimise_table(spark, mock_database_name, mock_table_name)
    # time.sleep(60)

    # Assert
    delta_table = DeltaTable.forName(spark, full_table_name)
    assert delta_table.history().filter("operation == 'OPTIMIZE'").count() > 0
