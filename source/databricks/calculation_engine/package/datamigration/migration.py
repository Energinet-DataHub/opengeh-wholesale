from spark_sql_migrations import (
    create_and_configure_container,
    schema_migration_pipeline,
    SparkSqlMigrationsConfiguration,
)

import package.datamigration_hive.migration as hive_migration
import package.infrastructure.environment_variables as env_vars
from package.infrastructure import paths
from .schema_config import schema_config
from .substitutions import get_substitutions

MIGRATION_SCRIPTS_FOLDER_PATH = "package.datamigration.migration_scripts"
UNUSED = "UNUSED"  # Marks fields that were only used in the old hive implementation


def migrate_data_lake(
    catalog_name: str | None = None,
    spark_config_hive: SparkSqlMigrationsConfiguration = None,
    is_testing: bool = False,
) -> None:
    """
    This method must remain parameterless because it will be called from the entry point when deployed.
    Default parameter values are used to allow for testing with custom configurations.
    """

    catalog_name = catalog_name or env_vars.get_catalog_name()
    print(f"Executing Unity Catalog migrations for catalog {catalog_name}")

    spark_config = _create_spark_config(catalog_name, is_testing)

    create_and_configure_container(spark_config)
    schema_migration_pipeline.migrate()

    # Execute the hive migrations until they are finally removed
    # IMPORTANT: Do this _after_ the UC migrations as the hive migrations changes the
    #            dependency injection container registrations
    hive_migration.migrate_data_lake(spark_config_hive)


def _create_spark_config(
    catalog_name: str, is_testing: bool
) -> SparkSqlMigrationsConfiguration:
    return SparkSqlMigrationsConfiguration(
        migration_schema_name=paths.WholesaleInternalDatabase.DATABASE_NAME,
        migration_table_name=paths.WholesaleInternalDatabase.EXECUTED_MIGRATIONS_TABLE_NAME,
        migration_scripts_folder_path=MIGRATION_SCRIPTS_FOLDER_PATH,
        current_state_schemas_folder_path=None,
        current_state_tables_folder_path=None,
        current_state_views_folder_path=None,
        schema_config=schema_config,
        substitution_variables=get_substitutions(catalog_name, is_testing),
        catalog_name=catalog_name,
    )
