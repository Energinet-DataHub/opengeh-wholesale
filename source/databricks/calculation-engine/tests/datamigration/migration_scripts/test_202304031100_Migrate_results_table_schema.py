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

from decimal import Decimal
import importlib
import shutil
from unittest.mock import patch, Mock
from types import ModuleType
from pyspark.sql import SparkSession
from package.datamigration.migration_script_args import MigrationScriptArgs


TABLE_NAME = "test_202304031100_Migrate_results_table_schema"


@patch(
    "package.datamigration.migration_scripts.202304031100_Migrate_results_table_schema._get_delta_table_path"
)
def test__apply__updates_quantity_type(
    get_delta_table_path_mock: Mock, tests_temp_path: str, spark: SparkSession
) -> None:
    # Arrange
    sut = _get_migration_script()
    delta_table_path = f"{tests_temp_path}/{TABLE_NAME}"
    shutil.rmtree(delta_table_path, ignore_errors=True)
    _create_result_table(spark, delta_table_path)
    get_delta_table_path_mock.return_value = delta_table_path
    migration_args = MigrationScriptArgs("", "", "", spark)

    # Act
    sut.apply(migration_args)

    # Assert: Type must be decimal(18,3) and quantity value must be preserved as-well
    results_df = spark.read.format("delta").load(delta_table_path)

    assert results_df.select("quantity").dtypes[0][1] == "decimal(18,3)"
    assert results_df.first()["quantity"] == Decimal("1.234")


def _get_migration_script() -> ModuleType:
    return importlib.import_module(
        "package.datamigration.migration_scripts.202304031100_Migrate_results_table_schema"
    )


def _create_result_table(spark: SparkSession, delta_table_path: str) -> None:
    row = {"quantity": "1.234"}
    df = spark.createDataFrame(data=[row])
    df.write.format("delta").mode("overwrite").save(delta_table_path)
