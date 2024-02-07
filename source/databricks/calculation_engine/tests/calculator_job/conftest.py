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

from datetime import datetime

import pyspark.sql.functions as F
import pytest
from azure.identity import ClientSecretCredential
from pyspark.sql import SparkSession, DataFrame

import package.calculation as calculation
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.preparation import PreparedDataReader

from package.calculation_input import TableReader
from package.codelists.process_type import ProcessType
from package.constants import EnergyResultColumnNames, WholesaleResultColumnNames
from package.infrastructure import paths
from . import configuration as C


@pytest.fixture(scope="session")
def calculator_args_balance_fixing(
    data_lake_path: str, calculation_input_path: str
) -> CalculatorArgs:
    return CalculatorArgs(
        data_storage_account_name="foo",
        data_storage_account_credentials=ClientSecretCredential("foo", "foo", "foo"),
        wholesale_container_path=data_lake_path,
        calculation_input_path=calculation_input_path,
        time_series_points_table_name=None,
        metering_point_periods_table_name=None,
        grid_loss_metering_points_table_name=None,
        calculation_id=C.executed_balance_fixing_calculation_id,
        calculation_process_type=ProcessType.BALANCE_FIXING,
        calculation_grid_areas=["805", "806"],
        calculation_period_start_datetime=datetime(2018, 1, 1, 23, 0, 0),
        calculation_period_end_datetime=datetime(2018, 1, 3, 23, 0, 0),
        calculation_execution_time_start=datetime(2018, 1, 5, 23, 0, 0),
        time_zone="Europe/Copenhagen",
    )


@pytest.fixture(scope="session")
def calculator_args_wholesale_fixing(
    calculator_args_balance_fixing: CalculatorArgs,
) -> CalculatorArgs:
    args = calculator_args_balance_fixing
    args.calculation_id = C.executed_wholesale_calculation_id
    args.calculation_process_type = ProcessType.WHOLESALE_FIXING
    return args


@pytest.fixture(scope="session")
def any_calculator_args(
    calculator_args_balance_fixing: CalculatorArgs,
) -> CalculatorArgs:
    return calculator_args_balance_fixing


@pytest.fixture(scope="session")
def executed_balance_fixing(
    spark: SparkSession,
    calculator_args_balance_fixing: CalculatorArgs,
    migrations_executed: None,
    energy_input_data_written_to_delta: None,
    calculation_input_path: str,
) -> None:
    """Execute the calculator job.
    This is the act part of a test in the arrange-act-assert paradigm.
    This act is made as a session-scoped fixture because it is a slow process
    and because lots of assertions can be made and split into separate tests
    without awaiting the execution in each test."""

    table_reader = TableReader(
        spark,
        calculation_input_path,
    )
    prepared_data_reader = PreparedDataReader(table_reader)
    calculation.execute(calculator_args_balance_fixing, prepared_data_reader)


@pytest.fixture(scope="session")
def executed_wholesale_fixing(
    spark: SparkSession,
    calculator_args_wholesale_fixing: CalculatorArgs,
    migrations_executed: None,
    energy_input_data_written_to_delta: None,
    price_input_data_written_to_delta: None,
    calculation_input_path: str,
) -> None:
    """Execute the calculator job.
    This is the act part of a test in the arrange-act-assert paradigm.
    This act is made as a session-scoped fixture because it is a slow process
    and because lots of assertions can be made and split into seperate tests
    without awaiting the execution in each test."""

    table_reader = TableReader(spark, calculation_input_path)
    prepared_data_reader = PreparedDataReader(table_reader)
    calculation.execute(calculator_args_wholesale_fixing, prepared_data_reader)


@pytest.fixture(scope="session")
def balance_fixing_results_df(
    spark: SparkSession,
    executed_balance_fixing: None,
) -> DataFrame:
    results_df = spark.read.table(
        f"{paths.OUTPUT_DATABASE_NAME}.{paths.ENERGY_RESULT_TABLE_NAME}"
    )
    return results_df.where(
        F.col(EnergyResultColumnNames.calculation_id)
        == C.executed_balance_fixing_calculation_id
    )


@pytest.fixture(scope="session")
def wholesale_fixing_energy_results_df(
    spark: SparkSession,
    executed_wholesale_fixing: None,
) -> DataFrame:
    results_df = spark.read.table(
        f"{paths.OUTPUT_DATABASE_NAME}.{paths.ENERGY_RESULT_TABLE_NAME}"
    )
    return results_df.where(
        F.col(EnergyResultColumnNames.calculation_id) == C.executed_wholesale_calculation_id
    )


@pytest.fixture(scope="session")
def wholesale_fixing_wholesale_results_df(
    spark: SparkSession,
    executed_wholesale_fixing: None,
) -> DataFrame:
    results_df = spark.read.table(
        f"{paths.OUTPUT_DATABASE_NAME}.{paths.WHOLESALE_RESULT_TABLE_NAME}"
    )
    return results_df.where(
        F.col(WholesaleResultColumnNames.calculation_id)
        == C.executed_wholesale_calculation_id
    )
