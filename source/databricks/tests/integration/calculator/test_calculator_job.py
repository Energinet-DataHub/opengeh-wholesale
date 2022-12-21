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

import re
from pyspark.sql import SparkSession
import pytest
import yaml
from unittest.mock import patch
from tests.contract_utils import assert_contract_matches_schema
from package.calculator_job import _get_valid_args_or_throw, _start_calculator, start
from package.calculator_args import CalculatorArgs
from package.schemas.time_series_point_schema import time_series_point_schema

executed_batch_id = "0b15a420-9fc8-409a-a169-fbd49479d718"


# Code snippet from https://joelmccune.com/python-dictionary-as-object/
class DictObj:
    def __init__(self, in_dict: dict):
        assert isinstance(in_dict, dict)
        for key, val in in_dict.items():
            if isinstance(val, (list, tuple)):
                setattr(
                    self, key, [DictObj(x) if isinstance(x, dict) else x for x in val]
                )
            else:
                setattr(self, key, DictObj(val) if isinstance(val, dict) else val)


@pytest.fixture(scope="session")
def test_data_job_parameters(
    databricks_path,
    data_lake_path,
    timestamp_factory,
    worker_id,
    test_files_folder_path,
) -> CalculatorArgs:
    return DictObj(
        {
            "data_storage_account_name": "foo",
            "data_storage_account_key": "foo",
            "wholesale_container_path": f"{data_lake_path}",
            "process_results_path": f"{data_lake_path}/{worker_id}/results",
            "batch_id": executed_batch_id,
            "batch_grid_areas": [805, 806],
            "batch_period_start_datetime": timestamp_factory(
                "2018-01-01T22:00:00.000Z"
            ),
            "batch_period_end_datetime": timestamp_factory("2018-01-03T22:00:00.000Z"),
            "time_zone": "Europe/Copenhagen",
        }
    )


@pytest.fixture(scope="session")
def executed_calculation_job(
    spark, test_data_job_parameters, test_files_folder_path, data_lake_path
):
    """Execute the calculator job.
    This is the act part of a test in the arrange-act-assert paradigm.
    This act is made as a session-scoped fixture because it is a slow process
    and because lots of assertions can be made and split into seperate tests
    without awaiting the execution in each test."""
    meteringPointsDf = spark.read.csv(
        f"{test_files_folder_path}/MeteringPointsPeriods.csv",
        header=True,
    )
    meteringPointsDf.write.format("delta").mode("overwrite").save(
        f"{data_lake_path}/calculation-input-v2/metering-point-periods"
    )

    timeseriesPointsDf = spark.read.csv(
        f"{test_files_folder_path}/TimeSeriesPoints.csv",
        header=True,
    )
    timeseriesPointsDf.write.format("delta").mode("overwrite").save(
        f"{data_lake_path}/calculation-input-v2/time-series-points"
    )
    _start_calculator(spark, test_data_job_parameters)


def _get_process_manager_parameters(filename):
    """Get the parameters as they are expected to be received from the process manager."""
    with open(filename) as file:
        text = file.read()
        text = text.replace("{batch-id}", "any-guid-id")
        lines = text.splitlines()
        return list(
            filter(lambda line: not line.startswith("#") and len(line) > 0, lines)
        )


def test__get_valid_args_or_throw__when_invoked_with_incorrect_parameters_fails(
    databricks_path,
):
    # Act
    with pytest.raises(SystemExit) as excinfo:
        _get_valid_args_or_throw("--unexpected-arg")
    # Assert
    assert excinfo.value.code == 2


def test__get_valid_args_or_throw__accepts_parameters_from_process_manager(
    data_lake_path, source_path, databricks_path
):
    """
    This test works in tandem with a .NET test ensuring that the calculator job accepts
    the arguments that are provided by the calling process manager.
    """

    # Arrange
    process_manager_parameters = _get_process_manager_parameters(
        f"{source_path}/contracts/internal/calculation-job-parameters-reference.txt"
    )

    command_line_args = [
        "--data-storage-account-name",
        "foo",
        "--data-storage-account-key",
        "foo",
        "--time-zone",
        "Europe/Copenhagen",
        "--log-level",
        "information",
    ]
    command_line_args.extend(process_manager_parameters)

    # Act and Assert
    _get_valid_args_or_throw(command_line_args)


