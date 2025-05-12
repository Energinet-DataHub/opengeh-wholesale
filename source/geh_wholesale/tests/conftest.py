"""
By having a conftest.py in this directory, we are able to add all packages
defined in the geh_stream directory in our tests.
"""

import os
import shutil
import subprocess
import sys
import uuid
from datetime import datetime, timezone
from pathlib import Path
from typing import Callable, Generator, Optional
from unittest import mock

import geh_common.telemetry.logging_configuration as config
import pytest
import yaml
from geh_common.pyspark.read_csv import read_csv_path
from geh_common.testing.spark.spark_test_session import get_spark_test_session
from pyspark.sql import SparkSession
from pyspark.sql.types import StructType

import tests.helpers.spark_sql_migration_helper as sql_migration_helper
from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.codelists import CalculationType
from geh_wholesale.container import Container, create_and_configure_container
from geh_wholesale.databases.migrations_wholesale.schemas import (
    charge_link_periods_schema,
    charge_price_information_periods_schema,
    charge_price_points_schema,
    metering_point_periods_schema,
    time_series_points_schema,
)
from geh_wholesale.databases.wholesale_internal.schemas import (
    grid_loss_metering_point_ids_schema,
)
from geh_wholesale.infrastructure import paths
from geh_wholesale.infrastructure.environment_variables import EnvironmentVariable
from geh_wholesale.infrastructure.infrastructure_settings import InfrastructureSettings
from tests import PROJECT_PATH, SPARK_CATALOG_NAME
from tests.testsession_configuration import (
    TestSessionConfiguration,
)


def _load_settings_from_file(file_path: Path) -> dict:
    if file_path.exists():
        with file_path.open() as stream:
            return yaml.safe_load(stream)
    else:
        return {}


settings_file_path = Path(__file__).parent / "test.local.settings.yml"
settings = _load_settings_from_file(settings_file_path)
test_session_config = TestSessionConfiguration(settings)
static_data_dir = Path(__file__).parent / "__spark-warehouse__"


if test_session_config.migrations.execute.value == sql_migration_helper.MigrationsExecution.ALL.value:
    if static_data_dir.exists():
        shutil.rmtree(static_data_dir)
    static_data_dir.mkdir(parents=True, exist_ok=True)

_spark, datadir = get_spark_test_session(
    config_overrides={
        "spark.driver.memory": "4g",
        "spark.executor.memory": "4g",
    },
    static_data_dir=static_data_dir,
)


@pytest.fixture(scope="session")
def test_files_folder_path(tests_path: str) -> str:
    return f"{tests_path}/test_files"


@pytest.fixture(scope="session")
def spark() -> Generator[SparkSession, None, None]:
    yield _spark
    _spark.stop()


@pytest.fixture(scope="session")
def calculation_engine_path() -> str:
    """
    Returns the <repo-root>/source folder path.
    Please note that this only works if current folder haven't been changed prior using `os.chdir()`.
    The correctness also relies on the prerequisite that this function is actually located in a
    file located directly in the tests folder.
    """
    return f"{PROJECT_PATH}/src/geh_wholesale"


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
        return datetime.strptime(date_time_string, date_time_formatting_string).replace(tzinfo=timezone.utc)

    return factory


@pytest.fixture(scope="session")
def tests_path() -> str:
    """
    Returns the tests folder path.
    Please note that this only works if current folder haven't been changed prior using `os.chdir()`.
    The correctness also relies on the prerequisite that this function is actually located in a
    file located directly in the tests folder.
    """
    return f"{PROJECT_PATH}/tests"


@pytest.fixture(scope="session")
def data_lake_path(tests_path: str, worker_id: str) -> str:
    return f"{tests_path}/__data_lake__/{worker_id}"


@pytest.fixture(scope="session")
def calculation_input_folder(data_lake_path: str) -> str:
    return "input"


@pytest.fixture(scope="session")
def calculation_input_database() -> str:
    return paths.MigrationsWholesaleDatabase.DATABASE_NAME


