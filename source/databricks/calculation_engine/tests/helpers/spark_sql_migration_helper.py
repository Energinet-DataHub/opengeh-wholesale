from pyspark.sql import SparkSession

import package.datamigration.constants as c

from spark_sql_migrations import (
    SparkSqlMigrationsConfiguration,
    create_and_configure_container,
    schema_migration_pipeline
)
from package.datamigration.substitutions import substitutions
from package.datamigration.schema_config import schema_config
from package.datamigration.migration_script_args import MigrationScriptArgs


def migrate(spark: SparkSession) -> None:
    migration_args = MigrationScriptArgs(
        data_storage_account_url="url",
        data_storage_account_name="data",
        calculation_input_folder="calculation_input",
        spark=spark,
        storage_container_path="container",
    )

    configuration = SparkSqlMigrationsConfiguration(
        migration_schema_name="schema_migration",
        migration_schema_location="schema_migration",
        migration_table_name="executed_migrations",
        migration_table_location="schema_migration",
        migration_scripts_folder_path=c.MIGRATION_SCRIPTS_FOLDER_PATH,
        current_state_schemas_folder_path=c.CURRENT_STATE_SCHEMAS_FOLDER_PATH,
        current_state_tables_folder_path=c.CURRENT_STATE_TABLES_FOLDER_PATH,
        schema_config=schema_config,
        substitution_variables=substitutions(migration_args),
    )

    create_and_configure_container(configuration)

    schema_migration_pipeline.migrate()
