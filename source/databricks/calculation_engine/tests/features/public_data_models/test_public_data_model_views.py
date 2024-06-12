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

from features.utils.dataframes.column_names.view_columns import ViewColumns


@pytest.mark.parametrize(
    "database_name",
    [
        "settlement_report",
        "wholesale_calculation_results",
    ],
)
def test__public_data_model_views_has_valid_column_names_and_types(
    migrations_executed: None,
    spark: SparkSession,
    database_name: str,
) -> None:
    """Verify that all columns in all views in all public view models match the expected column names and data types"""

    view_columns = ViewColumns()
    errors = []
    spark.catalog.setCurrentDatabase(database_name)
    views = spark.catalog.listTables()

    for view in views:
        df = spark.read.format("delta").table(f"{database_name}.{view.name}")

        for column in df.columns:
            try:
                assert_name_and_data_type(column, df, view_columns)
            except Exception as e:
                errors.append(f"{view.name}: {e}")

    assert not errors, "\n".join(errors) if errors else "All assertions passed."


def assert_name_and_data_type(
    column_name: str, df: DataFrame, view_column_names: ViewColumns
) -> None:
    actual_schema = df.schema[column_name]
    expected_column = view_column_names.get(actual_schema.name)
    assert expected_column is not None, f"Column {column_name} not found."
    expected_type = expected_column.data_type
    actual_type = actual_schema.dataType
    assert expected_type == actual_type, f"Column {column_name} has wrong type."
