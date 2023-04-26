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

from datetime import datetime
from decimal import Decimal
import os
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import lit, col
import pytest
import shutil
import uuid

from package.codelists import (
    AggregationLevel,
    ProcessType,
    TimeSeriesType,
    TimeSeriesQuality,
)
from package.constants import ResultTableColName
from package.datamigration.migration import _apply_migration
from package.datamigration.migration_script_args import MigrationScriptArgs
from package.datamigration.uncommitted_migrations import _get_all_migrations
from package.schemas import results_schema

TABLE_NAME = "result"
DATABASE_NAME = "wholesale_output"


def _create_df(spark: SparkSession) -> DataFrame:
    row = {
        ResultTableColName.grid_area: "543",
        ResultTableColName.energy_supplier_id: "energy_supplier_id",
        ResultTableColName.balance_responsible_id: "balance_responsible_id",
        ResultTableColName.quantity: Decimal("1.123"),
        ResultTableColName.quantity_quality: "missing",
        ResultTableColName.time: datetime(2020, 1, 1, 0, 0),
        ResultTableColName.aggregation_level: "total_ga",
        ResultTableColName.time_series_type: "production",
        ResultTableColName.batch_id: "batch_id",
        ResultTableColName.batch_process_type: "BalanceFixing",
        ResultTableColName.batch_execution_time_start: datetime(2020, 1, 1, 0, 0),
        ResultTableColName.from_grid_area: "843",
    }
    return spark.createDataFrame(data=[row], schema=results_schema)


@pytest.mark.parametrize(
    "column_name,invalid_column_value",
    [
        (ResultTableColName.batch_id, None),
        (ResultTableColName.batch_execution_time_start, None),
        (ResultTableColName.batch_process_type, None),
        (ResultTableColName.batch_process_type, "foo"),
        (ResultTableColName.time_series_type, None),
        (ResultTableColName.time_series_type, "foo"),
        (ResultTableColName.grid_area, None),
        (ResultTableColName.grid_area, "12"),
        (ResultTableColName.grid_area, "1234"),
        (ResultTableColName.from_grid_area, "12"),
        (ResultTableColName.from_grid_area, "1234"),
        (ResultTableColName.time, None),
        (ResultTableColName.quantity_quality, None),
        (ResultTableColName.quantity_quality, "foo"),
        (ResultTableColName.aggregation_level, None),
        (ResultTableColName.aggregation_level, "foo"),
    ],
)
def test__migrated_table_rejects_invalid_data(
    spark: SparkSession,
    column_name: str,
    invalid_column_value: str,
    migrations_executed: None,
) -> None:
    # Arrange
    results_df = _create_df(spark)
    invalid_df = results_df.withColumn(column_name, lit(invalid_column_value))

    # Act
    with pytest.raises(Exception) as ex:
        invalid_df.write.format("delta").option("mergeSchema", "false").insertInto(
            f"{DATABASE_NAME}.{TABLE_NAME}", overwrite=False
        )

    # Assert: Do sufficient assertions to be confident that the expected violation has been caught
    actual_error_message = str(ex.value)
    assert "DeltaInvariantViolationException" in actual_error_message
    assert column_name in actual_error_message


# According to SME there is no upper bounds limit from a business perspective.
# The chosen precision of 18 should however not cause any problems as the limit on time series
# is precision 6. Thus 1e9 time series points can be summed without any problem.
min_decimal = Decimal(f"-{'9'*15}.999")  # Precision=18 and scale=3
max_decimal = Decimal(f"{'9'*15}.999")  # Precision=18 and scale=3


@pytest.mark.parametrize(
    "column_name,column_value",
    [
        (ResultTableColName.batch_id, "some string"),
        (ResultTableColName.grid_area, "123"),
        (ResultTableColName.grid_area, "007"),
        (ResultTableColName.from_grid_area, None),
        (ResultTableColName.from_grid_area, "123"),
        (ResultTableColName.from_grid_area, "007"),
        (ResultTableColName.balance_responsible_id, None),
        (ResultTableColName.balance_responsible_id, "some string"),
        (ResultTableColName.energy_supplier_id, None),
        (ResultTableColName.energy_supplier_id, "some string"),
        (ResultTableColName.quantity, None),
        (ResultTableColName.quantity, Decimal("1.123")),
        (ResultTableColName.quantity, max_decimal),
        (ResultTableColName.quantity, -max_decimal),
    ],
)
def test__migrated_table_accepts_valid_data(
    spark: SparkSession,
    column_name: str,
    column_value: str,
    migrations_executed: None,
) -> None:
    # Arrange
    result_df = _create_df(spark)
    result_df = result_df.withColumn(column_name, lit(column_value))

    # Act and assert: Expectation is that no exception is raised
    result_df.write.format("delta").option("mergeSchema", "false").insertInto(
        f"{DATABASE_NAME}.{TABLE_NAME}"
    )


