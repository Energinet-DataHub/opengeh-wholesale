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
"""
By having a conftest.py in this directory, we are able to add all packages
defined in the geh_stream directory in our tests.
"""

from datetime import datetime

import yaml
from azure.identity import ClientSecretCredential
from delta import configure_spark_with_delta_pip
import os
from pyspark.sql import SparkSession
import pytest
import shutil
import subprocess
from typing import Generator, Callable, Optional

from package.datamigration.migration import _apply_migration
from package.datamigration.uncommitted_migrations import _get_all_migrations
from package.datamigration.migration_script_args import MigrationScriptArgs
from package.infrastructure.paths import OUTPUT_DATABASE_NAME

from tests.integration_test_configuration import IntegrationTestConfiguration


@pytest.fixture(scope="session")
def test_files_folder_path(tests_path: str) -> str:
    return f"{tests_path}/test_files"


@pytest.fixture(scope="session")
def spark() -> SparkSession:
    return configure_spark_with_delta_pip(  # see https://docs.delta.io/latest/quick-start.html#python
        SparkSession.builder.config("spark.sql.streaming.schemaInference", True)
        .config("spark.ui.showConsoleProgress", "false")
        .config("spark.ui.enabled", "false")
        .config("spark.ui.dagGraph.retainedRootRDDs", "1")
        .config("spark.ui.retainedJobs", "1")
        .config("spark.ui.retainedStages", "1")
        .config("spark.ui.retainedTasks", "1")
        .config("spark.sql.ui.retainedExecutions", "1")
        .config("spark.worker.ui.retainedExecutors", "1")
        .config("spark.worker.ui.retainedDrivers", "1")
        .config("spark.default.parallelism", 1)
        .config("spark.rdd.compress", False)
        .config("spark.shuffle.compress", False)
        .config("spark.shuffle.spill.compress", False)
        .config("spark.sql.shuffle.partitions", 1)
        .config("spark.sql.extensions", "io.delta.sql.DeltaSparkSessionExtension")
        .config(
            "spark.sql.catalog.spark_catalog",
            "org.apache.spark.sql.delta.catalog.DeltaCatalog",
        )
    ).getOrCreate()


@pytest.fixture(scope="session")
def file_path_finder() -> Callable[[str], str]:
    """
    Returns the path of the file.
    Please note that this only works if current folder haven't been changed prior using `os.chdir()`.
    The correctness also relies on the prerequisite that this function is actually located in a
    file located directly in the tests folder.
    """

    def finder(file: str) -> str:
        return os.path.dirname(os.path.normpath(file))

    return finder


@pytest.fixture(scope="session")
def source_path(file_path_finder: Callable[[str], str]) -> str:
    """
    Returns the <repo-root>/source folder path.
    Please note that this only works if current folder haven't been changed prior using `os.chdir()`.
    The correctness also relies on the prerequisite that this function is actually located in a
    file located directly in the tests folder.
    """
    return file_path_finder(f"{__file__}/../../..")


@pytest.fixture(scope="session")
def databricks_path(source_path: str) -> str:
    """
    Returns the source/databricks folder path.
    Please note that this only works if current folder haven't been changed prior using `os.chdir()`.
    The correctness also relies on the prerequisite that this function is actually located in a
    file located directly in the tests folder.
    """
    return f"{source_path}/databricks"


@pytest.fixture(scope="session")
def calculation_engine_path(databricks_path: str) -> str:
    """
    Returns the <repo-root>/source folder path.
    Please note that this only works if current folder haven't been changed prior using `os.chdir()`.
    The correctness also relies on the prerequisite that this function is actually located in a
    file located directly in the tests folder.
    """
    return f"{databricks_path}/calculation_engine"


@pytest.fixture(scope="session")
def contracts_path(calculation_engine_path: str) -> str:
    """
    Returns the source/contract folder path.
    Please note that this only works if current folder haven't been changed prior using `os.chdir()`.
    The correctness also relies on the prerequisite that this function is actually located in a
    file located directly in the tests folder.
    """
    return f"{calculation_engine_path}/contracts"


