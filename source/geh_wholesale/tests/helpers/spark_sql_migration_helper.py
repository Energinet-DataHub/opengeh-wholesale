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
from datetime import datetime
from enum import Enum
from pathlib import Path

import pyspark.sql.functions as f
from geh_common.migrations.utility import delta_table_helper
from pyspark.sql import SparkSession

from geh_wholesale.datamigration.migration import migrate_data_lake
from geh_wholesale.infrastructure.paths import UnityCatalogDatabaseNames

catalog_name = "spark_catalog"
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


def _create_databases(spark: SparkSession) -> None:
    """
    Create Unity Catalog databases as they are not created by migration scripts.
    They are created by infrastructure (in the real environments)
    In tests they are created in the single available default database.
    """

    spark.sql(f"CREATE DATABASE IF NOT EXISTS {schema_migration_schema_name}")

    for database in UnityCatalogDatabaseNames().get_names():
        print(f"Creating database {database}")  # noqa: T201
        spark.sql(f"CREATE DATABASE IF NOT EXISTS {database}")


def migrate(
    spark: SparkSession,
    migrations_execution: MigrationsExecution = MigrationsExecution.ALL,
) -> None:
    print(f"Preparing execution of migrations with execution type: {migrations_execution}")  # noqa: T201

    if migrations_execution.value == MigrationsExecution.NONE.value:
        print("Skipping migrations as MigrationsExecution is set to NONE")  # noqa: T201
        return

    if migrations_execution.value == MigrationsExecution.MODIFIED.value:
        _remove_registration_of_modified_scripts(spark, migrations_execution)

    _create_databases(spark)

    migrate_data_lake(catalog_name, is_testing=True)


def _remove_registration_of_modified_scripts(spark: SparkSession, migrations_execution: MigrationsExecution) -> None:
    migrations_table = f"{schema_migration_schema_name}.{schema_migration_table_name}"
    if not delta_table_helper.delta_table_exists(
        spark, catalog_name, schema_migration_schema_name, schema_migration_table_name
    ):
        print(f"Table {migrations_table} does not exist. Skipping removal of modified scripts")  # noqa: T201
        return

    latest_execution_time = (
        spark.sql(f"SELECT * FROM {migrations_table}")
        .agg(f.max("execution_datetime").alias("latest_execution_time"))
        .collect()[0]["latest_execution_time"]
    )
    modified_scripts = _get_recently_modified_migration_scripts(_get_migration_scripts_path(), latest_execution_time)
    if not modified_scripts:
        return

    if migrations_execution.value == MigrationsExecution.MODIFIED.value:
        in_clause = "'" + "', '".join(modified_scripts) + "'"
        sql = f"DELETE FROM {migrations_table} WHERE migration_name in ({in_clause})"
        spark.sql(sql)


def _get_migration_scripts_path() -> str:
    return f"{os.path.dirname(__file__)}/migration_scripts/"


def _get_recently_modified_migration_scripts(root_folder: str, reference_datetime: datetime) -> list[str]:
    recent_files = []

    # Traverse the folder and its sub-folders
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
