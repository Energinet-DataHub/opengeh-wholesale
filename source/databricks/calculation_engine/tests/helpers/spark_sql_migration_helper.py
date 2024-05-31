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
import shutil
from datetime import datetime
from enum import Enum
from pathlib import Path

from pyspark.sql import SparkSession
import pyspark.sql.functions as f
from spark_sql_migrations import (
    SparkSqlMigrationsConfiguration,
    create_and_configure_container,
    schema_migration_pipeline,
)
from spark_sql_migrations.utility import delta_table_helper

import package.datamigration.constants as c
from package.datamigration.migration_script_args import MigrationScriptArgs
from package.datamigration.schema_config import schema_config
from package.datamigration.substitutions import substitutions

schema_migration_schema_name = "schema_migration"
schema_migration_location = "schema_migration"
schema_migration_table_name = "executed_migrations"


class MigrationsExecution(Enum):
    """
    Configure the execution of migrations.
    The purpose is to allow the developer to determine what migrations should be executed to
    improve development speed and avoid unnecessary execution of migrations.
    """

    NONE = 0
    """Do not execute any migrations."""
    ALL = 1
    """Execute all migrations. This is similar to the CI behavior."""
    MODIFIED = 2
    """Execute only the migrations that have been modified since the last execution."""


def migrate(
    spark: SparkSession,
    substitution_variables: dict[str, str] | None = None,
    migrations_execution: MigrationsExecution = MigrationsExecution.ALL,
) -> None:
    print(
        f"Preparing execution of migrations with execution type: {migrations_execution}"
    )

    if migrations_execution.value == MigrationsExecution.NONE.value:
        print("Skipping migrations as MigrationsExecution is set to NONE")
        return

    if migrations_execution.value == MigrationsExecution.MODIFIED.value:
        _remove_registration_of_modified_scripts(spark, migrations_execution)

    if migrations_execution.value == MigrationsExecution.ALL.value:
        warehouse_location = spark.conf.get("spark.sql.warehouse.dir")
        if warehouse_location.startswith("file:"):
            warehouse_location = warehouse_location[5:]
        if os.path.exists(warehouse_location):
            print(f"Removing warehouse before clean run (path={warehouse_location})")
            shutil.rmtree(warehouse_location)

    configure_spark_sql_migration(spark, substitution_variables)
    schema_migration_pipeline.migrate()


def _remove_registration_of_modified_scripts(
    spark: SparkSession, migrations_execution: MigrationsExecution
) -> None:
    migrations_table = f"{schema_migration_schema_name}.{schema_migration_table_name}"
    if not delta_table_helper.delta_table_exists(
        spark, schema_migration_schema_name, schema_migration_table_name
    ):
        print(
            f"Table {migrations_table} does not exist. Skipping removal of modified scripts"
        )
        return

    latest_execution_time = (
        spark.sql(f"SELECT * FROM {migrations_table}")
        .agg(f.max("execution_datetime").alias("latest_execution_time"))
        .collect()[0]["latest_execution_time"]
    )
    modified_scripts = _get_recently_modified_migration_scripts(
        _get_migration_scripts_path(), latest_execution_time
    )
    if not modified_scripts:
        return

    if migrations_execution.value == MigrationsExecution.MODIFIED.value:
        in_clause = "'" + "', '".join(modified_scripts) + "'"
        sql = f"DELETE FROM {migrations_table} WHERE migration_name in ({in_clause})"
        spark.sql(sql)


def configure_spark_sql_migration(
    spark: SparkSession,
    table_prefix: str = "",
    substitution_variables: dict[str, str] | None = None,
) -> None:
    if substitution_variables is None:
        substitution_variables = update_substitutions(get_migration_script_args(spark))

    configuration = SparkSqlMigrationsConfiguration(
        migration_schema_name=schema_migration_schema_name,
        migration_schema_location="",
        migration_table_name=schema_migration_table_name,
        migration_table_location="",
        migration_scripts_folder_path=c.MIGRATION_SCRIPTS_FOLDER_PATH,
        current_state_schemas_folder_path=c.CURRENT_STATE_SCHEMAS_FOLDER_PATH,
        current_state_tables_folder_path=c.CURRENT_STATE_TABLES_FOLDER_PATH,
        current_state_views_folder_path=c.CURRENT_STATE_VIEWS_FOLDER_PATH,
        schema_config=schema_config,
        substitution_variables=substitution_variables,
        table_prefix=table_prefix,
    )

    create_and_configure_container(configuration)


def get_migration_script_args(spark: SparkSession) -> MigrationScriptArgs:
    return MigrationScriptArgs(
        data_storage_account_url="url",
        data_storage_account_name="data",
        calculation_input_folder="calculation_input",
        spark=spark,
        storage_container_path="container",
        schema_migration_storage_container_path="container",
    )


def migrate_with_current_state(spark: SparkSession) -> None:
    spark.sql(f"CREATE DATABASE IF NOT EXISTS {schema_migration_schema_name}")
    spark.sql(
        f"CREATE TABLE IF NOT EXISTS {schema_migration_schema_name}.{schema_migration_table_name} (migration_name STRING NOT NULL, execution_datetime TIMESTAMP NOT NULL) USING delta LOCATION '{schema_migration_location}/{schema_migration_table_name}'"
    )

    # Get all SQL files from migration_scripts folder
    directory_path = _get_migration_scripts_path()
    files = [
        file
        for file in os.listdir(directory_path)
        if os.path.isfile(os.path.join(directory_path, file))
    ]

    # Filter only SQL files
    sql_files = [file for file in files if file.endswith(".sql")]
    for sql_file in sql_files:
        spark.sql(
            f"INSERT INTO {schema_migration_schema_name}.{schema_migration_table_name} (migration_name, execution_datetime) VALUES ('{sql_file}', '2021-01-01')"
        )


def update_substitutions(
    migration_args: MigrationScriptArgs, replacements: dict[str, str] | None = None
) -> dict[str, str]:
    replacements = replacements or {}
    _substitutions = substitutions(migration_args)

    for key, value in replacements.items():
        _substitutions[key] = value

    return _substitutions


def _get_migration_scripts_path() -> str:
    return f"{os.path.dirname(c.__file__)}/migration_scripts/"


def _get_recently_modified_migration_scripts(
    root_folder: str, reference_datetime: datetime
) -> list[str]:
    recent_files = []

    # Traverse the folder and its subfolders
    for subdir, _, files in os.walk(root_folder):
        for file in files:
            if file.endswith(".sql"):
                file_path = os.path.join(subdir, file)
                # Get the modification time of the file
                modification_time = datetime.fromtimestamp(os.path.getmtime(file_path))

                # Compare with the reference datetime
                if modification_time > reference_datetime:
                    script_name_without_extension = Path(file_path).stem
                    recent_files.append(script_name_without_extension)

    return recent_files
