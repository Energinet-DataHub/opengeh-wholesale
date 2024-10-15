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
from typing import Callable, Generator

from delta import configure_spark_with_delta_pip
from pyspark.sql import SparkSession
from settlement_report_job.infrastructure.calculation_type import CalculationType
from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from tests.fixtures import DBUtilsFixture

from data_seeding import standard_wholesale_fixing_scenario_data_generator
from data_seeding.write_test_data import (
    write_metering_point_time_series_to_delta_table,
    write_charge_link_periods_to_delta_table,
    write_charge_price_information_periods_to_delta_table,
    write_energy_to_delta_table,
    write_energy_per_es_to_delta_table,
)


@pytest.fixture(scope="session")
def dbutils() -> DBUtilsFixture:
    """
    Returns a DBUtilsFixture instance that can be used to mock dbutils.
    """
    return DBUtilsFixture()


@pytest.fixture(scope="function")
def standard_wholesale_fixing_scenario_args(
    settlement_reports_output_path: str,
) -> SettlementReportArgs:
    return SettlementReportArgs(
        report_id=str(uuid.uuid4()),
        period_start=standard_wholesale_fixing_scenario_data_generator.FROM_DATE,
        period_end=standard_wholesale_fixing_scenario_data_generator.TO_DATE,
        calculation_type=CalculationType.WHOLESALE_FIXING,
        calculation_id_by_grid_area={
            standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[0]: uuid.UUID(
                standard_wholesale_fixing_scenario_data_generator.CALCULATION_ID
            ),
            standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[1]: uuid.UUID(
                standard_wholesale_fixing_scenario_data_generator.CALCULATION_ID
            ),
        },
        split_report_by_grid_area=True,
        prevent_large_text_files=False,
        time_zone="Europe/Copenhagen",
        catalog_name="spark_catalog",
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.SYSTEM_OPERATOR,  # using system operator since it is more complex (requires filter based on charge owner)
        requesting_actor_id=standard_wholesale_fixing_scenario_data_generator.CHARGE_OWNER_ID,
        settlement_reports_output_path=settlement_reports_output_path,
        include_basis_data=True,
        locale="da-dk",
    )


@pytest.fixture(scope="session")
def standard_wholesale_fixing_scenario_data_written_to_delta(
    spark: SparkSession,
    input_database_location: str,
) -> None:
    time_series_df = standard_wholesale_fixing_scenario_data_generator.create_metering_point_time_series(
        spark
    )
    write_metering_point_time_series_to_delta_table(
        spark, time_series_df, input_database_location
    )

    charge_link_periods_df = (
        standard_wholesale_fixing_scenario_data_generator.create_charge_link_periods(
            spark
        )
    )
    write_charge_link_periods_to_delta_table(
        spark, charge_link_periods_df, input_database_location
    )

    charge_price_information_periods_df = standard_wholesale_fixing_scenario_data_generator.create_charge_price_information_periods(
        spark
    )
    write_charge_price_information_periods_to_delta_table(
        spark, charge_price_information_periods_df, input_database_location
    )

    energy_df = standard_wholesale_fixing_scenario_data_generator.create_energy(
        spark, target_energy_per_es_v1=False
    )
    write_energy_to_delta_table(spark, energy_df, input_database_location)

    energy_per_es_df = standard_wholesale_fixing_scenario_data_generator.create_energy(
        spark, target_energy_per_es_v1=True
    )
    write_energy_per_es_to_delta_table(spark, energy_per_es_df, input_database_location)


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
def settlement_reports_output_path(data_lake_path: str) -> str:
    return f"{data_lake_path}/settlement_reports_output"


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


@pytest.fixture(autouse=True)
def configure_dummy_logging() -> None:
    """Ensure that logging hooks don't fail due to _TRACER_NAME not being set."""

    from settlement_report_job.logging.logging_configuration import configure_logging

    configure_logging(
        cloud_role_name="any-cloud-role-name", tracer_name="any-tracer-name"
    )
