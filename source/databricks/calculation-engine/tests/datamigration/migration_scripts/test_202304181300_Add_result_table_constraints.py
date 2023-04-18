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

import importlib
import shutil
from types import ModuleType
from pyspark.sql import SparkSession
from pyspark.sql.functions import lit
import pytest
from package.datamigration.migration_script_args import MigrationScriptArgs


MIGRATION_SCRIPT_NAME = "202304181300_Add_result_table_constraints"
TABLE_NAME = "result"


@pytest.mark.parametrize(
    "column_name,invalid_column_value",
    [
        ("grid_area", None),
        ("grid_area", "12"),
        ("grid_area", "1234"),
        ("out_grid_area", "12"),
        ("out_grid_area", "1234"),
        ("batch_process_type", "foo"),
        ("time_series_type", "foo"),
        ("quantity_quality", "foo"),
        ("aggregation_level", "foo"),
    ],
)
def test__apply__enforces_data_constraints(
    tests_path: str,
    spark: SparkSession,
    column_name: str,
    invalid_column_value: str,
) -> None:
    # Arrange
    sut = _get_migration_script()
    _create_result_table(spark, tests_path)  # Create table to be migrated
    migration_args = MigrationScriptArgs("", "", "", spark)

    # Act
    sut.apply(migration_args)

    # Assert:
    results_df = spark.read.table(TABLE_NAME)

    invalid_df = results_df.withColumn(column_name, lit(invalid_column_value))
    with pytest.raises(Exception) as ex:
        invalid_df.write.format("delta").mode("overwrite").saveAsTable(TABLE_NAME)

    actual_error_message = str(ex.value)

    # Do sufficient assertions to be confident that the expected violation has been caught
    assert "DeltaInvariantViolationException" in actual_error_message
    assert column_name in actual_error_message


def _get_migration_script() -> ModuleType:
    return importlib.import_module(
        f"package.datamigration.migration_scripts.{MIGRATION_SCRIPT_NAME}"
    )


def _create_result_table(spark: SparkSession, tests_path: str) -> None:
    spark.sql(f"DROP TABLE IF EXISTS {TABLE_NAME}")
    shutil.rmtree(
        f"{tests_path}/datamigration/migration_scripts/spark-warehouse/{TABLE_NAME}",
        ignore_errors=True,
    )
    row = {
        "grid_area": "543",
        "out_grid_area": "543",
        "batch_process_type": "BalanceFixing",
        "time_series_type": "production",
        "quantity_quality": "missing",
        "aggregation_level": "total_ga",
    }
    df = spark.createDataFrame(data=[row])
    df.write.format("delta").saveAsTable(TABLE_NAME)
