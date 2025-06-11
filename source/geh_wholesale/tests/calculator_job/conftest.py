import os
import sys
import uuid
from datetime import datetime, timezone

import pyspark.sql.functions as F
import pytest
from featuremanagement import FeatureManager
from pyspark.sql import DataFrame, SparkSession

import geh_wholesale.calculation as calculation
from geh_wholesale.calculation import CalculationCore
from geh_wholesale.calculation.calculation_metadata_service import CalculationMetadataService
from geh_wholesale.calculation.calculation_output_service import CalculationOutputService
from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.calculation.preparation import PreparedDataReader
from geh_wholesale.calculation.preparation.prepared_data_reader import MigrationsWholesaleRepository
from geh_wholesale.codelists.calculation_type import (
    CalculationType,
)
from geh_wholesale.databases import migrations_wholesale, wholesale_internal
from geh_wholesale.databases.table_column_names import TableColumnNames
from geh_wholesale.infrastructure import paths
from tests import SPARK_CATALOG_NAME

from . import configuration as C

DEFAULT_ARGS = {
    "calculation-id": str(uuid.uuid4()),
    "calculation-type": CalculationType.BALANCE_FIXING.value,
    "grid-areas": ["805", "806"],
    "period-start-datetime": datetime(2018, 1, 1, 23, 0, 0, tzinfo=timezone.utc).isoformat(),
    "period-end-datetime": datetime(2018, 1, 3, 23, 0, 0, tzinfo=timezone.utc).isoformat(),
    "created-by-user-id": str(uuid.uuid4()),
}

DEFAULT_ENV = {
    "TIME_ZONE": "Europe/Copenhagen",
    "QUARTERLY_RESOLUTION_TRANSITION_DATETIME": datetime(2023, 1, 31, 23, 0, 0, tzinfo=timezone.utc).isoformat(),
}


def _to_args_list(args: dict) -> str:
    return [CalculatorArgs.model_config.get("cli_prog_name", "calculator")] + [f"--{k}={v}" for k, v in args.items()]


@pytest.fixture(scope="session")
def calculator_args_balance_fixing() -> CalculatorArgs:
    with pytest.MonkeyPatch.context() as mp:
        args_dict = DEFAULT_ARGS.copy()
        args_dict["calculation-id"] = C.executed_balance_fixing_calculation_id
        mp.setattr(
            sys,
            "argv",
            _to_args_list(args_dict),
        )
        mp.setattr(os, "environ", DEFAULT_ENV)
        return CalculatorArgs()


@pytest.fixture(scope="session")
def calculator_args_wholesale_fixing() -> CalculatorArgs:
    with pytest.MonkeyPatch.context() as mp:
        args_dict = DEFAULT_ARGS.copy()
        args_dict["calculation-id"] = C.executed_wholesale_calculation_id
        args_dict["calculation-type"] = CalculationType.WHOLESALE_FIXING.value
        args_dict["period-start-datetime"] = datetime(2017, 12, 31, 23, 0, 0, tzinfo=timezone.utc).isoformat()
        args_dict["period-end-datetime"] = datetime(2018, 1, 31, 23, 0, 0, tzinfo=timezone.utc).isoformat()
        mp.setattr(
            sys,
            "argv",
            _to_args_list(args_dict),
        )
        mp.setattr(os, "environ", DEFAULT_ENV)
        return CalculatorArgs()


@pytest.fixture(scope="session")
def migrations_wholesale_repository(
    spark: SparkSession,
    mock_feature_manager_false: FeatureManager,
    calculation_input_database: str,
    measurements_gold_database: str,
) -> migrations_wholesale.MigrationsWholesaleRepository:
    """Create a migrations wholesale repository."""
    return migrations_wholesale.MigrationsWholesaleRepository(
        spark,
        mock_feature_manager_false,
        SPARK_CATALOG_NAME,
        calculation_input_database,
        measurements_gold_database,
    )