@pytest.fixture(scope="session")
def migrations_executed(
    spark: SparkSession,
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
    subprocess.call(["uv", "venv", ".wholesale-pytest", "--no-project"], shell=True, executable="/bin/bash")
    subprocess.call("source .wholesale-pytest/bin/activate", shell=True, executable="/bin/bash")

    yield None

    # Deactivate virtual environment upon test suite tear down
    subprocess.call("deactivate", shell=True, executable="/bin/bash")


@pytest.fixture(scope="session")
def installed_package(virtual_environment: Generator, calculation_engine_path: str) -> None:
    """Ensures that the wholesale package is installed (after building it)."""

    # Build the package wheel
    os.chdir(calculation_engine_path)
    subprocess.call("uv run python -m build --wheel", shell=True, executable="/bin/bash")

    # Uninstall the package in case it was left by a cancelled test suite
    subprocess.call(
        "uv pip uninstall geh_wholesale",
        shell=True,
        executable="/bin/bash",
    )

    # Intall wheel, which will also create console scripts for invoking
    # the entry points of the package
    subprocess.call(
        f"uv pip install {calculation_engine_path}/dist/geh_wholesale-1.0-py3-none-any.whl",
        shell=True,
        executable="/bin/bash",
    )


@pytest.fixture(scope="session")
def test_session_configuration(tests_path: str) -> TestSessionConfiguration:
    return test_session_config


@pytest.fixture
def any_calculator_args(monkeypatch: pytest.MonkeyPatch) -> CalculatorArgs:
    monkeypatch.setattr(
        sys,
        "argv",
        [
            CalculatorArgs.model_config.get("cli_prog_name", "calculator"),
            "--calculation-id=foo",
            f"--calculation-type={CalculationType.AGGREGATION.value}",
            "--grid-areas=[805,806]",
            "--period-start-datetime=2018-01-01T23:00:00Z",
            "--period-end-datetime=2018-01-03T23:00:00Z",
            f"--created-by-user-id={uuid.uuid4()}",
        ],
    )
    monkeypatch.setenv("TIME_ZONE", "Europe/Copenhagen")
    monkeypatch.setenv("QUARTERLY_RESOLUTION_TRANSITION_DATETIME", "2023-01-31T23:00:00Z")
    return CalculatorArgs()


@pytest.fixture
def infrastructure_settings(monkeypatch: pytest.MonkeyPatch) -> InfrastructureSettings:
    monkeypatch.setattr(
        os,
        "environ",
        {
            EnvironmentVariable.CATALOG_NAME.value: SPARK_CATALOG_NAME,
            EnvironmentVariable.CALCULATION_INPUT_DATABASE_NAME.value: "wholesale_migrations_wholesale",
            EnvironmentVariable.DATA_STORAGE_ACCOUNT_NAME.value: "foo",
            EnvironmentVariable.TENANT_ID.value: "tenant_id",
            EnvironmentVariable.SPN_APP_ID.value: "spn_app_id",
            EnvironmentVariable.SPN_APP_SECRET.value: "spn_app_secret",
            EnvironmentVariable.CALCULATION_INPUT_FOLDER_NAME.value: "calculation_input_folder",
        },
    )
    return InfrastructureSettings()


@pytest.fixture(scope="session", autouse=True)
def dependency_injection_container(spark: SparkSession) -> Container:
    """
    This enables the use of dependency injection in all tests.
    The container is created once for the entire test suite.
    """
    with pytest.MonkeyPatch.context() as mp:
        mp.setattr(
            os,
            "environ",
            {
                EnvironmentVariable.CATALOG_NAME.value: "spark_catalog",
                EnvironmentVariable.CALCULATION_INPUT_DATABASE_NAME.value: "wholesale_migrations_wholesale",
                EnvironmentVariable.DATA_STORAGE_ACCOUNT_NAME.value: "foo",
                EnvironmentVariable.TENANT_ID.value: "tenant_id",
                EnvironmentVariable.SPN_APP_ID.value: "spn_app_id",
                EnvironmentVariable.SPN_APP_SECRET.value: "spn_app_secret",
                EnvironmentVariable.CALCULATION_INPUT_FOLDER_NAME.value: "calculation_input_folder",
            },
        )
        infrastructure_settings = InfrastructureSettings()
    return create_and_configure_container(spark, infrastructure_settings)


@pytest.fixture(scope="session", autouse=True)
def configure_logging_dummy() -> config.LoggingSettings:
    """
    Configures the logging initially.
    """
    orchestration_instance_id = uuid.uuid4()
    sys_args = ["program_name", "--orchestration-instance-id", str(orchestration_instance_id)]
    subsystem = "unit-tests"
    cloud_role_name = "dbr-calculation-engine-tests"
    with (
        mock.patch.dict(os.environ, {"APPLICATIONINSIGHTS_CONNECTION_STRING": "connectionString"}),
        mock.patch.object(sys, "argv", sys_args),
        mock.patch("geh_common.telemetry.logging_configuration.configure_azure_monitor", mock.Mock()),
    ):
        logging_settings = config.configure_logging(
            subsystem=subsystem,
            cloud_role_name=cloud_role_name,
        )

        return logging_settings


@pytest.fixture(scope="session")
def grid_loss_metering_point_ids_input_data_written_to_delta(
    spark: SparkSession,
    test_files_folder_path: str,
    test_session_configuration: TestSessionConfiguration,
    migrations_executed: None,
) -> None:
    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/GridLossMeteringPointIds.csv",
        database_name=paths.WholesaleInternalDatabase.DATABASE_NAME,
        table_name=paths.WholesaleInternalDatabase.GRID_LOSS_METERING_POINT_IDS_TABLE_NAME,
        schema=grid_loss_metering_point_ids_schema,
    )


