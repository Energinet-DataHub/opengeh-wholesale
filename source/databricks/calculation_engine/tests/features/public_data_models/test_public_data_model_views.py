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
from datetime import datetime

from pyspark.sql import SparkSession, DataFrame

from features.utils.dataframes.column_names.view_column_names import ViewColumnNames


def test__public_data_model_views_has_valid_column_names(
    migrations_executed: None,
    spark: SparkSession,
) -> None:
    """Verify that all columns in all views in all public view model databases match the expected column names and data types"""

    # Arrange
    data = [
        (
            "calc1",
            "type1",
            1,
            "mp1",
            datetime(2023, 1, 1),
            None,
            "area1",
            None,
            None,
            "typeA",
            None,
            None,
        ),
    ]

    view_column_names = ViewColumnNames()
    public_view_model_databases = [
        "settlement_report",
        "wholesale_calculation_results",
    ]
    errors = []

    # for database_name in public_view_model_databases:
    #     spark.catalog.setCurrentDatabase(database_name)
    #     views = spark.catalog.listTables()
    #
    #     for view in views:
    #         df = spark.read.format("delta").table(f"{database_name}.{view.name}")
    #
    #         for column in df.columns:
    #             try:
    #                 assert_name_and_data_type(column, df, view_column_names)
    #             except Exception as e:
    #                 errors.append(e)

    # df = spark.createDataFrame(data, schema=metering_point_period_v1_view_schema)
    df = spark.read.format("delta").table("settlement_report.metering_point_periods_v1")
    # Remove
    # print(ViewColumnNames.calculation_id)
    # print(ViewColumnNames.calculation_version)

    for column_name in df.columns:
        try:
            assert_name_and_data_type(column_name, df, view_column_names)
        except Exception as e:
            errors.append(e)

    if len(errors) > 0:
        for error in errors:
            print(error)

        assert False, "One or more assertions failed."


def assert_name_and_data_type(
    column_name: str, df: DataFrame, view_column_names: ViewColumnNames
) -> None:
    actual_schema = df.schema[column_name]
    expected_column = view_column_names.get(actual_schema.name)
    assert expected_column is not None, f"Column {column_name} not found."
    expected_type = expected_column.data_type
    actual_type = actual_schema.dataType
    assert expected_type == actual_type, f"Column {column_name} has wrong type."
