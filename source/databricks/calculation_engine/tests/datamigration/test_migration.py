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

from pyspark.sql import SparkSession
from unittest.mock import Mock
import tests.helpers.spark_helper as spark_helper
import package.datamigration.schema_config as schema_config
import package.datamigration.migration as sut
import tests.helpers.mock_helper as mock_helper


def _diff(schema1, schema2):
    return {
        'fields_in_1_not_2': set(schema1) - set(schema2),
        'fields_in_2_not_1': set(schema2) - set(schema1)
    }


def test_migrate_with_schema_migration_scripts_compare_schemas(
    mocker: Mock, spark: SparkSession
) -> None:
    # Arrange
    mocker.patch.object(
        sut.paths,
        sut.paths.get_storage_account_url.__name__,
        side_effect=mock_helper.base_path_helper,
    )

    mocker.patch.object(
        sut.env_vars,
        sut.env_vars.get_storage_account_name.__name__,
        return_value="storage_account",
    )

    mocker.patch.object(
        sut.env_vars,
        sut.env_vars.get_calculation_input_folder_name.__name__,
        return_value="storage_account_2",
    )

    mocker.patch.object(
        sut.paths,
        sut.paths.get_container_url.__name__,
        return_value="storage_account",
    )

    spark_helper.reset_spark_catalog(spark)

    # Act
    sut.migrate_data_lake()

    # Assert
    for schema in schema_config.schema_config:
        for table in schema.tables:
            actual_table = spark.table(f"{schema.name}.{table.name}")
            diff = _diff(actual_table.schema, table.schema)
            assert actual_table.schema == table.schema


def test_migrate_with_schema_migration_scripts_compare_result_with_schema_config(
    mocker: Mock, spark: SparkSession
) -> None:
    """If this test fails, it indicates that a SQL script is creating something that the Schema Config does not know
    about"""
    # Arrange
    mocker.patch.object(
        sut.paths,
        sut.paths.get_storage_account_url.__name__,
        side_effect=mock_helper.base_path_helper,
    )

    mocker.patch.object(
        sut.env_vars,
        sut.env_vars.get_storage_account_name.__name__,
        return_value="storage_account",
    )

    mocker.patch.object(
        sut.env_vars,
        sut.env_vars.get_calculation_input_folder_name.__name__,
        return_value="storage_account",
    )

    mocker.patch.object(
        sut.paths,
        sut.paths.get_container_url.__name__,
        return_value="storage_account",
    )

    spark_helper.reset_spark_catalog(spark)

    # Act
    sut.migrate_data_lake()

    # Assert
    schemas = schema_config.schema_config
    actual_schemas = spark.catalog.listDatabases()
    for db in actual_schemas:
        if db.name == "default" or db.name == "schema_migration":
            continue

        schema = next((x for x in schemas if x.name == db.name), None)
        assert schema is not None, f"Schema {db.name} is not in the schema config"
        tables = spark.catalog.listTables(db.name)
        for table in tables:
            table_config = next((x for x in schema.tables if x.name == table.name), None)
            assert table_config is not None, f"Table {table.name} is not in the schema config"
            actual_table = spark.table(f"{db.name}.{table.name}")
            assert actual_table.schema == table_config.schema
