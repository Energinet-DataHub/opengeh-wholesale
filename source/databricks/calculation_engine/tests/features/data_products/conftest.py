from pathlib import Path

import pytest
from _pytest.fixtures import FixtureRequest
from pyspark.sql import SparkSession
from testcommon.dataframes.write_to_delta import write_when_files_to_delta
from testcommon.etl import TestCase, TestCases

from package.databases.wholesale_basis_data_internal.schemas import (
    metering_point_periods_schema,
    time_series_points_schema,
    charge_link_periods_schema,
    charge_price_information_periods_schema,
    charge_price_points_schema,
    grid_loss_metering_point_ids_schema,
)
from package.databases.wholesale_internal.schemas import calculations_schema


@pytest.fixture(scope="module")
def test_cases(
    migrations_executed: None,
    request: FixtureRequest,
    spark: SparkSession,
) -> TestCases:
    scenario_path = str(Path(request.module.__file__).parent)

    path_schema_dict = {
        "wholesale_basis_data_internal.charge_link_periods.csv": charge_link_periods_schema,
        "wholesale_basis_data_internal.charge_price_information_periods.csv": charge_price_information_periods_schema,
        "wholesale_basis_data_internal.charge_price_points.csv": charge_price_points_schema,
        "wholesale_basis_data_internal.grid_loss_metering_points.csv": grid_loss_metering_point_ids_schema,
        "wholesale_basis_data_internal.metering_point_periods.csv": metering_point_periods_schema,
        "wholesale_basis_data_internal.time_series_points.csv": time_series_points_schema,
        "wholesale_internal.calculations.csv": calculations_schema,
    }

    write_when_files_to_delta(
        spark,
        scenario_path,
        list(path_schema_dict.items()),
    )

    # Define then path
    then_path = f"{scenario_path}/then"
    csv_files_then = [x.name for x in list(Path(then_path).rglob("*.csv"))]

    test_cases = []
    for path_name in csv_files_then:
        view_path_name = path_name.removesuffix(".csv")
        dict_path_name = path_name.removesuffix("_v1.csv")

        view = spark.sql(f"SELECT * FROM {view_path_name}")
        if dict_path_name in path_schema_dict.keys():
            test_cases.append(
                TestCase(
                    expected_csv_path=path_name,
                    actual=view,
                )
            )
        elif dict_path_name in "wholesale_internal.succeeded_external_calculations":
            test_cases.append(
                TestCase(
                    expected_csv_path="wholesale_internal.succeeded_external_calculations_v1.csv",
                    actual=view,
                )
            )

    return TestCases(test_cases)
