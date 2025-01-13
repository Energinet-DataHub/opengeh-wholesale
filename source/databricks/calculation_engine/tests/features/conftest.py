from pathlib import Path
from unittest.mock import Mock

import pytest
import yaml
from _pytest.fixtures import FixtureRequest
from pyspark.sql import SparkSession
from testcommon.dataframes import AssertDataframesConfiguration, read_csv
from testcommon.etl import TestCase, TestCases

from tests.features.utils.expected_output import ExpectedOutput
from tests.features.utils.scenario_executor import ScenarioExecutor
from tests.features.utils.views.dataframe_wrapper import DataframeWrapper
from tests.features.utils.views.view_scenario_executor import ViewScenarioExecutor
from package.calculation.calculation_output import CalculationOutput

from source.databricks.calculation_engine.package.calculation import CalculationCore, PreparedDataReader
from source.databricks.calculation_engine.package.calculation.calculator_args import CalculatorArgs
from source.databricks.calculation_engine.package.databases.migrations_wholesale import MigrationsWholesaleRepository
from source.databricks.calculation_engine.package.databases.migrations_wholesale.schemas import \
    time_series_points_schema, metering_point_periods_schema
from source.databricks.calculation_engine.package.databases.wholesale_internal import WholesaleInternalRepository
from source.databricks.calculation_engine.package.databases.wholesale_internal.schemas import \
    grid_loss_metering_point_ids_schema

from source.databricks.calculation_engine.tests.features.utils.calculation_args import create_calculation_args
from testsession_configuration import TestSessionConfiguration


@pytest.fixture(scope="module", autouse=True)
def clear_cache(spark: SparkSession) -> None:
    yield
    # Clear the cache after each test module to avoid memory issues
    spark.catalog.clearCache()


@pytest.fixture(scope="module")
def actual_and_expected(
    request: FixtureRequest,
    spark: SparkSession,
) -> tuple[CalculationOutput, list[ExpectedOutput]]:
    """
    Provides the actual and expected output for a scenario test case.

    IMPORTANT: It is crucial that this fixture has scope=module, as the scenario executor
    is specific to a single scenario, which are each located in their own module.
    """

    scenario_path = str(Path(request.module.__file__).parent)
    scenario_executor = ScenarioExecutor(spark)
    return scenario_executor.execute(scenario_path)


@pytest.fixture(scope="module")
def actual_and_expected_views(
    migrations_executed: None,
    request: FixtureRequest,
    spark: SparkSession,
) -> tuple[list[DataframeWrapper], list[DataframeWrapper]]:
    """
    Provides the actual and expected output for a scenario test case.

    IMPORTANT: It is crucial that this fixture has scope=module, as the scenario executor
    is specific to a single scenario, which are each located in their own module.
    """

    scenario_path = str(Path(request.module.__file__).parent)
    executor = ViewScenarioExecutor(spark)
    return executor.execute(scenario_path)

@pytest.fixture(scope="module")
def test_cases(spark: SparkSession, request: pytest.FixtureRequest) -> TestCases:
    """Fixture used for scenario tests. Learn more in package `testcommon.etl`."""

    # Get the path to the scenario
    scenario_path = str(Path(request.module.__file__).parent)
    calculation_args = create_calculation_args(f"{scenario_path}/when/")

    # Read input data
    time_series_points = read_csv(
        spark,
        f"{scenario_path}/when/time_series_points.csv",
        time_series_points_schema,
    )
    time_series_points = spark.createDataFrame(time_series_points.rdd, schema=time_series_points_schema, verifySchema=True)
    grid_loss_metering_points = read_csv(
        spark,
        f"{scenario_path}/when/grid_loss_metering_points.csv",
        grid_loss_metering_point_ids_schema,
    )
    grid_loss_metering_points = spark.createDataFrame(grid_loss_metering_points.rdd, schema=grid_loss_metering_point_ids_schema, verifySchema=True)

    metering_point_periods = read_csv(
        spark,
        f"{scenario_path}/when/metering_point_periods.csv",
        metering_point_periods_schema,
    )
    metering_point_periods = spark.createDataFrame(metering_point_periods.rdd, schema=metering_point_periods_schema, verifySchema=True)


    migrations_wholesale_repository: MigrationsWholesaleRepository = Mock()
    migrations_wholesale_repository.read_time_series_points.return_value = time_series_points
    migrations_wholesale_repository.read_metering_point_periods.return_value = metering_point_periods

    wholesale_internal_repository: WholesaleInternalRepository = Mock()
    wholesale_internal_repository.read_grid_loss_metering_point_ids.return_value = grid_loss_metering_points


    # Execute the calculation logic
    calculation_output = CalculationCore().execute(
        calculation_args,
        PreparedDataReader(
            migrations_wholesale_repository,
            wholesale_internal_repository,
        ),
    )

    # Return test cases
    return TestCases(
        [
            TestCase(
                expected_csv_path=f"{scenario_path}/then/basis_data/grid_loss_metering_points.csv",
                actual=calculation_output.basis_data_output.grid_loss_metering_points
            ),
            TestCase(
                expected_csv_path=f"{scenario_path}/then/basis_data/metering_point_periods.csv",
                actual=calculation_output.basis_data_output.metering_point_periods
            ),
            TestCase(
                expected_csv_path=f"{scenario_path}/then/basis_data/time_series_points.csv",
                actual=calculation_output.basis_data_output.time_series_points
            )
        ]
    )

@pytest.fixture(scope="session")
def assert_dataframes_configuration(
    test_session_configuration: TestSessionConfiguration,
) -> AssertDataframesConfiguration:
    return AssertDataframesConfiguration(
        show_actual_and_expected_count=test_session_configuration.feature_tests.show_actual_and_expected_count,
        show_actual_and_expected=test_session_configuration.feature_tests.show_actual_and_expected,
        show_columns_when_actual_and_expected_are_equal=test_session_configuration.feature_tests.show_columns_when_actual_and_expected_are_equal,
    )
