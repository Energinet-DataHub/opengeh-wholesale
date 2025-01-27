from pathlib import Path
from unittest.mock import Mock

import pytest
from _pytest.fixtures import FixtureRequest
from pyspark.sql import SparkSession
from testcommon.dataframes import AssertDataframesConfiguration, read_csv
from testcommon.etl import TestCase, TestCases

from features.utils.calculation_args import create_calculation_args
from package.calculation import CalculationCore, PreparedDataReader
from package.codelists.calculation_type import is_wholesale_calculation_type
from package.databases.migrations_wholesale.schemas import charge_price_points_schema
from tests.features.utils.expected_output import ExpectedOutput
from tests.features.utils.scenario_executor import ScenarioExecutor

from package.calculation.calculation_output import CalculationOutput

from package.databases.migrations_wholesale import (
    MigrationsWholesaleRepository,
)
from package.databases.migrations_wholesale.schemas import (
    time_series_points_schema,
    metering_point_periods_schema,
    charge_link_periods_schema,
    charge_price_information_periods_schema,
)
from package.databases.wholesale_internal import (
    WholesaleInternalRepository,
)
from package.databases.wholesale_internal.schemas import (
    grid_loss_metering_point_ids_schema,
)

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

    grid_loss_metering_points = read_csv(
        spark,
        f"{scenario_path}/when/grid_loss_metering_points.csv",
        grid_loss_metering_point_ids_schema,
    )

    metering_point_periods = read_csv(
        spark,
        f"{scenario_path}/when/metering_point_periods.csv",
        metering_point_periods_schema,
    )

    # Defining the mocks for the data frames in the "when" folder
    migrations_wholesale_repository: MigrationsWholesaleRepository = Mock()
    migrations_wholesale_repository.read_time_series_points.return_value = time_series_points
    migrations_wholesale_repository.read_metering_point_periods.return_value = metering_point_periods

    wholesale_internal_repository: WholesaleInternalRepository = Mock()
    wholesale_internal_repository.read_grid_loss_metering_point_ids.return_value = grid_loss_metering_points

    # Only for wholesale we need these additional tests
    if is_wholesale_calculation_type(calculation_args.calculation_type):
        charge_link_periods = read_csv(
            spark,
            f"{scenario_path}/when/charge_link_periods.csv",
            charge_link_periods_schema,
        )

        charge_price_information_periods = read_csv(
            spark,
            f"{scenario_path}/when/charge_price_information_periods.csv",
            charge_price_information_periods_schema,
        )

        charge_price_points = read_csv(
            spark,
            f"{scenario_path}/when/charge_price_points.csv",
            charge_price_points_schema,
        )

        # Mock the dataframes specific to the wholesales
        migrations_wholesale_repository.read_charge_link_periods.return_value = charge_link_periods
        migrations_wholesale_repository.read_charge_price_information_periods.return_value = (
            charge_price_information_periods
        )
        migrations_wholesale_repository.read_charge_price_points.return_value = charge_price_points

    # Execute the calculation logic
    calculation_output = CalculationCore().execute(
        calculation_args,
        PreparedDataReader(
            migrations_wholesale_repository,
            wholesale_internal_repository,
        ),
    )

    # Return test cases
    test_cases = []

    if (
        calculation_output.basis_data_output
    ):  # logic to look into any folders that are defined in the calculation_output
        test_cases.extend(
            [
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/basis_data/grid_loss_metering_points.csv",
                    actual=calculation_output.basis_data_output.grid_loss_metering_points,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/basis_data/metering_point_periods.csv",
                    actual=calculation_output.basis_data_output.metering_point_periods,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/basis_data/time_series_points.csv",
                    actual=calculation_output.basis_data_output.time_series_points,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/basis_data/charge_link_periods.csv",
                    actual=calculation_output.basis_data_output.charge_link_periods,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/basis_data/charge_price_information_periods.csv",
                    actual=calculation_output.basis_data_output.charge_price_information_periods,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/basis_data/charge_price_points.csv",
                    actual=calculation_output.basis_data_output.charge_price_points,
                ),
            ]
        )

    # Defining logic for when the dataframe is in 'Then' folder and not in 'When', then return none for the test for the wholesale_results
    if calculation_output.wholesale_results_output:
        test_cases.extend(
            [
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/wholesale_results/daily_tariff_per_co_es.csv",
                    actual=calculation_output.wholesale_results_output.daily_tariff_per_co_es,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/wholesale_results/hourly_tariff_per_co_es.csv",
                    actual=calculation_output.wholesale_results_output.hourly_tariff_per_co_es,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/wholesale_results/fee_per_co_es.csv",
                    actual=calculation_output.wholesale_results_output.fee_per_co_es,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/wholesale_results/subscription_per_co_es.csv",
                    actual=calculation_output.wholesale_results_output.subscription_per_co_es,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/wholesale_results/monthly_subscription_per_co_es.csv",
                    actual=calculation_output.wholesale_results_output.monthly_subscription_per_co_es,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/wholesale_results/monthly_fee_per_co_es.csv",
                    actual=calculation_output.wholesale_results_output.monthly_fee_per_co_es,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/wholesale_results/monthly_tariff_from_daily_per_co_es.csv",
                    actual=calculation_output.wholesale_results_output.monthly_tariff_from_daily_per_co_es,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/wholesale_results/monthly_tariff_from_hourly_per_co_es.csv",
                    actual=calculation_output.wholesale_results_output.monthly_tariff_from_hourly_per_co_es,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/wholesale_results/total_monthly_amounts_per_es.csv",
                    actual=calculation_output.wholesale_results_output.total_monthly_amounts_per_es,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/wholesale_results/total_monthly_amounts_per_co_es.csv",
                    actual=calculation_output.wholesale_results_output.total_monthly_amounts_per_co_es,
                ),
            ]
        )

    # Defining logic for when the dataframe is in 'Then' folder and not in 'When', then return none for the test for the energy_results
    if calculation_output.energy_results_output:
        test_cases.extend(
            [
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/exchange.csv",
                    actual=calculation_output.energy_results_output.exchange,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/flex_consumption_per_es.csv",
                    actual=calculation_output.energy_results_output.flex_consumption_per_es,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/grid_loss.csv",
                    actual=calculation_output.energy_results_output.grid_loss,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/production_per_es.csv",
                    actual=calculation_output.energy_results_output.production_per_es,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/total_consumption.csv",
                    actual=calculation_output.energy_results_output.total_consumption,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/non_profiled_consumption_per_es.csv",
                    actual=calculation_output.energy_results_output.non_profiled_consumption_per_es,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/flex_consumption.csv",
                    actual=calculation_output.energy_results_output.flex_consumption,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/non_profiled_consumption.csv",
                    actual=calculation_output.energy_results_output.non_profiled_consumption,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/non_profiled_consumption_per_es.csv",
                    actual=calculation_output.energy_results_output.non_profiled_consumption_per_es,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/production.csv",
                    actual=calculation_output.energy_results_output.production,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/temporary_flex_consumption.csv",
                    actual=calculation_output.energy_results_output.temporary_flex_consumption,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/temporary_production.csv",
                    actual=calculation_output.energy_results_output.temporary_production,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/exchange_per_neighbor.csv",
                    actual=calculation_output.energy_results_output.exchange_per_neighbor,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/negative_grid_loss.csv",
                    actual=calculation_output.energy_results_output.negative_grid_loss,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/positive_grid_loss.csv",
                    actual=calculation_output.energy_results_output.positive_grid_loss,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/flex_consumption_per_brp.csv",
                    actual=calculation_output.energy_results_output.flex_consumption_per_brp,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/non_profiled_consumption_per_brp.csv",
                    actual=calculation_output.energy_results_output.non_profiled_consumption_per_brp,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/non_profiled_consumption_per_brp.csv",
                    actual=calculation_output.energy_results_output.non_profiled_consumption_per_brp,
                ),
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/energy_results/production_per_brp.csv",
                    actual=calculation_output.energy_results_output.production_per_brp,
                ),
            ]
        )

    return TestCases(test_cases)


@pytest.fixture(scope="session")
def assert_dataframes_configuration(
    test_session_configuration: TestSessionConfiguration,
) -> AssertDataframesConfiguration:
    return AssertDataframesConfiguration(
        show_actual_and_expected_count=test_session_configuration.feature_tests.show_actual_and_expected_count,
        show_actual_and_expected=test_session_configuration.feature_tests.show_actual_and_expected,
        show_columns_when_actual_and_expected_are_equal=test_session_configuration.feature_tests.show_columns_when_actual_and_expected_are_equal,
    )
