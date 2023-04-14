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


MIGRATION_SCRIPT_NAME = "202304130900_Add_result_table_constraints"


@pytest.mark.parametrize(
    "column_name,invalid_column_value",
    [
        ("grid_area", None),
        ("grid_area", "12"),
        ("grid_area", "1234"),
        ("out_grid_area", None),  # TODO: Should not fail
        ("out_grid_area", "12"),
        ("out_grid_area", "1234"),
        ("batch_process_type", "foo"),
        ("time_series_type", "foo"),
        ("quantity_quality", "foo"),
        ("aggregation_level", "foo"),
    ],
)
def test__apply__enforces_data_constraints(
    tests_temp_path: str,
    spark: SparkSession,
    column_name: str,
    invalid_column_value: str,
) -> None:
    # Arrange
    sut = _get_migration_script()
    path = f"{tests_temp_path}/{MIGRATION_SCRIPT_NAME}"
    shutil.rmtree(path, ignore_errors=True)  # Clean up after previous runs
    delta_table_path = f"{path}/calculation-output/result"
    _create_result_table(spark, delta_table_path)  # Create table to be migrated
    migration_args = MigrationScriptArgs("", "", "", spark)

    # Act
    sut.apply(migration_args)

    # Assert: Column `out_grid_area` has been added as a string column with null values for existing rows
    results_df = spark.read.format("delta").load(delta_table_path)

    invalid_df = results_df.withColumn(column_name, lit(invalid_column_value))
    with pytest.raises(Exception) as ex:
        invalid_df.write.format("delta").mode("overwrite").save(delta_table_path)

    assert str(ex) == "foo"


def _get_migration_script() -> ModuleType:
    return importlib.import_module(
        f"package.datamigration.migration_scripts.{MIGRATION_SCRIPT_NAME}"
    )


def _create_result_table(spark: SparkSession, delta_table_path: str) -> None:
    row = {
        "grid_area": "543",
        "out_grid_area": "543",
        "batch_process_type": "BalanceFixing",
        "time_series_type": "production",
        "quantity_quality": "missing",
        "aggregation_level": "total_ga",
    }
    df = spark.createDataFrame(data=[row])
    df.write.format("delta").mode("overwrite").save(delta_table_path)
