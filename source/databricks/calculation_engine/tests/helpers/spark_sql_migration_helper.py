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

from pyspark.sql import SparkSession
from spark_sql_migrations import (
    SparkSqlMigrationsConfiguration,
    create_and_configure_container,
    schema_migration_pipeline,
)

import package.datamigration.constants as c
from package.datamigration.migration_script_args import MigrationScriptArgs
from package.datamigration.schema_config import schema_config
from package.datamigration.substitutions import substitutions

schema_migration_schema_name = "schema_migration"
schema_migration_location = "schema_migration"
schema_migration_table_name = "executed_migrations"


def migrate(
    spark: SparkSession,
    table_prefix: str = "",
    location: str = schema_migration_location,
    substitution_variables: dict[str, str] = None,
) -> None:
    configure_spark_sql_migration(spark, table_prefix, location, substitution_variables)
    schema_migration_pipeline.migrate()


def configure_spark_sql_migration(
    spark: SparkSession,
    table_prefix: str = "",
    location: str = schema_migration_location,
    substitution_variables: dict[str, str] = None,
) -> None:
    if substitution_variables is None:
        substitution_variables = update_substitutions(get_migration_script_args(spark))

    configuration = SparkSqlMigrationsConfiguration(
        migration_schema_name=schema_migration_schema_name,
        migration_schema_location=location,
        migration_table_name=schema_migration_table_name,
        migration_table_location=location,
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
    directory_path = f"{os.path.dirname(c.__file__)}/migration_scripts/"
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
    migration_args: MigrationScriptArgs, replacements: dict[str, str] = {}
) -> dict[str, str]:
    _substitutions = substitutions(migration_args)

    for key, value in replacements.items():
        _substitutions[key] = value

    return _substitutions
