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


from spark_sql_migrations import (
    create_and_configure_container,
    schema_migration_pipeline,
    SparkSqlMigrationsConfiguration,
)

import package.datamigration.constants as c
import package.infrastructure.environment_variables as env_vars
from package.infrastructure import paths, initialize_spark
from .migration_script_args import MigrationScriptArgs
from .schema_config import schema_config
from .substitutions import substitutions


# This method must remain parameterless because it will be called from the entry point when deployed.
def migrate_data_lake() -> None:
    storage_account_name = env_vars.get_storage_account_name()
    calculation_input_folder = env_vars.get_calculation_input_folder_name()

    spark = initialize_spark()

    storage_account_url = paths.get_storage_account_url(
        storage_account_name,
    )

    spark_container_url = paths.get_spark_sql_migrations_path(storage_account_name)
    container_url = paths.get_container_root_path(storage_account_name)

    migration_args = MigrationScriptArgs(
        data_storage_account_url=storage_account_url,
        data_storage_account_name=storage_account_name,
        schema_migration_storage_container_path=spark_container_url,
        storage_container_path=container_url,
        spark=spark,
        calculation_input_folder=calculation_input_folder,
    )

    spark_config = SparkSqlMigrationsConfiguration(
        migration_schema_name="schema_migration",
        migration_schema_location=migration_args.schema_migration_storage_container_path,
        migration_table_name="executed_migrations",
        migration_table_location=migration_args.schema_migration_storage_container_path,
        migration_scripts_folder_path=c.MIGRATION_SCRIPTS_FOLDER_PATH,
        current_state_schemas_folder_path=c.CURRENT_STATE_SCHEMAS_FOLDER_PATH,
        current_state_tables_folder_path=c.CURRENT_STATE_TABLES_FOLDER_PATH,
        schema_config=schema_config,
        substitution_variables=substitutions(migration_args),
    )

    create_and_configure_container(spark_config)
    schema_migration_pipeline.migrate()
