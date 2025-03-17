from geh_common.migrations import (
    SparkSqlMigrationsConfiguration,
    schema_migration_pipeline,
)

import geh_wholesale.infrastructure.environment_variables as env_vars
from geh_wholesale.infrastructure import paths

from .substitutions import get_substitutions

MIGRATION_SCRIPTS_FOLDER_PATH = "package.datamigration.migration_scripts"


def migrate_data_lake(
    catalog_name: str | None = None,
    is_testing: bool = False,
) -> None:
    """Migration entry point for the Unity Catalog.

    This method must remain parameterless because it will be called from the entry point when deployed.
    Default parameter values are used to allow for testing with custom configurations.
    """
    catalog_name = catalog_name or env_vars.get_catalog_name()
    print(f"Executing Unity Catalog migrations for catalog {catalog_name}")  # noqa: T201

    spark_config = _create_spark_config(catalog_name, is_testing)
    schema_migration_pipeline.migrate(spark_config)


def _create_spark_config(catalog_name: str, is_testing: bool) -> SparkSqlMigrationsConfiguration:
    return SparkSqlMigrationsConfiguration(
        migration_schema_name=paths.WholesaleInternalDatabase.DATABASE_NAME,
        migration_table_name=paths.WholesaleInternalDatabase.EXECUTED_MIGRATIONS_TABLE_NAME,
        migration_scripts_folder_path=MIGRATION_SCRIPTS_FOLDER_PATH,
        substitution_variables=get_substitutions(catalog_name, is_testing),
        catalog_name=catalog_name,
    )
