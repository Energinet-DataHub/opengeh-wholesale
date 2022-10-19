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
import shutil
import subprocess
import pytest
from pyspark.sql.functions import col
from pyspark.sql.types import (
    DecimalType,
    StructType,
    StructField,
    StringType,
    TimestampType,
    IntegerType,
    LongType,
)
from tests.contract_utils import assert_contract_matches_schema
from package.calculator_job import internal_start as start_calculator


def _get_process_manager_parameters(filename):
    """Get the parameters as they are expected to be received from the process manager."""
    with open(filename) as file:
        text = file.read()
        text = text.replace("{batch-id}", "any-guid-id")
        lines = text.splitlines()
        return list(
            filter(lambda line: not line.startswith("#") and len(line) > 0, lines)
        )


def test_calculator_job_when_invoked_with_incorrect_parameters_fails(
    integration_tests_path, databricks_path
):
    exit_code = subprocess.call(
        [
            "python",
            f"{databricks_path}/package/calculator_job.py",
            "--unexpected-arg",
        ]
    )

    assert (
        exit_code != 0
    ), "Calculator job should return non-zero exit when invoked with bad arguments"


def test_calculator_job_accepts_parameters_from_process_manager(
    data_lake_path, source_path, databricks_path
):
    """
    This test works in tandem with a .NET test ensuring that the calculator job accepts
    the arguments that are provided by the calling process manager.
    """

    # Arrange
    process_manager_parameters = _get_process_manager_parameters(
        f"{source_path}/contracts/calculation-job-parameters-reference.txt"
    )

    python_parameters = [
        "python",
        f"{databricks_path}/package/calculator_job.py",
        "--data-storage-account-name",
        "foo",
        "--data-storage-account-key",
        "foo",
        "--integration-events-path",
        "foo",
        "--time-series-points-path",
        "foo",
        "--process-results-path",
        "foo",
        "--only-validate-args",
        "1",
        "--time-zone",
        "Europe/Copenhagen",
    ]
    python_parameters.extend(process_manager_parameters)

    # Act
    exit_code = subprocess.call(python_parameters)

    # Assert
    assert exit_code == 0, "Calculator job failed to accept provided input arguments"


def test__result_is_generated_for_requested_grid_areas(
    spark, test_data_job_parameters, data_lake_path, source_path
):
    # Act
    start_calculator(spark, test_data_job_parameters)

    # Assert
    result_805 = spark.read.json(f"{data_lake_path}/results/batch_id=1/grid_area=805")
    result_806 = spark.read.json(f"{data_lake_path}/results/batch_id=1/grid_area=806")
    assert result_805.count() >= 1, "Calculator job failed to write files"
    assert result_806.count() >= 1, "Calculator job failed to write files"


def test__published_time_series_points_contract_matches_schema_from_input_time_series_points(
    spark, test_data_job_parameters, data_lake_path, source_path
):
    # Act
    start_calculator(spark, test_data_job_parameters)

    # Assert
    input_time_series_points = spark.read.parquet(
        f"{data_lake_path}/parquet_test_files/time_series_points"
    )
    # When asserting both that the calculator creates output and it does it with input data that matches
    # the time series points contract from the time-series domain (in the same test), then we can infer that the
    # calculator works with the format of the data published from the time-series domain.
    assert_contract_matches_schema(
        f"{source_path}/contracts/events/published-time-series-points.json",
        input_time_series_points.schema,
    )


def test__calculator_result_schema_must_match_contract_with_dotnet(
    spark, test_data_job_parameters, data_lake_path, source_path
):
    # Act
    start_calculator(spark, test_data_job_parameters)

    # Assert
    result_805 = spark.read.json(f"{data_lake_path}/results/batch_id=1/grid_area=805")
    assert_contract_matches_schema(
        f"{source_path}/contracts/calculator-result.json",
        result_805.schema,
    )


def test__quantity_is_with_precision_3(
    spark, test_data_job_parameters, data_lake_path, find_first_file
):
    # Act
    start_calculator(spark, test_data_job_parameters)

    # Assert: Quantity output is a string encoded decimal with precision 3 (number of digits after delimiter)
    # Note that any change or violation may impact consumers that expects exactly this precision from the result
    result_805 = spark.read.json(f"{data_lake_path}/results/batch_id=1/grid_area=805")
    import re

    assert re.search(r"^\d+\.\d{3}$", result_805.first().quantity)


def test__result_file_is_created(
    spark, test_data_job_parameters, data_lake_path, find_first_file
):
    # Act
    start_calculator(spark, test_data_job_parameters)

    # Assert: Relative path of result file must match expectation of .NET
    # IMPORTANT: If the expected result path changes it probably requires .NET changes too
    expected_result_path = f"{data_lake_path}/results/batch_id=1/grid_area=805"
    actual_result_file = find_first_file(expected_result_path, "part-*.json")
    assert actual_result_file is not None


def test__creates_hour_csv_with_expected_columns_names(
    spark, test_data_job_parameters, data_lake_path
):
    # Act
    start_calculator(spark, test_data_job_parameters)

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/results/basis-data/batch_id=1/time-series-hour/grid_area=805"
    )

    assert actual.columns == [
        "METERINGPOINTID",
        "TYPEOFMP",
        "STARTDATETIME",
        *[f"ENERGYQUANTITY{i+1}" for i in range(24)],
    ]


def test__creates_quarter_csv_with_expected_columns_names(
    spark, test_data_job_parameters, data_lake_path
):
    # Act
    start_calculator(spark, test_data_job_parameters)

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/results/basis-data/batch_id=1/time-series-quarter/grid_area=805"
    )

    assert actual.columns == [
        "METERINGPOINTID",
        "TYPEOFMP",
        "STARTDATETIME",
        *[f"ENERGYQUANTITY{i+1}" for i in range(96)],
    ]


def test__creates_csv_per_grid_area(spark, test_data_job_parameters, data_lake_path):
    # Act
    start_calculator(spark, test_data_job_parameters)

    # Assert
    basis_data_805 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/results/basis-data/batch_id=1/time-series-quarter/grid_area=805"
    )

    basis_data_806 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/results/basis-data/batch_id=1/time-series-quarter/grid_area=806"
    )

    assert (
        basis_data_805.count() >= 1
    ), "Calculator job failed to write basis data files for grid area 805"

    assert (
        basis_data_806.count() >= 1
    ), "Calculator job failed to write basis data files for grid area 806"


def test__master_data_csv_with_expected_columns_names(
    spark, test_data_job_parameters, data_lake_path
):
    # Act
    start_calculator(spark, test_data_job_parameters)

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/results/master-basis-data/batch_id=1/grid_area=805"
    )

    assert actual.columns == [
        "METERINGPOINTID",
        "VALIDFROM",
        "VALIDTO",
        "GRIDAREA",
        "TOGRIDAREA",
        "FROMGRIDAREA",
        "TYPEOFMP",
        "SETTLEMENTMETHOD",
        "ENERGYSUPPLIERID",
    ]