@pytest.fixture(scope="session")
def executed_balance_fixing(
    spark: SparkSession,
    migrations_wholesale_repository: MigrationsWholesaleRepository,
    calculator_args_balance_fixing: CalculatorArgs,
    energy_input_data_written_to_delta: None,
    grid_loss_metering_point_ids_input_data_written_to_delta,
) -> None:
    """Execute the calculator job.
    This is the act part of a test in the arrange-act-assert paradigm.
    This act is made as a session-scoped fixture because it is a slow process
    and because lots of assertions can be made and split into separate tests
    without awaiting the execution in each test."""
    wholesale_internal_repository = wholesale_internal.WholesaleInternalRepository(spark, SPARK_CATALOG_NAME)
    prepared_data_reader = PreparedDataReader(migrations_wholesale_repository, wholesale_internal_repository)
    calculation.execute(
        calculator_args_balance_fixing,
        prepared_data_reader,
        CalculationCore(),
        CalculationMetadataService(),
        CalculationOutputService(),
    )


@pytest.fixture(scope="session")
def executed_wholesale_fixing(
    spark: SparkSession,
    migrations_wholesale_repository: MigrationsWholesaleRepository,
    calculator_args_wholesale_fixing: CalculatorArgs,
    energy_input_data_written_to_delta: None,
    price_input_data_written_to_delta: None,
    grid_loss_metering_point_ids_input_data_written_to_delta,
) -> None:
    """Execute the calculator job.
    This is the act part of a test in the arrange-act-assert paradigm.
    This act is made as a session-scoped fixture because it is a slow process
    and because lots of assertions can be made and split into seperate tests
    without awaiting the execution in each test."""
    wholesale_internal_repository = wholesale_internal.WholesaleInternalRepository(spark, SPARK_CATALOG_NAME)
    prepared_data_reader = PreparedDataReader(migrations_wholesale_repository, wholesale_internal_repository)
    calculation.execute(
        calculator_args_wholesale_fixing,
        prepared_data_reader,
        CalculationCore(),
        CalculationMetadataService(),
        CalculationOutputService(),
    )


@pytest.fixture(scope="session")
def wholesale_fixing_energy_results_df(
    spark: SparkSession,
    executed_wholesale_fixing: None,
) -> DataFrame:
    results_df = spark.read.table(
        f"{paths.WholesaleResultsInternalDatabase.DATABASE_NAME}.{paths.WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME}"
    )
    return results_df.where(F.col(TableColumnNames.calculation_id) == C.executed_wholesale_calculation_id)


@pytest.fixture(scope="session")
def wholesale_fixing_amounts_per_charge_df(
    spark: SparkSession,
    executed_wholesale_fixing: None,
) -> DataFrame:
    results_df = spark.read.table(
        f"{paths.WholesaleResultsInternalDatabase.DATABASE_NAME}.{paths.WholesaleResultsInternalDatabase.AMOUNTS_PER_CHARGE_TABLE_NAME}"
    )
    return results_df.where(F.col(TableColumnNames.calculation_id) == C.executed_wholesale_calculation_id)


@pytest.fixture(scope="session")
def wholesale_fixing_total_monthly_amounts_df(
    spark: SparkSession,
    executed_wholesale_fixing: None,
) -> DataFrame:
    results_df = spark.read.table(
        f"{paths.WholesaleResultsInternalDatabase.DATABASE_NAME}.{paths.WholesaleResultsInternalDatabase.TOTAL_MONTHLY_AMOUNTS_TABLE_NAME}"
    )
    return results_df.where(F.col(TableColumnNames.calculation_id) == C.executed_wholesale_calculation_id)


@pytest.fixture(scope="session")
def wholesale_fixing_monthly_amounts_per_charge_df(
    spark: SparkSession,
    executed_wholesale_fixing: None,
) -> DataFrame:
    results_df = spark.read.table(
        f"{paths.WholesaleResultsInternalDatabase.DATABASE_NAME}.{paths.WholesaleResultsInternalDatabase.MONTHLY_AMOUNTS_PER_CHARGE_TABLE_NAME}"
    )
    return results_df.where(F.col(TableColumnNames.calculation_id) == C.executed_wholesale_calculation_id)
