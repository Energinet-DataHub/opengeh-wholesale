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
from unittest.mock import Mock

import spark_sql_migrations.schema_migration_pipeline as schema_migration_pipeline
from pyspark.sql import SparkSession
from package.infrastructure.paths import (
    OUTPUT_DATABASE_NAME,
    INPUT_DATABASE_NAME,
    BASIS_DATA_DATABASE_NAME,
    SETTLEMENT_REPORT_DATABASE_NAME,
    EdiResults,
)
import package.datamigration.migration as sut
import package.datamigration.schema_config as schema_config
import tests.helpers.mock_helper as mock_helper
import tests.helpers.spark_sql_migration_helper as spark_sql_migration_helper


def test__schema_config__when_current_state_script_files_are_executed(
    mocker: Mock, spark: SparkSession
) -> None:
    # Arrange
    storage_account = "storage_account_4"
    mocker.patch.object(
        sut.paths,
        sut.paths.get_storage_account_url.__name__,
        side_effect=mock_helper.base_path_helper,
    )

    mocker.patch.object(
        sut.env_vars,
        sut.env_vars.get_storage_account_name.__name__,
        return_value=storage_account,
    )

    mocker.patch.object(
        sut.env_vars,
        sut.env_vars.get_calculation_input_folder_name.__name__,
        return_value=storage_account,
    )

    mocker.patch.object(
        sut.paths,
        sut.paths.get_spark_sql_migrations_path.__name__,
        return_value=storage_account,
    )

    mocker.patch.object(
        sut.paths,
        sut.paths.get_container_root_path.__name__,
        return_value=storage_account,
    )

    spark_helper.reset_spark_catalog(spark)
    spark_sql_migration_helper.migrate_with_current_state(spark)

    # Act
    sut.migrate_data_lake()

    # Assert
    schemas = spark.catalog.listDatabases()
    for schema in schema_config.schema_config:
        assert schema.name in [schema.name for schema in schemas]
        for table in schema.tables:
            actual_table = spark.table(f"{schema.name}.{table.name}")
            assert actual_table.schema == table.schema


def test__current_state_and_migration_scripts__should_give_same_result(
    mocker: Mock, spark: SparkSession
) -> None:
    # Arrange
    storage_account = "storage_account_5"
    mocker.patch.object(
        sut.paths,
        sut.paths.get_storage_account_url.__name__,
        side_effect=mock_helper.base_path_helper,
    )

    mocker.patch.object(
        sut.env_vars,
        sut.env_vars.get_storage_account_name.__name__,
        return_value=storage_account,
    )

    mocker.patch.object(
        sut.env_vars,
        sut.env_vars.get_calculation_input_folder_name.__name__,
        return_value=storage_account,
    )

    mocker.patch.object(
        sut.paths,
        sut.paths.get_spark_sql_migrations_path.__name__,
        return_value=storage_account,
    )

    mocker.patch.object(
        sut.paths,
        sut.paths.get_container_root_path.__name__,
        return_value=storage_account,
    )

    # Act migration scripts
    migration_scripts_prefix = "migration_scripts"
    migration_scripts_substitutions = spark_sql_migration_helper.update_substitutions(
        spark_sql_migration_helper.get_migration_script_args(spark),
        {
            "{OUTPUT_DATABASE_NAME}": f"{migration_scripts_prefix}{OUTPUT_DATABASE_NAME}",
            "{INPUT_DATABASE_NAME}": f"{migration_scripts_prefix}{INPUT_DATABASE_NAME}",
            "{BASIS_DATA_DATABASE_NAME}": f"{migration_scripts_prefix}{BASIS_DATA_DATABASE_NAME}",
            "{EDI_RESULTS_DATABASE_NAME}": f"{migration_scripts_prefix}{EdiResults.DATABASE_NAME}",
            "{SETTLEMENT_REPORT_DATABASE_NAME}": f"{migration_scripts_prefix}{SETTLEMENT_REPORT_DATABASE_NAME}",
            "{OUTPUT_FOLDER}": f"{migration_scripts_prefix}migration_test",
            "{BASIS_DATA_FOLDER}": f"{migration_scripts_prefix}basis_folder",
            "{INPUT_FOLDER}": f"{migration_scripts_prefix}input_folder",
        },
    )
    spark_sql_migration_helper.configure_spark_sql_migration(
        spark,
        substitution_variables=migration_scripts_substitutions,
        location="migration_test",
        table_prefix="migration_",
    )
    schema_migration_pipeline.migrate()

    # Act current state scripts
    current_state_prefix = "current_state"

    substitutions = spark_sql_migration_helper.update_substitutions(
        spark_sql_migration_helper.get_migration_script_args(spark),
        {
            "{OUTPUT_DATABASE_NAME}": f"{current_state_prefix}{OUTPUT_DATABASE_NAME}",
            "{INPUT_DATABASE_NAME}": f"{current_state_prefix}{INPUT_DATABASE_NAME}",
            "{BASIS_DATA_DATABASE_NAME}": f"{current_state_prefix}{BASIS_DATA_DATABASE_NAME}",
            "{SETTLEMENT_REPORT_DATABASE_NAME}": f"{current_state_prefix}{SETTLEMENT_REPORT_DATABASE_NAME}",
            "{OUTPUT_FOLDER}": f"{current_state_prefix}migration_test",
            "{BASIS_DATA_FOLDER}": f"{current_state_prefix}basis_folder",
            "{INPUT_FOLDER}": f"{current_state_prefix}input_folder",
        },
    )
    spark_sql_migration_helper.configure_spark_sql_migration(
        spark,
        substitution_variables=substitutions,
        location="migration_test",
        table_prefix="migration_",
    )
    schema_migration_pipeline._migrate(0)

    # Clean up DI
    spark_sql_migration_helper.configure_spark_sql_migration(spark)

    # Assert
    migration_databases = spark.catalog.listDatabases()
    assert len(migration_databases) > 0

    for db in migration_databases:
        table = spark.catalog.listTables(db.name)
        assert table is not None

    for schema in schema_config.schema_config:
        for table in schema.tables:
            migration_script_table_name = (
                f"{migration_scripts_prefix}{schema.name}.{table.name}"
            )
            current_state_script_tag = (
                f"{current_state_prefix}{schema.name}.{table.name}"
            )

            migration_script_table_df = spark.table(migration_script_table_name)
            current_state_table_df = spark.table(current_state_script_tag)
            assert migration_script_table_df.schema == current_state_table_df.schema

            # Assert properties and location
            migration_script_details = spark.sql(
                f"DESCRIBE DETAIL {migration_script_table_name}"
            ).collect()[0]
            current_state_details = spark.sql(
                f"DESCRIBE DETAIL {current_state_script_tag}"
            ).collect()[0]

            # Remove delta.columnMapping properties
            migration_script_details_props = {
                k: v
                for k, v in migration_script_details["properties"].items()
                if not k.startswith("delta.columnMapping.maxColumnId")
            }
            current_state_details_props = {
                k: v
                for k, v in current_state_details["properties"].items()
                if not k.startswith("delta.columnMapping.maxColumnId")
            }

            migrations_script_location = migration_script_details["location"].replace(
                migration_scripts_prefix, ""
            )
            current_state_location = current_state_details["location"].replace(
                current_state_prefix, ""
            )
            assert (
                migrations_script_location == current_state_location
            ), f"{migration_script_table_name} and {current_state_script_tag} have different locations"

            assert (
                migration_script_details_props == current_state_details_props
            ), f"{migration_script_table_name} and {current_state_script_tag} have different properties"