@pytest.fixture(scope="session")
def timestamp_factory() -> Callable[[str], Optional[datetime]]:
    """Creates timestamp from utc string in correct format yyyy-mm-ddThh:mm:ss.nnnZ"""

    def factory(date_time_string: str) -> Optional[datetime]:
        date_time_formatting_string = "%Y-%m-%dT%H:%M:%S.%fZ"
        if date_time_string is None:
            return None
        return datetime.strptime(date_time_string, date_time_formatting_string)

    return factory


@pytest.fixture(scope="session")
def tests_path(calculation_engine_path: str) -> str:
    """
    Returns the tests folder path.
    Please note that this only works if current folder haven't been changed prior using `os.chdir()`.
    The correctness also relies on the prerequisite that this function is actually located in a
    file located directly in the tests folder.
    """
    return f"{calculation_engine_path}/tests"


@pytest.fixture(scope="session")
def data_lake_path(tests_path: str, worker_id: str) -> str:
    return f"{tests_path}/__data_lake__/{worker_id}"


@pytest.fixture(scope="session")
def calculation_input_path(data_lake_path: str) -> str:
    return f"{data_lake_path}/calculation_input"


@pytest.fixture(scope="session")
def calculation_output_path(data_lake_path: str) -> str:
    return f"{data_lake_path}/calculation-output"


@pytest.fixture(scope="session")
def migrations_executed(spark: SparkSession, data_lake_path: str, calculation_output_path: str) -> None:
    # Clean up to prevent problems from previous test runs
    shutil.rmtree(calculation_output_path, ignore_errors=True)
    spark.sql(f"DROP DATABASE IF EXISTS {OUTPUT_DATABASE_NAME} CASCADE")

    migration_args = MigrationScriptArgs(
        data_storage_account_url="foo",
        data_storage_account_name="foo",
        data_storage_container_name="foo",
        data_storage_credential=ClientSecretCredential("foo", "foo", "foo"),
        spark=spark,
    )
    # Overwrite in test
    migration_args.storage_container_path = data_lake_path

    # Execute all migrations
    migrations = _get_all_migrations()
    for name in migrations:
        _apply_migration(name, migration_args)


@pytest.fixture(scope="session")
def virtual_environment() -> Generator:
    """Fixture ensuring execution in a virtual environment.
    Uses `virtualenv` instead of conda environments due to problems
    activating the virtual environment from pytest."""

    # Create and activate the virtual environment
    subprocess.call(["virtualenv", ".wholesale-pytest"])
    subprocess.call(
        "source .wholesale-pytest/bin/activate", shell=True, executable="/bin/bash"
    )

    yield None

    # Deactivate virtual environment upon test suite tear down
    subprocess.call("deactivate", shell=True, executable="/bin/bash")


@pytest.fixture(scope="session")
def installed_package(
    virtual_environment: Generator, calculation_engine_path: str
) -> None:
    """Ensures that the wholesale package is installed (after building it)."""

    # Build the package wheel
    os.chdir(calculation_engine_path)
    subprocess.call("python -m build --wheel", shell=True, executable="/bin/bash")

    # Uninstall the package in case it was left by a cancelled test suite
    subprocess.call(
        "pip uninstall -y package",
        shell=True,
        executable="/bin/bash",
    )

    # Intall wheel, which will also create console scripts for invoking
    # the entry points of the package
    subprocess.call(
        f"pip install {calculation_engine_path}/dist/package-1.0-py3-none-any.whl",
        shell=True,
        executable="/bin/bash",
    )


@pytest.fixture()
def integration_test_configuration(tests_path: str) -> IntegrationTestConfiguration:
    """Load settings and sets the properties as environment variables."""

    settings_file_path = f"{tests_path}/integrationtest.local.settings.yml"

    # Read settings from settings file if it exists
    if os.path.exists(settings_file_path):
        with open(settings_file_path) as stream:
            settings = yaml.safe_load(stream)
            azure_keyvault_url = settings["AZURE_KEYVAULT_URL"]
            return IntegrationTestConfiguration(azure_keyvault_url=azure_keyvault_url)

    # Otherwise, read settings from environment variables
    if "AZURE_KEYVAULT_URL" in os.environ:
        azure_keyvault_url = os.getenv("AZURE_KEYVAULT_URL")
        return IntegrationTestConfiguration(azure_keyvault_url=azure_keyvault_url)

    # If neither settings file nor environment variables are found, raise exception
    raise Exception(
        f"Settings file not found at {settings_file_path} and no environment variables found neither"
    )
