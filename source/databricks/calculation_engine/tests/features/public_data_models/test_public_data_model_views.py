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

from pyspark.sql import SparkSession

from features.utils.dataframes.schemas.metering_point_period_v1_view_schema import (
    metering_point_period_v1_view_schema,
)
from features.utils.dataframes.view_column_names import ViewColumnNames


def test__public_data_model_views_has_valid_column_names(
    # migrations_executed: None,
    spark: SparkSession,
) -> None:

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

    df = spark.createDataFrame(data, schema=metering_point_period_v1_view_schema)
    # df = spark.read.format("delta").table("settlement_report.metering_point_periods_v1")
    errors = []
    view_column_names = ViewColumnNames()

    for column in df.columns:
        try:
            column_schema = df.schema[column]
            expected_column = view_column_names.get(column_schema.name)
            assert expected_column is not None, f"Column {column} not found."

            expected_type = expected_column["type"]
            actual_type = column_schema.dataType
            assert expected_type == actual_type, f"Column {column} has wrong type."

        except Exception as e:
            errors.append(e)

    if len(errors) > 0:
        for error in errors:
            print(error)

        assert False, "One or more assertions failed."
