from pathlib import Path

import pytest
from _pytest.fixtures import FixtureRequest
from geh_common.testing.dataframes.write_to_delta import write_when_files_to_delta
from geh_common.testing.scenario_testing import TestCase, TestCases, get_then_names
from pyspark.sql import SparkSession

from geh_wholesale.databases.wholesale_basis_data_internal.schemas import (
    charge_link_periods_schema,
    charge_price_information_periods_schema,
    charge_price_points_schema,
    grid_loss_metering_point_ids_schema,
    metering_point_periods_schema,
    time_series_points_schema,
)
from geh_wholesale.databases.wholesale_internal.schemas import (
    calculation_grid_areas_schema,
    calculations_schema,
)
from geh_wholesale.databases.wholesale_results_internal.schemas import (
    amounts_per_charge_schema,
    energy_per_brp_schema,
    energy_per_es_schema,
    energy_schema,
    exchange_per_neighbor_schema,
    grid_loss_metering_point_time_series_schema,
    monthly_amounts_schema_uc,
    total_monthly_amounts_schema_uc,
)


@pytest.fixture(scope="module")
def test_cases(
    migrations_executed: None,
    request: FixtureRequest,
    spark: SparkSession,
) -> TestCases:
    scenario_path = str(Path(request.module.__file__).parent)

    # Define a list of tuples containing a 'when' file name and the corresponding schema
    path_schema_tuples = [
        (
            "wholesale_basis_data_internal.charge_link_periods.csv",
            charge_link_periods_schema,
        ),
        (
            "wholesale_basis_data_internal.charge_price_information_periods.csv",
            charge_price_information_periods_schema,
        ),
        (
            "wholesale_basis_data_internal.charge_price_points.csv",
            charge_price_points_schema,
        ),
        (
            "wholesale_basis_data_internal.grid_loss_metering_points.csv",
            grid_loss_metering_point_ids_schema,
        ),
        (
            "wholesale_basis_data_internal.metering_point_periods.csv",
            metering_point_periods_schema,
        ),
        (
            "wholesale_basis_data_internal.time_series_points.csv",
            time_series_points_schema,
        ),
        ("wholesale_internal.calculations.csv", calculations_schema),
        (
            "wholesale_internal.calculation_grid_areas.csv",
            calculation_grid_areas_schema,
        ),
        ("wholesale_results_internal.energy.csv", energy_schema),
        ("wholesale_results_internal.energy_per_brp.csv", energy_per_brp_schema),
        ("wholesale_results_internal.energy_per_es.csv", energy_per_es_schema),
        (
            "wholesale_results_internal.exchange_per_neighbor_ga.csv",
            exchange_per_neighbor_schema,
        ),
        (
            "wholesale_results_internal.amounts_per_charge.csv",
            amounts_per_charge_schema,
        ),
        (
            "wholesale_results_internal.monthly_amounts_per_charge.csv",
            monthly_amounts_schema_uc,
        ),
        (
            "wholesale_results_internal.total_monthly_amounts.csv",
            total_monthly_amounts_schema_uc,
        ),
        (
            "wholesale_results_internal.grid_loss_metering_point_time_series.csv",
            grid_loss_metering_point_time_series_schema,
        ),
    ]

    # Populate the delta tables with the 'when' files.
    write_when_files_to_delta(
        spark,
        scenario_path,
        path_schema_tuples,
    )

    # Receive a list of 'then' file names
    then_files = get_then_names(scenario_path)

    # Construct a list of TestCase objects
    test_cases = []
    for path_name in then_files:
        actual = spark.sql(f"SELECT * FROM {path_name}")

        test_cases.append(
            TestCase(
                expected_csv_path=f"{scenario_path}/then/{path_name}.csv",
                actual=actual,
            )
        )

    return TestCases(test_cases)
