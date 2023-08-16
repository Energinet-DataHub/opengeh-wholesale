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

from azure.identity import ClientSecretCredential
from datetime import datetime
from pyspark.sql import SparkSession, DataFrame
import pyspark.sql.functions as F
import pytest
from unittest.mock import patch

from . import configuration as C
from package.calculator_job import (
    _start_calculator,
)
import package.calculation_input.grid_loss_responsible as grid_loss_responsible
from package.calculator_args import CalculatorArgs
from package.codelists.process_type import ProcessType
from package.constants import Colname
import package.infrastructure as infra
from package.schemas import time_series_point_schema, metering_point_period_schema


@pytest.fixture(scope="session")
def calculator_args_balance_fixing(
    data_lake_path: str
) -> CalculatorArgs:
    return CalculatorArgs(data_storage_account_name="foo",
                          data_storage_account_credentials=ClientSecretCredential("foo", "foo", "foo"),
                          wholesale_container_path=f"{data_lake_path}",
                          batch_id=C.executed_balance_fixing_batch_id,
                          batch_process_type=ProcessType.BALANCE_FIXING,
                          batch_grid_areas=["805", "806"],
                          batch_period_start_datetime=datetime(2018, 1, 1, 23, 0, 0),
                          batch_period_end_datetime=datetime(2018, 1, 3, 23, 0, 0),
                          batch_execution_time_start=datetime(2018, 1, 5, 23, 0, 0),
                          time_zone="Europe/Copenhagen",)


@pytest.fixture(scope="session")
def calculator_args_wholesale_fixing(
    calculator_args_balance_fixing: CalculatorArgs
) -> CalculatorArgs:
    args = calculator_args_balance_fixing
    args.batch_id = C.executed_wholesale_batch_id
    args.batch_process_type = ProcessType.WHOLESALE_FIXING
    return args


@pytest.fixture(scope="session")
def grid_loss_responsible_test_data(
    spark: SparkSession,
    test_files_folder_path: str,
) -> DataFrame:
    return spark.read.csv(f"{test_files_folder_path}/GridLossResponsible.csv", header=True)


@pytest.fixture(scope="session")
def test_data_written_to_delta_tables(
    spark: SparkSession,
    test_files_folder_path: str,
    data_lake_path: str,
) -> None:

    metering_points_df = spark.read.csv(
        f"{test_files_folder_path}/MeteringPointsPeriods.csv",
        header=True,
        schema=metering_point_period_schema,
    )

    metering_points_df.write.format("delta").mode("overwrite").saveAsTable(f"{infra.INPUT_DATABASE_NAME}.{infra.METERING_POINT_PERIODS_TABLE_NAME}")

    timeseries_points_df = spark.read.csv(
        f"{test_files_folder_path}/TimeSeriesPoints.csv",
        header=True,
        schema=time_series_point_schema,
    )

    timeseries_points_df.write.format("delta").mode("overwrite").saveAsTable(f"{infra.INPUT_DATABASE_NAME}.{infra.TIME_SERIES_POINTS_TABLE_NAME}")


@pytest.fixture(scope="session")
def executed_balance_fixing(
    spark: SparkSession,
    calculator_args_balance_fixing: CalculatorArgs,
    migrations_executed: None,
    test_data_written_to_delta_tables: None,
    grid_loss_responsible_test_data: DataFrame,
) -> None:
    """Execute the calculator job.
    This is the act part of a test in the arrange-act-assert paradigm.
    This act is made as a session-scoped fixture because it is a slow process
    and because lots of assertions can be made and split into seperate tests
    without awaiting the execution in each test."""

    with patch.object(grid_loss_responsible, '_get_all_grid_loss_responsible', return_value=grid_loss_responsible_test_data):
        _start_calculator(calculator_args_balance_fixing, spark)


@pytest.fixture(scope="session")
def executed_wholesale_fixing(
    spark: SparkSession,
    calculator_args_wholesale_fixing: CalculatorArgs,
    migrations_executed: None,
    test_data_written_to_delta_tables: None,
    grid_loss_responsible_test_data: DataFrame,
) -> None:
    """Execute the calculator job.
    This is the act part of a test in the arrange-act-assert paradigm.
    This act is made as a session-scoped fixture because it is a slow process
    and because lots of assertions can be made and split into seperate tests
    without awaiting the execution in each test."""

    with patch.object(grid_loss_responsible, '_get_all_grid_loss_responsible', return_value=grid_loss_responsible_test_data):
        _start_calculator(calculator_args_wholesale_fixing, spark)


@pytest.fixture(scope="session")
def balance_fixing_results_df(
    spark: SparkSession,
    executed_balance_fixing: None,
) -> DataFrame:
    results_df = spark.read.table(f"{infra.OUTPUT_DATABASE_NAME}.{infra.ENERGY_RESULT_TABLE_NAME}")
    return results_df.where(F.col(Colname.batch_id) == C.executed_balance_fixing_batch_id)


@pytest.fixture(scope="session")
def wholesale_fixing_results_df(
    spark: SparkSession,
    executed_wholesale_fixing: None,
) -> DataFrame:
    results_df = spark.read.table(f"{infra.OUTPUT_DATABASE_NAME}.{infra.ENERGY_RESULT_TABLE_NAME}")
    return results_df.where(F.col(Colname.batch_id) == C.executed_wholesale_batch_id)
