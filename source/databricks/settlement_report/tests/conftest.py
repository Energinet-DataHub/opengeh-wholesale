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
import os
import shutil
import uuid
import pytest
import yaml
from pathlib import Path
from datetime import datetime
from typing import Callable, Generator

from delta import configure_spark_with_delta_pip
from pyspark.sql import SparkSession

from settlement_report_job.calculation_type import CalculationType
from settlement_report_job.settlement_report_args import SettlementReportArgs

from tests.testsession_configuration import (
    TestSessionConfiguration,
    MigrationsExecution,
)


@pytest.fixture(scope="session")
def any_settlement_report_args() -> SettlementReportArgs:
    return SettlementReportArgs(
        report_id=str(uuid.uuid4()),
        period_start=datetime(2018, 3, 31, 22, 0, 0),
        period_end=datetime(2018, 4, 30, 22, 0, 0),
        calculation_type=CalculationType.WHOLESALE_FIXING,
        split_report_by_grid_area=True,
        prevent_large_text_files=False,
        time_zone="Europe/Copenhagen",
        catalog_name="catalog_name",
    )


def _load_settings_from_file(file_path: Path) -> dict:
    if file_path.exists():
        with file_path.open() as stream:
            return yaml.safe_load(stream)
    else:
        return {}


@pytest.fixture(scope="session")
def file_path_finder() -> Callable[[str], str]:
    """
    Returns the path of the file.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """

    def finder(file: str) -> str:
        return os.path.dirname(os.path.normpath(file))

    return finder


@pytest.fixture(scope="session")
def source_path(file_path_finder: Callable[[str], str]) -> str:
    """
    Returns the <repo-root>/source folder path.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """
    return file_path_finder(f"{__file__}/../../..")


@pytest.fixture(scope="session")
def databricks_path(source_path: str) -> str:
    """
    Returns the source/databricks folder path.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """
    return f"{source_path}/databricks"


@pytest.fixture(scope="session")
def settlement_report_path(databricks_path: str) -> str:
    """
    Returns the <repo-root>/source folder path.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """
    return f"{databricks_path}/settlement_report"


@pytest.fixture(scope="session")
def tests_path(settlement_report_path: str) -> str:
    """
    Returns the tests folder path.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """
    return f"{settlement_report_path}/tests"


@pytest.fixture(scope="session")
def test_session_configuration(tests_path: str) -> TestSessionConfiguration:
    settings_file_path = Path(tests_path) / "test.local.settings.yml"
    settings = _load_settings_from_file(settings_file_path)
    return TestSessionConfiguration(settings)


@pytest.fixture(scope="session")
def spark(
    test_session_configuration: TestSessionConfiguration,
    tests_path: str,
) -> Generator[SparkSession, None, None]:
    warehouse_location = f"{tests_path}/__spark-warehouse__"
    metastore_path = f"{tests_path}/__metastore_db__"

    if (
        test_session_configuration.migrations.execute.value
        == MigrationsExecution.ALL.value
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
