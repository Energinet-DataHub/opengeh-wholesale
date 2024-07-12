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
from pyspark.sql.functions import col
import pytest

from helpers.data_frame_utils import set_column

from package.databases.migrations_wholesale.schemas import (
    grid_loss_metering_points_schema,
)
from package.infrastructure import paths


def _create_df(spark: SparkSession) -> DataFrame:
    row = {
        "metering_point_id": "571313180400100657",
    }
    return spark.createDataFrame(data=[row], schema=grid_loss_metering_points_schema)


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

    invalid_df = set_column(results_df, column_name, invalid_column_value)

    # Act
    with pytest.raises(Exception) as ex:
        invalid_df.write.format("delta").option("mergeSchema", "false").insertInto(
            f"{paths.WholesaleInternalDatabase.DATABASE_NAME}.{paths.WholesaleInternalDatabase.GRID_LOSS_METERING_POINTS_TABLE_NAME}",
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
        f"{paths.WholesaleInternalDatabase.DATABASE_NAME}.{paths.WholesaleInternalDatabase.GRID_LOSS_METERING_POINTS_TABLE_NAME}"
    )
