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
from delta.tables import DeltaTable
from package.optimize_job.delta_optimization import _optimize_table, optimize_tables
from pyspark.sql import SparkSession
from tests.helpers.delta_table_utils import write_dataframe_to_table
from pyspark.sql.types import StructType, StructField, StringType
import pytest
from telemetry_logging import Logger
from package.infrastructure.paths import (
    WholesaleResultsInternalDatabase,
)


def test__optimize_tables__optimize_is_in_history_of_delta_table(
    spark: SparkSession,
) -> None:
    # Arrange
    catalog_name = spark.catalog.currentCatalog()
    database_name = WholesaleResultsInternalDatabase.DATABASE_NAME
    table_name = WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME
    table_location = "/tmp/test"
    full_table_name = f"{database_name}.{table_name}"

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
        database_name,
        table_name,
        table_location,
        schema,
    )

    write_dataframe_to_table(
        spark,
        df,
        database_name,
        table_name,
        table_location,
        schema,
        mode="append",
    )

    # Act
    optimize_tables(catalog_name)

    # Assert
    delta_table = DeltaTable.forName(spark, full_table_name)
    assert delta_table.history().filter("operation == 'OPTIMIZE'").count() > 0