@pytest.fixture(scope="session")
def energy_input_data_written_to_delta(
    spark: SparkSession,
    test_files_folder_path: str,
    test_session_configuration: TestSessionConfiguration,
    calculation_input_database: str,
) -> None:
    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/MeteringPointsPeriods.csv",
        database_name=calculation_input_database,
        table_name=paths.MigrationsWholesaleDatabase.METERING_POINT_PERIODS_TABLE_NAME,
        schema=metering_point_periods_schema,
    )

    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/TimeSeriesPoints.csv",
        database_name=calculation_input_database,
        table_name=paths.MigrationsWholesaleDatabase.TIME_SERIES_POINTS_TABLE_NAME,
        schema=time_series_points_schema,
    )

    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/ChargePriceInformationPeriods.csv",
        database_name=calculation_input_database,
        table_name=paths.MigrationsWholesaleDatabase.CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME,
        schema=charge_price_information_periods_schema,
    )

    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/ChargeLinkPeriods.csv",
        database_name=calculation_input_database,
        table_name=paths.MigrationsWholesaleDatabase.CHARGE_LINK_PERIODS_TABLE_NAME,
        schema=charge_link_periods_schema,
    )

    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/ChargePricePoints.csv",
        database_name=calculation_input_database,
        table_name=paths.MigrationsWholesaleDatabase.CHARGE_PRICE_POINTS_TABLE_NAME,
        schema=charge_price_points_schema,
    )


@pytest.fixture(scope="session")
def price_input_data_written_to_delta(
    spark: SparkSession,
    test_files_folder_path: str,
    test_session_configuration: TestSessionConfiguration,
    calculation_input_database: str,
) -> None:
    # Charge master data periods
    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/ChargePriceInformationPeriods.csv",
        database_name=calculation_input_database,
        table_name=paths.MigrationsWholesaleDatabase.CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME,
        schema=charge_price_information_periods_schema,
    )

    # Charge link periods
    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/ChargeLinkPeriods.csv",
        database_name=calculation_input_database,
        table_name=paths.MigrationsWholesaleDatabase.CHARGE_LINK_PERIODS_TABLE_NAME,
        schema=charge_link_periods_schema,
    )

    # Charge price points
    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/ChargePricePoints.csv",
        database_name=calculation_input_database,
        table_name=paths.MigrationsWholesaleDatabase.CHARGE_PRICE_POINTS_TABLE_NAME,
        schema=charge_price_points_schema,
    )


def _write_input_test_data_to_table(
    spark: SparkSession,
    file_name: str,
    database_name: str,
    table_name: str,
    schema: StructType,
) -> None:
    df = read_csv_path(spark, file_name, schema=schema, sep=";")
    fqn = f"{SPARK_CATALOG_NAME}.{database_name}.{table_name}"
    spark.sql(f"CREATE DATABASE IF NOT EXISTS {SPARK_CATALOG_NAME}.{database_name}")
    df.write.saveAsTable(fqn, format="delta", mode="overwrite")
    spark.sql(f"ALTER TABLE {fqn} CLUSTER BY ({schema.fieldNames()[0]})")
    spark.sql(f"ALTER TABLE {fqn} SET TBLPROPERTIES (delta.deletedFileRetentionDuration = 'interval 30 days')")
