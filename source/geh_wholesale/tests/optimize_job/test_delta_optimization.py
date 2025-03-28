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
from geh_common.telemetry import Logger
from pyspark.sql import SparkSession
from pyspark.sql.types import StringType, StructField, StructType

from geh_wholesale.optimize_job.delta_optimization import _optimize_table
from tests.helpers.delta_table_utils import write_dataframe_to_table


def test__optimize_table__optimize_is_in_history_of_delta_table(
    spark: SparkSession,
) -> None:
    # Arrange
    database_name = "test_database"
    table_name = "test_table"
    table_location = "/tmp/test"
    full_table_name = f"{database_name}.{table_name}"
    logger = Logger("test_logger")

    schema = StructType(
        [
            StructField("name", StringType(), False),
            StructField("row", StringType(), False),
        ]
    )
    df = spark.createDataFrame([("1", "foo"), ("2", "bar"), ("3", "test")], schema=schema)

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
    _optimize_table(spark, database_name, table_name, logger)

    # Assert
    delta_table = DeltaTable.forName(spark, full_table_name)
    assert delta_table.history().filter("operation == 'OPTIMIZE'").count() > 0
