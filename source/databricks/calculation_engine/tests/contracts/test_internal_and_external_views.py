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

import pytest
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import IntegerType

from contracts.databases_and_schemas import (
    get_expected_schemas,
    get_views_from_database,
)
from features.utils.dataframes.columns.view_columns import ViewColumns
from package.common import assert_schema


@pytest.mark.parametrize(
    "database_name",
    [
        "wholesale_internal",
        "wholesale_results",
        "wholesale_settlement_reports",
        "wholesale_sap",
        "wholesale_basis_data",
    ],
)
def test__views_have_the_expected_column_names_and_types(
    migrations_executed: None,
    spark: SparkSession,
    database_name: str,
) -> None:
    """Verify that all columns in views have the expected column name and data type."""

    # Arrange
    views = get_views_from_database(database_name, spark)
    errors = []

    # Act
    assert views, f"No views found in database {database_name}."

    for view in views:
        try:
            df = spark.table(f"{database_name}.{view.name}")
            for column in df.columns:
                # Assert
                _assert_name_and_data_type(column, df)
        except Exception as e:
            errors.append(f"{database_name}.{view.name}: {e}")

    assert not errors, "\n".join(errors) if errors else "All assertions passed."


@pytest.mark.parametrize(
    "folder",
    [
        "wholesale_internal",
        "data_products/wholesale_results",
        "data_products/wholesale_settlement_reports",
        "data_products/wholesale_sap",
        "data_products/wholesale_basis_data",
    ],
)
def test__views_have_the_expected_schemas(
    migrations_executed: None,
    spark: SparkSession,
    folder: str,
) -> None:
    """Verify that wholesale internal and public data views have the expected schemas."""

    # Arrange
    expected_schemas = get_expected_schemas(folder)
    if not expected_schemas:
        raise ValueError(f"No expected schemas found in folder {folder}.")

    database_name = folder.split("/")[-1]
    views = get_views_from_database(database_name, spark)
    errors = []

    # Act
    for view in views:
        view_identifier = f"{database_name}.{view.name}"
        try:
            if view_identifier not in expected_schemas:
                raise ValueError(f"Expected schema for {view_identifier} not found.")

            actual_df = spark.table(view_identifier)
            expected_df = spark.createDataFrame([], expected_schemas[view_identifier])

            # Assert
            assert_schema(
                actual_df.schema,
                expected_df.schema,
                ignore_nullability=True,
            )
        except Exception as e:
            errors.append(f"{view_identifier}: {e}")

    assert not errors, "\n".join(errors) if errors else "All assertions passed."


def _assert_name_and_data_type(column_name: str, df: DataFrame) -> None:
    actual_schema = df.schema[column_name]
    expected_column = ViewColumns.get(actual_schema.name)
    assert (
        expected_column is not None
    ), f"Column '{column_name}' is missing in class `ViewColumns`."
    expected_type = expected_column.data_type
    actual_type = actual_schema.dataType

    # Because quantity can be a DecimalType or IntegerType depending on context the following check is necessary.
    if column_name == "quantity":
        if expected_type == actual_type or actual_type == IntegerType():
            return

    assert (
        expected_type == actual_type
    ), f"Column '{column_name}' has wrong type. Is '{actual_type}', expected '{expected_type}'."
