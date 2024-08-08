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
import logging
import os
import shutil
import subprocess
import uuid
from datetime import datetime
from pathlib import Path
from typing import Generator, Callable, Optional

import pytest
import yaml
from azure.identity import ClientSecretCredential
from delta import configure_spark_with_delta_pip
from pyspark.sql import SparkSession
from pyspark.sql.types import StructType

import tests.helpers.spark_sql_migration_helper as sql_migration_helper
from package.calculation.calculator_args import CalculatorArgs
from package.databases.migrations_wholesale.schemas import (
    time_series_points_schema,
    metering_point_periods_schema,
    charge_price_information_periods_schema,
    charge_price_points_schema,
    charge_link_periods_schema,
)
from package.codelists import CalculationType
from package.container import create_and_configure_container, Container
from package.databases.wholesale_internal.schemas import (
    grid_loss_metering_points_schema,
)
from package.infrastructure import paths
from package.infrastructure.infrastructure_settings import InfrastructureSettings
from tests.helpers.delta_table_utils import write_dataframe_to_table
from tests.integration_test_configuration import IntegrationTestConfiguration
from testsession_configuration import (
    TestSessionConfiguration,
)


@pytest.fixture(scope="session")
def test_files_folder_path(tests_path: str) -> str:
    return f"{tests_path}/test_files"


@pytest.fixture(scope="session")
def spark(
    test_session_configuration: TestSessionConfiguration,
    tests_path: str,
) -> SparkSession:
    warehouse_location = f"{tests_path}/__spark-warehouse__"
    metastore_path = f"{tests_path}/__metastore_db__"

    if (
        test_session_configuration.migrations.execute.value
        == sql_migration_helper.MigrationsExecution.ALL.value
    ):
        if os.path.exists(warehouse_location):
            print(f"Removing warehouse before clean run (path={warehouse_location})")
            shutil.rmtree(warehouse_location)
        if os.path.exists(metastore_path):
            print(f"Removing metastore before clean run (path={metastore_path})")
            shutil.rmtree(metastore_path)

    session = configure_spark_with_delta_pip(
        SparkSession.builder.config("spark.sql.warehouse.dir", warehouse_location)
        .config("spark.sql.streaming.schemaInference", True)
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
        .config("spark.driver.memory", "2g")
        .config("spark.executor.memory", "2g")
        .config("spark.rdd.compress", False)
        .config("spark.shuffle.compress", False)
        .config("spark.shuffle.spill.compress", False)
        .config("spark.sql.shuffle.partitions", 1)
        .config("spark.sql.extensions", "io.delta.sql.DeltaSparkSessionExtension")
        .config(
            "spark.sql.catalog.spark_catalog",
            "org.apache.spark.sql.delta.catalog.DeltaCatalog",
        )
        # Enable Hive support for persistence across test sessions
        .config("spark.sql.catalogImplementation", "hive")
        .config(
            "javax.jdo.option.ConnectionURL",
            f"jdbc:derby:;databaseName={metastore_path};create=true",
        )
        .config(
            "javax.jdo.option.ConnectionDriverName",
            "org.apache.derby.jdbc.EmbeddedDriver",
        )
        .config("javax.jdo.option.ConnectionUserName", "APP")
        .config("javax.jdo.option.ConnectionPassword", "mine")
        .config("datanucleus.autoCreateSchema", "true")
        .config("hive.metastore.schema.verification", "false")
        .config("hive.metastore.schema.verification.record.version", "false")
        .enableHiveSupport()
    ).getOrCreate()

    yield session
    session.stop()


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
def calculation_input_folder(data_lake_path: str) -> str:
    return "input"


@pytest.fixture(scope="session")
def calculation_input_database(data_lake_path: str) -> str:
    return "wholesale_input"


@pytest.fixture(scope="session")
def calculation_input_path(data_lake_path: str, calculation_input_folder: str) -> str:
    return f"{data_lake_path}/{calculation_input_folder}"


@pytest.fixture(scope="session")
def calculation_output_path(data_lake_path: str) -> str:
    return f"{data_lake_path}/{paths.HiveOutputDatabase.FOLDER_NAME}"