def test__result_is_generated_for_requested_grid_areas(
    spark,
    test_data_job_parameters,
    data_lake_path,
    source_path,
    worker_id,
    executed_calculation_job,
):
    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    result_805 = spark.read.json(
        f"{data_lake_path}/{worker_id}/results/batch_id={executed_batch_id}/grid_area=805"
    )
    result_806 = spark.read.json(
        f"{data_lake_path}/{worker_id}/results/batch_id={executed_batch_id}/grid_area=806"
    )
    assert result_805.count() >= 1, "Calculator job failed to write files"
    assert result_806.count() >= 1, "Calculator job failed to write files"


def test__published_time_series_points_contract_matches_schema_from_input_time_series_points(
    spark: SparkSession, test_files_folder_path, executed_calculation_job
):
    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    input_time_series_points = (
        spark.read.format("csv")
        .schema(time_series_point_schema)
        .option("header", "true")
        .option("mode", "FAILFAST")
        .load(f"{test_files_folder_path}/TimeSeriesPoints.csv")
    )
    # When asserting both that the calculator creates output and it does it with input data that matches
    # the time series points contract from the time-series domain (in the same test), then we can infer that the
    # calculator works with the format of the data published from the time-series domain.
    # NOTE:It is not evident from this test that it uses the same input as the calculator job
    # Apparently nullability is ignored for CSV sources so we have to compare schemas in this slightly odd way
    # See more at https://stackoverflow.com/questions/50609548/compare-schema-ignoring-nullable
    assert all(
        (a.name, a.dataType) == (b.name, b.dataType)
        for a, b in zip(input_time_series_points.schema, time_series_point_schema)
    )


def test__calculator_result_schema_must_match_contract_with_dotnet(
    spark,
    test_data_job_parameters,
    data_lake_path,
    source_path,
    worker_id,
    executed_calculation_job,
):
    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    result_805 = spark.read.json(
        f"{data_lake_path}/{worker_id}/results/batch_id={executed_batch_id}/grid_area=805"
    )
    result_805.printSchema()
    assert_contract_matches_schema(
        f"{source_path}/contracts/internal/calculator-result.json",
        result_805.schema,
    )


def test__quantity_is_with_precision_3(
    spark,
    test_data_job_parameters,
    data_lake_path,
    find_first_file,
    worker_id,
    executed_calculation_job,
):
    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file
    # Assert: Quantity output is a string encoded decimal with precision 3 (number of digits after delimiter)
    # Note that any change or violation may impact consumers that expects exactly this precision from the result
    result_805 = spark.read.json(
        f"{data_lake_path}/{worker_id}/results/batch_id={executed_batch_id}/grid_area=805"
    )
    import re

    assert re.search(r"^\d+\.\d{3}$", result_805.first().quantity)


@pytest.fixture(scope="session")
def calculation_file_paths_contract(source_path):
    with open(f"{source_path}/contracts/calculation-file-paths.yml", "r") as stream:
        return DictObj(yaml.safe_load(stream))


def create_file_path_expression(directory_expression, extension):
    """Create file path regular expression from a directory expression
    and a file extension.
    The remaining base file name can be one or more characters except for forward slash ("/").
    """
    return f"{directory_expression}[^/]+{extension}"


def test__result_file_path_matches_contract(
    spark,
    test_data_job_parameters,
    data_lake_path,
    find_first_file,
    worker_id,
    executed_calculation_job,
    calculation_file_paths_contract,
):
    # Arrange
    contract = calculation_file_paths_contract.result_file
    expected_path_expression = create_file_path_expression(
        contract.directory_expression,
        contract.extension,
    )
    # Act: Executed in fixture executed_calculation_job

    # Assert
    actual_result_file = find_first_file(
        f"{data_lake_path}/{worker_id}",
        f"results/batch_id={executed_batch_id}/grid_area=805/part-*.json",
    )
    assert re.match(expected_path_expression, actual_result_file)


def test__creates_hour_csv_with_expected_columns_names(
    spark,
    test_data_job_parameters,
    data_lake_path,
    executed_calculation_job,
    worker_id,
):
    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/results/basis-data/batch_id={executed_batch_id}/time-series-hour/grid_area=805"
    )
    assert actual.columns == [
        "METERINGPOINTID",
        "TYPEOFMP",
        "STARTDATETIME",
        *[f"ENERGYQUANTITY{i+1}" for i in range(24)],
    ]


