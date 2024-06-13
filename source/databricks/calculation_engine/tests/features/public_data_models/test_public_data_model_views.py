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
from typing import List

from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.catalog import Database

from features.utils.dataframes.columns.view_columns import ViewColumns


def test__public_data_model_views_have_registered_column_names_and_types(
    migrations_executed: None,
    spark: SparkSession,
) -> None:
    """Verify that all columns in all views in all public view models match the expected column names and data types"""

    # Arrange
    databases = get_view_databases(spark)
    errors = []

    # Act & Assert
    for database in databases:
        views = spark.catalog.listTables(database.name)
        assert views, f"No views found in database {database.name}."

        for view in views:
            df = spark.table(f"{database.name}.{view.name}")
            for column in df.columns:
                try:
                    assert_name_and_data_type(column, df)
                except Exception as e:
                    errors.append(f"{database.name}.{view.name}: {e}")

    assert not errors, "\n".join(errors) if errors else "All assertions passed."


def get_view_databases(spark: SparkSession) -> List[Database]:
    """
    Get all view databases.
    """
    negative_databases = {"default", "wholesale_output", "wholesale_input"}
    databases = [
        db for db in spark.catalog.listDatabases() if db.name not in negative_databases
    ]

    if not databases:
        raise ValueError("No databases found.")

    return databases


def assert_name_and_data_type(column_name: str, df: DataFrame) -> None:
    actual_schema = df.schema[column_name]
    expected_column = ViewColumns.get(actual_schema.name)
    assert expected_column is not None, f"Column {column_name} not found."
    expected_type = expected_column.data_type
    actual_type = actual_schema.dataType
    assert expected_type == actual_type, f"Column {column_name} has wrong type."