@pytest.fixture(scope="session")
def migrations_executed(
    spark: SparkSession,
    calculation_output_path: str,
    energy_input_data_written_to_delta: None,
    test_session_configuration: TestSessionConfiguration,
) -> None:
    # Execute all migrations
    sql_migration_helper.migrate(
        spark,
        migrations_execution=test_session_configuration.migrations.execute,
    )


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


@pytest.fixture(scope="session")
def test_session_configuration(tests_path: str) -> TestSessionConfiguration:
    settings_file_path = Path(tests_path) / "test.local.settings.yml"
    settings = _load_settings_from_file(settings_file_path)
    return TestSessionConfiguration(settings)


@pytest.fixture(scope="session")
def integration_test_configuration(tests_path: str) -> IntegrationTestConfiguration:
    """
    Load settings for integration tests either from a local YAML settings file or from environment variables.
    Proceeds even if certain Azure-related keys are not present in the settings file.
    """

    settings_file_path = Path(tests_path) / "integrationtest.local.settings.yml"

    def load_settings_from_env() -> dict:
        return {
            key: os.getenv(key)
            for key in [
                "AZURE_KEYVAULT_URL",
                "AZURE_CLIENT_ID",
                "AZURE_CLIENT_SECRET",
                "AZURE_TENANT_ID",
                "AZURE_SUBSCRIPTION_ID",
            ]
            if os.getenv(key) is not None
        }

    settings = _load_settings_from_file(settings_file_path) or load_settings_from_env()

    # Set environment variables from loaded settings
    for key, value in settings.items():
        if value is not None:
            os.environ[key] = value

    if "AZURE_KEYVAULT_URL" in settings:
        return IntegrationTestConfiguration(
            azure_keyvault_url=settings["AZURE_KEYVAULT_URL"]
        )

    logging.error(
        f"Integration test configuration could not be loaded from {settings_file_path} or environment variables."
    )
    raise Exception(
        "Failed to load integration test settings. Ensure that the Azure Key Vault URL is provided in the settings file or as an environment variable."
    )


def _load_settings_from_file(file_path: Path) -> dict:
    if file_path.exists():
        with file_path.open() as stream:
            return yaml.safe_load(stream)
    else:
        return {}


@pytest.fixture(scope="session")
def any_calculator_args() -> CalculatorArgs:
    return CalculatorArgs(
        calculation_id="foo",
        calculation_type=CalculationType.AGGREGATION,
        calculation_grid_areas=["805", "806"],
        calculation_period_start_datetime=datetime(2018, 1, 1, 23, 0, 0),
        calculation_period_end_datetime=datetime(2018, 1, 3, 23, 0, 0),
        calculation_execution_time_start=datetime(2018, 1, 5, 23, 0, 0),
        created_by_user_id=str(uuid.uuid4()),
        time_zone="Europe/Copenhagen",
        quarterly_resolution_transition_datetime=datetime(2023, 1, 31, 23, 0, 0),
        is_simulation=False,
    )


@pytest.fixture(scope="session")
def any_calculator_args_for_wholesale() -> CalculatorArgs:
    return CalculatorArgs(
        calculation_id="foo",
        calculation_type=CalculationType.WHOLESALE_FIXING,
        calculation_grid_areas=["805", "806"],
        calculation_period_start_datetime=datetime(2022, 6, 30, 22, 0, 0),
        calculation_period_end_datetime=datetime(2022, 7, 31, 22, 0, 0),
        calculation_execution_time_start=datetime(2022, 8, 1, 22, 0, 0),
        created_by_user_id=str(uuid.uuid4()),
        time_zone="Europe/Copenhagen",
        quarterly_resolution_transition_datetime=datetime(2023, 1, 31, 23, 0, 0),
        is_simulation=False,
    )


@pytest.fixture(scope="session")
def infrastructure_settings(
    data_lake_path: str, calculation_input_path: str
) -> InfrastructureSettings:
    return InfrastructureSettings(
        catalog_name="spark_catalog",
        calculation_input_database_name="wholesale_migrations_wholesale",
        data_storage_account_name="foo",
        data_storage_account_credentials=ClientSecretCredential("foo", "foo", "foo"),
        wholesale_container_path=data_lake_path,
        calculation_input_path=calculation_input_path,
        time_series_points_table_name=None,
        metering_point_periods_table_name=None,
        grid_loss_metering_points_table_name=None,
    )


