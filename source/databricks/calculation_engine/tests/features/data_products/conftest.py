from pathlib import Path

import pytest
from _pytest.fixtures import FixtureRequest
from pyspark.sql import SparkSession
from testcommon.dataframes.write_to_delta import write_when_files_to_delta
from testcommon.etl import TestCase, TestCases, get_then_names

from package.databases.wholesale_basis_data_internal.schemas import (
    metering_point_periods_schema,
    time_series_points_schema,
    charge_link_periods_schema,
    charge_price_information_periods_schema,
    charge_price_points_schema,
    grid_loss_metering_point_ids_schema,
)
from package.databases.wholesale_internal.schemas import (
    calculations_schema,
    calculation_grid_areas_schema,
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
        (
            "wholesale_internal.calculations.csv",
            calculations_schema,
        ),
        (
            "wholesale_internal.calculation_grid_areas.csv",
            calculation_grid_areas_schema,
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