@pytest.mark.parametrize(
    "column_name,column_value",
    [
        *[(ResultTableColName.batch_process_type, x.value) for x in ProcessType],
        *[(ResultTableColName.time_series_type, x.value) for x in TimeSeriesType],
        *[(ResultTableColName.quantity_quality, x.value) for x in TimeSeriesQuality],
        *[(ResultTableColName.aggregation_level, x.value) for x in AggregationLevel],
    ],
)
def test__migrated_table_accepts_enum_value(
    spark: SparkSession,
    column_name: str,
    column_value: str,
    migrations_executed: None,
) -> None:
    "Test that all enum values are accepted by the delta table"

    # Arrange
    result_df = _create_df(spark)
    result_df = result_df.withColumn(column_name, lit(column_value))

    # Act and assert: Expectation is that no exception is raised
    result_df.write.format("delta").option("mergeSchema", "false").insertInto(
        f"{DATABASE_NAME}.{TABLE_NAME}"
    )


@pytest.mark.parametrize(
    "quantity",
    [
        min_decimal,
        max_decimal,
        Decimal("0.000"),
        Decimal("0.001"),
        Decimal("0.005"),
        Decimal("0.009"),
    ],
)
def test__migrated_table_does_not_round_valid_decimal(
    spark: SparkSession,
    quantity: Decimal,
    migrations_executed: None,
) -> None:
    # Arrange
    result_df = _create_df(spark)
    result_df = result_df.withColumn("quantity", lit(quantity))
    batch_id = str(uuid.uuid4())
    result_df = result_df.withColumn(ResultTableColName.batch_id, lit(batch_id))

    # Act
    result_df.write.format("delta").option("mergeSchema", "false").insertInto(
        f"{DATABASE_NAME}.{TABLE_NAME}"
    )

    # Assert
    actual_df = spark.read.table(f"{DATABASE_NAME}.{TABLE_NAME}").where(
        col(ResultTableColName.batch_id) == batch_id
    )
    assert actual_df.collect()[0].quantity == quantity


def test__result_table__is_not_managed(
    spark: SparkSession, migrations_executed: None
) -> None:
    """
    It is desired that the table is unmanaged to provide for greater flexibility.
    According to https://learn.microsoft.com/en-us/azure/databricks/lakehouse/data-objects#--what-is-a-database:
    "To manage data life cycle independently of database, save data to a location that is not nested under any database locations."
    Thus we check whether the table is managed by comparing its location to the location of the database/schema.
    """
    database_details = spark.sql(f"DESCRIBE DATABASE {DATABASE_NAME}")
    table_details = spark.sql(f"DESCRIBE DETAIL {DATABASE_NAME}.{TABLE_NAME}")

    database_location = database_details.where(
        col("info_name") == "Location"
    ).collect()[0]["info_value"]
    table_location = table_details.collect()[0]["location"]

    assert not table_location.startswith(database_location)


# def test__result_table__is_not_managed(data_lake_path: str) -> None:
#     # Arrange
#     spark = (
#         SparkSession.builder.config("spark.sql.streaming.schemaInference", True)
#         .config("spark.ui.showConsoleProgress", "false")
#         .config("spark.ui.enabled", "false")
#         .config("spark.ui.dagGraph.retainedRootRDDs", "1")
#         .config("spark.ui.retainedJobs", "1")
#         .config("spark.ui.retainedStages", "1")
#         .config("spark.ui.retainedTasks", "1")
#         .config("spark.sql.ui.retainedExecutions", "1")
#         .config("spark.worker.ui.retainedExecutors", "1")
#         .config("spark.worker.ui.retainedDrivers", "1")
#         .config("spark.default.parallelism", 1)
#         .config("spark.rdd.compress", False)
#         .config("spark.shuffle.compress", False)
#         .config("spark.shuffle.spill.compress", False)
#         .config("spark.sql.shuffle.partitions", 1)
#         .config("spark.sql.extensions", "io.delta.sql.DeltaSparkSessionExtension")
#         .config(
#             "spark.sql.catalog.spark_catalog",
#             "org.apache.spark.sql.delta.catalog.DeltaCatalog",
#         )
#         .getOrCreate()
#     )

#     path = f"{data_lake_path}/__test_result_table_is_not_managed__"

#     # Clean up to prevent problems from previous test runs
#     spark.sql(f"DROP DATABASE IF EXISTS {DATABASE_NAME} CASCADE")
#     shutil.rmtree(path, ignore_errors=True)

#     migration_args = MigrationScriptArgs(
#         data_storage_account_url="foo",
#         data_storage_account_name="foo",
#         data_storage_container_name="foo",
#         data_storage_credential="foo",
#         spark=spark,
#     )
#     migration_args.storage_container_path = path

#     # Execute all migrations
#     migrations = _get_all_migrations()
#     for name in migrations:
#         _apply_migration(name, migration_args)

#     # Act: Drop database, which will delete all managed tables
#     spark.sql(f"DROP DATABASE {DATABASE_NAME} CASCADE")

#     # Assert: The data files still exists after dropping the database implies that the table is not managed
#     assert os.path.exists(f"{path}/calculation-output/result/_delta_log")