@pytest.fixture(scope="session", autouse=True)
def dependency_injection_container(
    spark: SparkSession,
    infrastructure_settings: InfrastructureSettings,
) -> Container:
    """
    This enables the use of dependency injection in all tests.
    The container is created once for the entire test suite.
    """
    return create_and_configure_container(spark, infrastructure_settings)


@pytest.fixture(scope="session")
def energy_input_data_written_to_delta(
    spark: SparkSession,
    test_files_folder_path: str,
    calculation_input_path: str,
    test_session_configuration: TestSessionConfiguration,
    calculation_input_database: str,
) -> None:
    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/MeteringPointsPeriods.csv",
        database_name=calculation_input_database,
        table_name=paths.InputDatabase.METERING_POINT_PERIODS_TABLE_NAME,
        schema=metering_point_periods_schema,
        table_location=f"{calculation_input_path}/{paths.InputDatabase.METERING_POINT_PERIODS_TABLE_NAME}",
    )

    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/TimeSeriesPoints.csv",
        database_name=calculation_input_database,
        table_name=paths.InputDatabase.TIME_SERIES_POINTS_TABLE_NAME,
        schema=time_series_points_schema,
        table_location=f"{calculation_input_path}/{paths.InputDatabase.TIME_SERIES_POINTS_TABLE_NAME}",
    )

    # grid loss
    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/GridLossResponsible.csv",
        database_name=calculation_input_database,
        table_name=paths.WholesaleInternalDatabase.GRID_LOSS_METERING_POINTS_TABLE_NAME,
        schema=grid_loss_metering_points_schema,
        table_location=f"{calculation_input_path}/{paths.WholesaleInternalDatabase.GRID_LOSS_METERING_POINTS_TABLE_NAME}",
    )

    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/ChargePriceInformationPeriods.csv",
        database_name=calculation_input_database,
        table_name=paths.InputDatabase.CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME,
        schema=charge_price_information_periods_schema,
        table_location=f"{calculation_input_path}/{paths.InputDatabase.CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME}",
    )

    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/ChargeLinkPeriods.csv",
        database_name=calculation_input_database,
        table_name=paths.InputDatabase.CHARGE_LINK_PERIODS_TABLE_NAME,
        schema=charge_link_periods_schema,
        table_location=f"{calculation_input_path}/{paths.InputDatabase.CHARGE_LINK_PERIODS_TABLE_NAME}",
    )

    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/ChargePricePoints.csv",
        database_name=calculation_input_database,
        table_name=paths.InputDatabase.CHARGE_PRICE_POINTS_TABLE_NAME,
        schema=charge_price_points_schema,
        table_location=f"{calculation_input_path}/{paths.InputDatabase.CHARGE_PRICE_POINTS_TABLE_NAME}",
    )


@pytest.fixture(scope="session")
def price_input_data_written_to_delta(
    spark: SparkSession,
    test_files_folder_path: str,
    calculation_input_path: str,
    test_session_configuration: TestSessionConfiguration,
    calculation_input_database: str,
) -> None:
    # Charge master data periods
    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/ChargePriceInformationPeriods.csv",
        database_name=calculation_input_database,
        table_name=paths.InputDatabase.CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME,
        schema=charge_price_information_periods_schema,
        table_location=f"{calculation_input_path}/charge_price_information_periods",
    )

    # Charge link periods
    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/ChargeLinkPeriods.csv",
        database_name=calculation_input_database,
        table_name=paths.InputDatabase.CHARGE_LINK_PERIODS_TABLE_NAME,
        schema=charge_link_periods_schema,
        table_location=f"{calculation_input_path}/charge_link_periods",
    )

    # Charge price points
    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/ChargePricePoints.csv",
        database_name=calculation_input_database,
        table_name=paths.InputDatabase.CHARGE_PRICE_POINTS_TABLE_NAME,
        schema=charge_price_points_schema,
        table_location=f"{calculation_input_path}/charge_price_points",
    )


def _write_input_test_data_to_table(
    spark: SparkSession,
    file_name: str,
    database_name: str,
    table_name: str,
    table_location: str,
    schema: StructType,
) -> None:
    df = spark.read.csv(file_name, header=True, schema=schema)
    write_dataframe_to_table(
        spark,
        df,
        database_name,
        table_name,
        table_location,
        schema,
    )
