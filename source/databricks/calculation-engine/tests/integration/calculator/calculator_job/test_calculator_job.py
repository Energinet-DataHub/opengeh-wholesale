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

from azure.identity import ClientSecretCredential
from pyspark.sql import SparkSession
import pytest
from unittest.mock import patch, Mock
from package.calculator_job import (
    _get_valid_args_or_throw,
    start,
    _start,
)
from package.schemas import time_series_point_schema


def _get_process_manager_parameters(filename: str) -> list[str]:
    """Get the parameters as they are expected to be received from the process manager."""
    with open(filename) as file:
        text = file.read()
        text = text.replace("{batch-id}", "any-guid-id")
        lines = text.splitlines()
        return list(
            filter(lambda line: not line.startswith("#") and len(line) > 0, lines)
        )


@pytest.fixture(scope="session")
def dummy_job_parameters(contracts_path: str) -> list[str]:
    process_manager_parameters = _get_process_manager_parameters(
        f"{contracts_path}/calculation-job-parameters-reference.txt"
    )

    return process_manager_parameters


def test__get_valid_args_or_throw__when_invoked_with_incorrect_parameters_fails() -> (
    None
):
    # Act
    with pytest.raises(SystemExit) as excinfo:
        _get_valid_args_or_throw(["--unexpected-arg"])
    # Assert
    assert excinfo.value.code == 2


def test__get_valid_args_or_throw__accepts_parameters_from_process_manager(
    dummy_job_parameters: list[str],
) -> None:
    """
    This test works in tandem with a .NET test ensuring that the calculator job accepts
    the arguments that are provided by the calling process manager.
    """

    # Arrange

    # Act and Assert
    _get_valid_args_or_throw(dummy_job_parameters)


def test__published_time_series_points_contract_matches_schema_from_input_time_series_points(
    spark: SparkSession, test_files_folder_path: str, executed_calculation_job: None
) -> None:
    # Act: Calculator job is executed just once per session. See the fixture `executed_calculation_job`

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


@patch("package.calculator_job._get_valid_args_or_throw")
@patch("package.calculator_job.env_vars")
@patch("package.calculator_job.islocked")
def test__when_data_lake_is_locked__return_exit_code_3(
    mock_islocked: Mock,
    mock_env_vars: Mock,
    mock_args_parser: Mock,
) -> None:
    # Arrange
    mock_islocked.return_value = True

    # Act
    with pytest.raises(SystemExit) as excinfo:
        start()
    # Assert
    assert excinfo.value.code == 3


@patch("package.calculator_job.initialize_spark")
@patch("package.calculator_job.islocked")
@patch("package.calculator_job._start_calculator")
def test__start__start_calculator_called_without_exceptions(
    mock_start_calculator: Mock,
    mock_is_locked: Mock,
    mock_init_spark: Mock,
) -> None:
    # Arrange
    mock_is_locked.return_value = False
    dummy_job_args = Mock()

    # Act
    _start(
        "dummy_account",
        ClientSecretCredential("1", "1", "1"),
        "dummy_time_zone",
        dummy_job_args,
    )

    # Assert
    mock_start_calculator.assert_called_once()
