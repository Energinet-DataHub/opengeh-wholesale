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
import pytest
import shutil

from package.datamigration.migration import _apply_migration
from package.datamigration.uncommitted_migrations import _get_all_migrations
from package.datamigration.migration_script_args import MigrationScriptArgs
from package.file_writers.process_step_result_writer import DATABASE_NAME


@pytest.fixture(scope="session")
def integration_tests_path(calculation_engine_path: str) -> str:
    """
    Returns the integration tests folder path.
    Please note that this only works if current folder haven't been changed prior using `os.chdir()`.
    The correctness also relies on the prerequisite that this function is actually located in a
    file located directly in the integration tests folder.
    """
    return f"{calculation_engine_path}/tests/integration"


@pytest.fixture(scope="session")
def test_files_folder_path(integration_tests_path: str) -> str:
    return f"{integration_tests_path}/calculator/test_files"


@pytest.fixture(scope="session")
def data_lake_path(integration_tests_path: str) -> str:
    return f"{integration_tests_path}/__data_lake__"


@pytest.fixture(scope="session")
def migrations_executed(spark: SparkSession, integration_tests_path: str) -> None:
    # Clean up to prevent problems from previous test runs
    container_path = f"{integration_tests_path}/spark-warehouse/wholesale"
    shutil.rmtree(container_path, ignore_errors=True)
    spark.sql(f"DROP DATABASE IF EXISTS {DATABASE_NAME}")

    migration_args = MigrationScriptArgs(
        data_storage_account_url="foo",
        data_storage_account_name="foo",
        data_storage_container_name="foo",
        data_storage_credential="foo",
        spark=spark,
    )
    # Overwrite in test
    migration_args.storage_container_path = container_path

    # Execute all migrations
    migrations = _get_all_migrations()
    for name in migrations:
        _apply_migration(name, migration_args)
