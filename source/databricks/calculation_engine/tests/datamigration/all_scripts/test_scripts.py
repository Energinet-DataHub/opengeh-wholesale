﻿# Copyright 2020 Energinet DataHub A/S
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
    HiveOutputDatabase,
    InputDatabase,
    BasisDataDatabase,
    SettlementReportPublicDataModel,
    CalculationResultsPublicDataModel,
    OutputDatabase,
)
import package.datamigration.migration as sut
import package.datamigration.schema_config as schema_config
import tests.helpers.mock_helper as mock_helper
import tests.helpers.spark_sql_migration_helper as spark_sql_migration_helper


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

    mocker.patch.object(
        sut.env_vars,
        sut.env_vars.get_catalog_name.__name__,
        return_value="some_catalog",
    )

    # Act migration scripts
    migration_scripts_prefix = "migration_scripts"
    migration_scripts_substitutions = spark_sql_migration_helper.update_substitutions(
        spark_sql_migration_helper.get_migration_script_args(spark),
        {
            "{HIVE_OUTPUT_DATABASE_NAME}": f"{migration_scripts_prefix}{HiveOutputDatabase.DATABASE_NAME}",
            "{INPUT_DATABASE_NAME}": f"{migration_scripts_prefix}{InputDatabase.DATABASE_NAME}",
            "{BASIS_DATA_DATABASE_NAME}": f"{migration_scripts_prefix}{BasisDataDatabase.DATABASE_NAME}",
            "{CALCULATION_RESULTS_DATABASE_NAME}": f"{migration_scripts_prefix}{CalculationResultsPublicDataModel.DATABASE_NAME}",
            "{SETTLEMENT_REPORT_DATABASE_NAME}": f"{migration_scripts_prefix}{SettlementReportPublicDataModel.DATABASE_NAME}",
            "{OUTPUT_FOLDER}": f"{migration_scripts_prefix}migration_test",
            "{BASIS_DATA_FOLDER}": f"{migration_scripts_prefix}basis_folder",
            "{INPUT_FOLDER}": f"{migration_scripts_prefix}input_folder",
        },
    )
    spark_sql_migration_helper.configure_spark_sql_migration(
        spark,
        catalog_name="some_catalog",
        substitution_variables=migration_scripts_substitutions,
        table_prefix="migration_",
    )
    schema_migration_pipeline.migrate()

    # Act current state scripts
    current_state_prefix = "current_state"

    substitutions = spark_sql_migration_helper.update_substitutions(
        spark_sql_migration_helper.get_migration_script_args(spark),
        {
            "{HIVE_OUTPUT_DATABASE_NAME}": f"{current_state_prefix}{HiveOutputDatabase.DATABASE_NAME}",
            "{OUTPUT_DATABASE_NAME}": f"{current_state_prefix}{OutputDatabase.DATABASE_NAME}",
            "{INPUT_DATABASE_NAME}": f"{current_state_prefix}{InputDatabase.DATABASE_NAME}",
            "{BASIS_DATA_DATABASE_NAME}": f"{current_state_prefix}{BasisDataDatabase.DATABASE_NAME}",
            "{SETTLEMENT_REPORT_DATABASE_NAME}": f"{current_state_prefix}{SettlementReportPublicDataModel.DATABASE_NAME}",
            "{OUTPUT_FOLDER}": f"{current_state_prefix}migration_test",
            "{BASIS_DATA_FOLDER}": f"{current_state_prefix}basis_folder",
            "{INPUT_FOLDER}": f"{current_state_prefix}input_folder",
        },
    )
    spark_sql_migration_helper.configure_spark_sql_migration(
        spark,
        substitution_variables=substitutions,
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
