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

from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import array, col, lit
import pytest

from helpers.data_frame_utils import set_column
from package.calculation.preparation.grid_loss_responsible import (
    grid_area_responsible_schema,
)
from package.infrastructure.paths import (
    GRID_LOSS_METERING_POINTS_TABLE_NAME,
    INPUT_DATABASE_NAME,
)


def _create_df(spark: SparkSession) -> DataFrame:
    row = {
        "metering_point_id": "571313180400100657",
    }
    return spark.createDataFrame(data=[row], schema=grid_area_responsible_schema)


@pytest.mark.parametrize(
    "column_name,invalid_column_value",
    [
        ("metering_point_id", None),
        ("metering_point_id", "not-a-metering-point-id"),
    ],
)
def test__migrated_table_rejects_invalid_data(
    spark: SparkSession,
    column_name: str,
    invalid_column_value: str | list,
    migrations_executed: None,
) -> None:
    # Arrange
    results_df = _create_df(spark)

    if isinstance(invalid_column_value, list):
        invalid_df = results_df.withColumn(
            column_name, array(*map(lit, invalid_column_value))
        )
    else:
        invalid_df = results_df.withColumn(column_name, lit(invalid_column_value))

    # Act
    with pytest.raises(Exception) as ex:
        invalid_df.write.format("delta").option("mergeSchema", "false").insertInto(
            f"{INPUT_DATABASE_NAME}.{GRID_LOSS_METERING_POINTS_TABLE_NAME}",
            overwrite=False,
        )

    # Assert: Do sufficient assertions to be confident that the expected violation has been caught
    actual_error_message = str(ex.value)
    assert "DeltaInvariantViolationException" in actual_error_message
    assert column_name in actual_error_message


@pytest.mark.parametrize(
    "column_name,column_value",
    [
        ("metering_point_id", "571313180400100657"),
        ("metering_point_id", "250483500000000000"),
    ],
)
def test__migrated_table_accepts_valid_data(
    spark: SparkSession,
    column_name: str,
    column_value: str | list,
    migrations_executed: None,
) -> None:
    # Arrange
    result_df = _create_df(spark)
    result_df = set_column(result_df, column_name, column_value)

    # Act and assert: Expectation is that no exception is raised
    result_df.write.format("delta").option("mergeSchema", "false").insertInto(
        f"{INPUT_DATABASE_NAME}.{GRID_LOSS_METERING_POINTS_TABLE_NAME}"
    )


def test__table__is_not_managed(spark: SparkSession, migrations_executed: None) -> None:
    """
    It is desired that the table is unmanaged to provide for greater flexibility.
    According to https://learn.microsoft.com/en-us/azure/databricks/lakehouse/data-objects#--what-is-a-database:
    "To manage data life cycle independently of database, save data to a location that is not nested under any database locations."
    Thus we check whether the table is managed by comparing its location to the location of the database/schema.
    """
    database_details = spark.sql(f"DESCRIBE DATABASE {INPUT_DATABASE_NAME}")
    table_details = spark.sql(
        f"DESCRIBE DETAIL {INPUT_DATABASE_NAME}.{GRID_LOSS_METERING_POINTS_TABLE_NAME}"
    )

    database_location = database_details.where(
        col("info_name") == "Location"
    ).collect()[0]["info_value"]
    table_location = table_details.collect()[0]["location"]

    assert not table_location.startswith(database_location)