def test__creates_quarter_csv_with_expected_columns_names(
    spark, test_data_job_parameters, data_lake_path, executed_calculation_job, worker_id
):
    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/results/basis-data/batch_id={executed_batch_id}/time-series-quarter/grid_area=805"
    )

    assert actual.columns == [
        "METERINGPOINTID",
        "TYPEOFMP",
        "STARTDATETIME",
        *[f"ENERGYQUANTITY{i+1}" for i in range(96)],
    ]


def test__creates_csv_per_grid_area(
    spark, test_data_job_parameters, data_lake_path, executed_calculation_job, worker_id
):
    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    basis_data_805 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/results/basis-data/batch_id={executed_batch_id}/time-series-quarter/grid_area=805"
    )

    basis_data_806 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/results/basis-data/batch_id={executed_batch_id}/time-series-quarter/grid_area=806"
    )

    assert (
        basis_data_805.count() >= 1
    ), "Calculator job failed to write basis data files for grid area 805"

    assert (
        basis_data_806.count() >= 1
    ), "Calculator job failed to write basis data files for grid area 806"


def test__master_data_csv_with_expected_columns_names(
    spark, data_lake_path, executed_calculation_job, worker_id
):
    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/results/master-basis-data/batch_id={executed_batch_id}/grid_area=805"
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


def test__creates_master_data_csv_per_grid_area(
    spark, data_lake_path, executed_calculation_job, worker_id
):
    # Act: Executed in fixture executed_calculation_job

    # Assert
    master_basis_data_805 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/results/master-basis-data/batch_id={executed_batch_id}/grid_area=805"
    )

    master_basis_data_806 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/results/master-basis-data/batch_id={executed_batch_id}/grid_area=806"
    )

    assert (
        master_basis_data_805.count() >= 1
    ), "Calculator job failed to write master basis data files for grid area 805"

    assert (
        master_basis_data_806.count() >= 1
    ), "Calculator job failed to write master basis data files for grid area 806"


def test__master_basis_data_file_matches_contract(
    spark,
    data_lake_path,
    find_first_file,
    worker_id,
    executed_calculation_job,
    calculation_file_paths_contract,
):
    # Arrange
    contract = calculation_file_paths_contract.master_basis_data_file
    expected_path_expression = create_file_path_expression(
        contract.directory_expression,
        contract.extension,
    )
    # Act: Executed in fixture executed_calculation_job

    # Assert
    actual_file_path = find_first_file(
        f"{data_lake_path}/{worker_id}/",
        f"results/master-basis-data/batch_id={executed_batch_id}/grid_area=805/part-*.csv",
    )
    assert re.match(expected_path_expression, actual_file_path)


def test__hourly_basis_data_file_matches_contract(
    spark,
    test_data_job_parameters,
    data_lake_path,
    find_first_file,
    worker_id,
    executed_calculation_job,
    calculation_file_paths_contract,
):
    # Arrange
    contract = calculation_file_paths_contract.time_series_hour_basis_data_file
    expected_path_expression = create_file_path_expression(
        contract.directory_expression,
        contract.extension,
    )

    #     # Act: Executed in fixture executed_calculation_job

    #     # Assert
    actual_file_path = find_first_file(
        f"{data_lake_path}/{worker_id}",
        f"results/basis-data/batch_id={executed_batch_id}/time-series-hour/grid_area=805/part-*.csv",
    )
    assert re.match(expected_path_expression, actual_file_path)


def test__quarterly_basis_data_file_matches_contract(
    spark,
    test_data_job_parameters,
    data_lake_path,
    find_first_file,
    worker_id,
    executed_calculation_job,
    calculation_file_paths_contract,
):
    # Arrange
    contract = calculation_file_paths_contract.time_series_quarter_basis_data_file
    expected_path_expression = create_file_path_expression(
        contract.directory_expression,
        contract.extension,
    )

    # Act: Executed in fixture executed_calculation_job

    # Assert
    actual_file_path = find_first_file(
        f"{data_lake_path}/{worker_id}",
        f"results/basis-data/batch_id={executed_batch_id}/time-series-quarter/grid_area=805/part-*.csv",
    )
    assert re.match(expected_path_expression, actual_file_path)


@patch("package.calculator_job._get_valid_args_or_throw")
@patch("package.calculator_job.islocked")
def test__when_data_lake_is_locked__return_exit_code_3(mock_islocked, mock_args_parser):
    # Arrange
    mock_islocked.return_value = True
    # Act
    with pytest.raises(SystemExit) as excinfo:
        start()
    # Assert
    assert excinfo.value.code == 3
