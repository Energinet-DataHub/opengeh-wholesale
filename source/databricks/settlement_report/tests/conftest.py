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
import uuid
import pytest
from datetime import datetime
from typing import Callable, Generator

from delta import configure_spark_with_delta_pip
from pyspark.sql import SparkSession
from pyspark.sql.types import StructType

from settlement_report_job.domain.calculation_type import CalculationType
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from tests.fixtures import DBUtilsFixture

from settlement_report_job.infrastructure import database_definitions
from settlement_report_job.infrastructure.schemas.metering_point_time_series_v1 import (
    metering_point_time_series_v1,
)


@pytest.fixture(scope="session")
def dbutils() -> DBUtilsFixture:
    """
    Returns a DBUtilsFixture instance that can be used to mock dbutils.
    """
    return DBUtilsFixture()


@pytest.fixture(scope="session")
def any_settlement_report_args() -> SettlementReportArgs:
    return SettlementReportArgs(
        report_id=str(uuid.uuid4()),
        period_start=datetime(2024, 6, 30, 22, 0, 0),
        period_end=datetime(2024, 7, 31, 22, 0, 0),
        calculation_type=CalculationType.WHOLESALE_FIXING,
        calculation_id_by_grid_area={
            "016": uuid.UUID("32e49805-20ef-4db2-ac84-c4455de7a373")
        },
        split_report_by_grid_area=True,
        prevent_large_text_files=False,
        time_zone="Europe/Copenhagen",
        catalog_name="spark_catalog",
        energy_supplier_id="1234567890123",
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id="1111111111111",
    )


@pytest.fixture(scope="session")
def metering_point_time_series_written_to_delta_table(
    spark: SparkSession, input_database_location: str, test_files_folder_path: str
) -> None:
    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/metering_point_time_series_v1.csv",
        database_name=database_definitions.WholesaleWholesaleResultsDatabase.DATABASE_NAME,
        table_name=database_definitions.WholesaleSettlementReportDatabase.METERING_POINT_TIME_SERIES_VIEW_NAME,
        table_location=f"{input_database_location}/{database_definitions.WholesaleSettlementReportDatabase.METERING_POINT_TIME_SERIES_VIEW_NAME}",
        schema=metering_point_time_series_v1,
    )


def _write_input_test_data_to_table(
    spark: SparkSession,
    file_name: str,
    database_name: str,
    table_name: str,
    table_location: str,
    schema: StructType,
    mode: str = "overwrite",
) -> None:
    df = spark.read.csv(file_name, header=True, schema=schema)

    spark.sql(f"CREATE DATABASE IF NOT EXISTS {database_name}")

    sql_schema = _struct_type_to_sql_schema(schema)
    spark.sql(
        f"CREATE TABLE IF NOT EXISTS {database_name}.{table_name} ({sql_schema}) USING DELTA LOCATION '{table_location}'"
    )
    df.show()
    df.write.format("delta").mode(mode).saveAsTable(f"{database_name}.{table_name}")


def _struct_type_to_sql_schema(schema: StructType) -> str:
    schema_string = ""
    for field in schema.fields:
        field_name = field.name
        field_type = field.dataType.simpleString()

        if not field.nullable:
            field_type += " NOT NULL"

        schema_string += f"{field_name} {field_type}, "

    # Remove the trailing comma and space
    schema_string = schema_string.rstrip(", ")
    return schema_string


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
    Returns the source/databricks/ folder path.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """
    return f"{databricks_path}/settlement_report"


@pytest.fixture(scope="session")
def contracts_path(settlement_report_path: str) -> str:
    """
    Returns the source/contract folder path.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """
    return f"{settlement_report_path}/contracts"


@pytest.fixture(scope="session")
def test_files_folder_path(tests_path: str) -> str:
    return f"{tests_path}/test_files"


@pytest.fixture(scope="session")
def input_database_location(data_lake_path: str) -> str:
    return f"{data_lake_path}/input_database"


@pytest.fixture(scope="session")
def data_lake_path(tests_path: str, worker_id: str) -> str:
    return f"{tests_path}/__data_lake__/{worker_id}"


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
def settlement_report_job_container_path(databricks_path: str) -> str:
    """
    Returns the <repo-root>/source folder path.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """
    return f"{databricks_path}/settlement_report"


@pytest.fixture(scope="session")
def spark(
    tests_path: str,
) -> Generator[SparkSession, None, None]:
    warehouse_location = f"{tests_path}/__spark-warehouse__"

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
    ).getOrCreate()

    yield session
    session.stop()
