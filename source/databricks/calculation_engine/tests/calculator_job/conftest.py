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
import uuid
from datetime import datetime

import pyspark.sql.functions as F
import pytest
from pyspark.sql import SparkSession, DataFrame

import package.calculation as calculation
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.preparation import PreparedDataReader

from package.calculation.input import TableReader
from package.codelists.calculation_type import (
    CalculationType,
)
from package.constants import EnergyResultColumnNames, WholesaleResultColumnNames
from package.constants.result_column_names import ResultColumnNames
from package.infrastructure import paths
from . import configuration as C


@pytest.fixture(scope="session")
def calculator_args_balance_fixing() -> CalculatorArgs:
    return CalculatorArgs(
        calculation_id=C.executed_balance_fixing_calculation_id,
        calculation_type=CalculationType.BALANCE_FIXING,
        calculation_grid_areas=["805", "806"],
        calculation_period_start_datetime=datetime(2018, 1, 1, 23, 0, 0),
        calculation_period_end_datetime=datetime(2018, 1, 3, 23, 0, 0),
        calculation_execution_time_start=datetime(2018, 1, 5, 23, 0, 0),
        created_by_user_id=str(uuid.uuid4()),
        time_zone="Europe/Copenhagen",
        quarterly_resolution_transition_datetime=datetime(2023, 1, 31, 23, 0, 0),
    )


@pytest.fixture(scope="session")
def calculator_args_wholesale_fixing(
    calculator_args_balance_fixing: CalculatorArgs,
) -> CalculatorArgs:
    args = calculator_args_balance_fixing
    args.calculation_id = C.executed_wholesale_calculation_id
    args.calculation_type = CalculationType.WHOLESALE_FIXING
    args.calculation_period_start_datetime = datetime(2017, 12, 31, 23, 0, 0)
    args.calculation_period_end_datetime = datetime(2018, 1, 31, 23, 0, 0)
    args.calculation_execution_time_start = datetime(2018, 2, 1, 15, 0, 0)

    return args


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
    calculation.execute(calculator_args_balance_fixing, prepared_data_reader, spark)


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
    calculation.execute(calculator_args_wholesale_fixing, prepared_data_reader, spark)


@pytest.fixture(scope="session")
def balance_fixing_results_df(
    spark: SparkSession,
    executed_balance_fixing: None,
) -> DataFrame:
    results_df = spark.read.table(
        f"{paths.HiveOutputDatabase.DATABASE_NAME}.{paths.HiveOutputDatabase.ENERGY_RESULT_TABLE_NAME}"
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
        f"{paths.HiveOutputDatabase.DATABASE_NAME}.{paths.HiveOutputDatabase.ENERGY_RESULT_TABLE_NAME}"
    )
    return results_df.where(
        F.col(EnergyResultColumnNames.calculation_id)
        == C.executed_wholesale_calculation_id
    )


@pytest.fixture(scope="session")
def wholesale_fixing_wholesale_results_df(
    spark: SparkSession,
    executed_wholesale_fixing: None,
) -> DataFrame:
    results_df = spark.read.table(
        f"{paths.HiveOutputDatabase.DATABASE_NAME}.{paths.HiveOutputDatabase.WHOLESALE_RESULT_TABLE_NAME}"
    )
    return results_df.where(
        F.col(WholesaleResultColumnNames.calculation_id)
        == C.executed_wholesale_calculation_id
    )


@pytest.fixture(scope="session")
def wholesale_fixing_total_monthly_amounts(
    spark: SparkSession,
    executed_wholesale_fixing: None,
) -> DataFrame:
    results_df = spark.read.table(
        f"{paths.HiveOutputDatabase.DATABASE_NAME}.{paths.HiveOutputDatabase.TOTAL_MONTHLY_AMOUNTS_TABLE_NAME}"
    )
    return results_df.where(
        F.col(ResultColumnNames.calculation_id) == C.executed_wholesale_calculation_id
    )


@pytest.fixture(scope="session")
def wholesale_fixing_monthly_amounts(
    spark: SparkSession,
    executed_wholesale_fixing: None,
) -> DataFrame:
    results_df = spark.read.table(
        f"{paths.HiveOutputDatabase.DATABASE_NAME}.{paths.HiveOutputDatabase.MONTHLY_AMOUNTS_TABLE_NAME}"
    )
    return results_df.where(
        F.col(WholesaleResultColumnNames.calculation_id)
        == C.executed_wholesale_calculation_id
    )
