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

from contracts.test_contracts_utils import get_database_objects
from package.infrastructure import paths
from package.infrastructure.paths import UnityCatalogDatabaseNames


def test__when_migrations_executed__all_databases_are_created(
    spark: SparkSession, migrations_executed: None
) -> None:

    # Arrange
    expected = UnityCatalogDatabaseNames.get_names()
    actual = [db.name for db in spark.catalog.listDatabases("wholesale_*")]
    errors = []

    # Act
    for database_name in expected:

        try:
            assert database_name in actual

        except Exception as e:
            errors.append(f"{database_name}: {e}")

    # Assert
    assert len(expected) == len(actual), "Number of databases do not match."
    assert not errors, "\n".join(errors) if errors else "All assertions passed."


def test_all_tables_and_views_created_after_migrations(
    spark: SparkSession, migrations_executed: None
) -> None:
    """
    Validates that all expected tables and views are created in the workspace after migrations are executed.
    """
    # Arrange
    expected = get_database_objects(paths)
    errors: list = []

    # Helper: Retrieve the names of tables or views from the catalog
    def get_names_by_type(objects, *obj_types):
        return [obj.name for obj in objects if obj.tableType in obj_types]

    # Act: Validate each expected database and its objects
    for db_name, expected_objects in expected.items():

        # Retrieve actual tables and views from the database
        actual_objects = spark.catalog.listTables(db_name)
        actual_tables = get_names_by_type(actual_objects, "MANAGED", "TABLE")
        actual_views = get_names_by_type(actual_objects, "VIEW")

        _validate_the_number_of_tables(actual_tables, db_name, errors, expected_objects)
        _validate_each_table_exists(actual_tables, db_name, errors, expected_objects)

        _validate_the_number_of_views(actual_views, db_name, errors, expected_objects)
        _validate_each_view_exists(actual_views, db_name, errors, expected_objects)

    # Assert
    if errors:
        raise AssertionError("\n".join(errors))


def _validate_the_number_of_tables(
    actual_tables, db_name, errors, expected_objects
) -> None:
    try:
        assert len(expected_objects["tables"]) == len(actual_tables), (
            f"Database {db_name}: Expected {len(expected_objects['tables'])} tables, "
            f"but found {len(actual_tables)}."
        )
    except AssertionError as e:
        errors.append(str(e))


def _validate_each_table_exists(
    actual_tables, db_name, errors, expected_objects
) -> None:
    for table_name in expected_objects["tables"]:
        try:
            assert (
                table_name in actual_tables
            ), f"Table {table_name} is missing in {db_name}."
        except AssertionError as e:
            errors.append(str(e))


def _validate_the_number_of_views(
    actual_views, db_name, errors, expected_objects
) -> None:
    try:
        assert len(expected_objects["views"]) == len(actual_views), (
            f"Database {db_name}: Expected {len(expected_objects['views'])} views, "
            f"but found {len(actual_views)}."
        )
    except AssertionError as e:
        errors.append(str(e))


def _validate_each_view_exists(actual_views, db_name, errors, expected_objects) -> None:
    for view_name in expected_objects["views"]:
        try:
            assert (
                view_name in actual_views
            ), f"View {view_name} is missing in {db_name}."
        except AssertionError as e:
            errors.append(str(e))
