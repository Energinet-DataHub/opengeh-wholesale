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

from os import path
from shutil import rmtree
from pyspark.sql import SparkSession
import pytest
from unittest.mock import patch, Mock
from tests.contract_utils import assert_contract_matches_schema
from package.calculator_job import (
    _get_valid_args_or_throw,
    _start_calculator,
    start,
    _start,
)
from package.calculator_args import CalculatorArgs
from package.codelists import TimeSeriesType, Grouping
import package.infrastructure as infra
from package.schemas import time_series_point_schema, metering_point_period_schema
from tests.helpers.file_utils import find_file
from tests.helpers.assert_calculation_file_path import (
    CalculationFileType,
    assert_file_path_match_contract,
)
from typing import Callable, Optional
from datetime import datetime


executed_batch_id = "0b15a420-9fc8-409a-a169-fbd49479d718"
energy_supplier_gln_a = "8100000000108"
energy_supplier_gln_b = "8100000000109"
balance_responsible_party_gln_a = "1"


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
    data_lake_path: str,
    timestamp_factory: Callable[[str], Optional[datetime]],
    worker_id: str,
) -> CalculatorArgs:
    return DictObj(
        {
            "data_storage_account_name": "foo",
            "data_storage_account_key": "foo",
            "wholesale_container_path": f"{data_lake_path}/{worker_id}",
            "batch_id": executed_batch_id,
            "batch_grid_areas": [805, 806],
            "batch_period_start_datetime": timestamp_factory(
                "2018-01-01T23:00:00.000Z"
            ),
            "batch_period_end_datetime": timestamp_factory("2018-01-03T23:00:00.000Z"),
            "time_zone": "Europe/Copenhagen",
        }
    )


@pytest.fixture(scope="session")
def executed_calculation_job(
    spark: SparkSession,
    test_data_job_parameters: CalculatorArgs,
    test_files_folder_path: str,
    data_lake_path: str,
    worker_id: str,
) -> None:
    """Execute the calculator job.
    This is the act part of a test in the arrange-act-assert paradigm.
    This act is made as a session-scoped fixture because it is a slow process
    and because lots of assertions can be made and split into seperate tests
    without awaiting the execution in each test."""

    output_path = f"{data_lake_path}/{worker_id}/{infra.OUTPUT_FOLDER}"

    if path.isdir(output_path):
        # Since we are appending the result dataframes we must ensure that the path is removed before executing the tests
        rmtree(output_path)

    metering_points_df = spark.read.csv(
        f"{test_files_folder_path}/MeteringPointsPeriods.csv",
        header=True,
        schema=metering_point_period_schema,
    )
    metering_points_df.write.format("delta").save(
        f"{data_lake_path}/{worker_id}/calculation-input-v2/metering-point-periods",
        mode="overwrite",
    )
    timeseries_points_df = spark.read.csv(
        f"{test_files_folder_path}/TimeSeriesPoints.csv",
        header=True,
        schema=time_series_point_schema,
    )

    timeseries_points_df.write.format("delta").save(
        f"{data_lake_path}/{worker_id}/calculation-input-v2/time-series-points",
        mode="overwrite",
    )

    _start_calculator(spark, test_data_job_parameters)


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
def dummy_job_parameters(source_path: str) -> list[str]:
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

    return command_line_args


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


def test__result_is_generated_for_requested_grid_areas(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    expected_ga_gln_type = [
        ["805", None, None, TimeSeriesType.PRODUCTION, Grouping.total_ga],
        ["806", None, None, TimeSeriesType.PRODUCTION, Grouping.total_ga],
        [
            "805",
            energy_supplier_gln_a,
            None,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            Grouping.es_per_ga,
        ],
        [
            "806",
            energy_supplier_gln_a,
            None,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            Grouping.es_per_ga,
        ],
        [
            "805",
            energy_supplier_gln_b,
            None,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            Grouping.es_per_ga,
        ],
        [
            "806",
            energy_supplier_gln_b,
            None,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            Grouping.es_per_ga,
        ],  # ga brp es
        [
            "805",
            energy_supplier_gln_a,
            balance_responsible_party_gln_a,
            TimeSeriesType.PRODUCTION,
            Grouping.es_per_brp_per_ga,
        ],
        [
            "806",
            energy_supplier_gln_a,
            balance_responsible_party_gln_a,
            TimeSeriesType.PRODUCTION,
            Grouping.es_per_brp_per_ga,
        ],
    ]

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    for (
        grid_area,
        energy_supplier_gln,
        balance_responsible_party_gln,
        time_series_type,
        grouping,
    ) in expected_ga_gln_type:
        result_path = infra.get_result_file_relative_path(
            executed_batch_id,
            grid_area,
            energy_supplier_gln,
            balance_responsible_party_gln,
            time_series_type,
            grouping,
        )
        result = spark.read.json(f"{data_lake_path}/{worker_id}/{result_path}")
        assert result.count() >= 1, "Calculator job failed to write files"


def test__published_time_series_points_contract_matches_schema_from_input_time_series_points(
    spark: SparkSession, test_files_folder_path: str, executed_calculation_job: None
) -> None:
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


def test__calculator_result_total_ga_schema_must_match_contract_with_dotnet(
    spark: SparkSession,
    data_lake_path: str,
    source_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    result_relative_path = infra.get_result_file_relative_path(
        executed_batch_id,
        "805",
        None,
        None,
        TimeSeriesType.PRODUCTION,
        Grouping.total_ga,
    )
    result_path = f"{data_lake_path}/{worker_id}/{result_relative_path}"

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    result_805 = spark.read.json(result_path)

    assert_contract_matches_schema(
        f"{source_path}/contracts/internal/calculator-result.json",
        result_805.schema,
    )


def test__calculator_result_es_per_ga_schema_must_match_contract_with_dotnet(
    spark: SparkSession,
    data_lake_path: str,
    source_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    result_relative_path = infra.get_result_file_relative_path(
        executed_batch_id,
        "805",
        energy_supplier_gln_a,
        None,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        Grouping.es_per_ga,
    )
    result_path = f"{data_lake_path}/{worker_id}/{result_relative_path}"

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    result_805 = spark.read.json(result_path)

    assert_contract_matches_schema(
        f"{source_path}/contracts/internal/calculator-result.json",
        result_805.schema,
    )


def test__quantity_is_with_precision_3(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    result_relative_path_production = infra.get_result_file_relative_path(
        executed_batch_id,
        "805",
        None,
        None,
        TimeSeriesType.PRODUCTION,
        Grouping.total_ga,
    )

    result_relative_path_non_profiled_consumption = infra.get_result_file_relative_path(
        executed_batch_id,
        "805",
        energy_supplier_gln_a,
        None,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        Grouping.es_per_ga,
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file
    # Assert: Quantity output is a string encoded decimal with precision 3 (number of digits after delimiter)
    # Note that any change or violation may impact consumers that expects exactly this precision from the result
    result_production = spark.read.json(
        f"{data_lake_path}/{worker_id}/{result_relative_path_production}"
    )
    result_non_profiled_consumption = spark.read.json(
        f"{data_lake_path}/{worker_id}/{result_relative_path_non_profiled_consumption}"
    )

    import re

    assert re.search(r"^\d+\.\d{3}$", result_production.first().quantity)
    assert re.search(r"^\d+\.\d{3}$", result_non_profiled_consumption.first().quantity)


def test__result_file_has_correct_expected_number_of_rows_for_consumption(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    result_relative_path = infra.get_result_file_relative_path(
        executed_batch_id,
        "806",
        energy_supplier_gln_a,
        None,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        Grouping.es_per_ga,
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    consumption_806 = spark.read.json(
        f"{data_lake_path}/{worker_id}/{result_relative_path}"
    )
    assert consumption_806.count() == 192  # period is from 01-01 -> 01-03


def test__result_file_has_correct_expected_number_of_rows_for_production(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    result_relative_path = infra.get_result_file_relative_path(
        executed_batch_id,
        "806",
        None,
        None,
        TimeSeriesType.PRODUCTION,
        Grouping.total_ga,
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    production_806 = spark.read.json(
        f"{data_lake_path}/{worker_id}/{result_relative_path}"
    )
    assert production_806.count() == 192  # period is from 01-01 -> 01-03


def test__creates_hour_csv_with_expected_columns_names(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    basis_data_relative_path = infra.get_time_series_hour_for_total_ga_relative_path(
        executed_batch_id, "805"
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_relative_path}"
    )
    assert actual.columns == [
        "METERINGPOINTID",
        "TYPEOFMP",
        "STARTDATETIME",
        *[f"ENERGYQUANTITY{i+1}" for i in range(24)],
    ]


def test__creates_quarter_csv_with_expected_columns_names(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    relative_path = infra.get_time_series_quarter_for_total_ga_relative_path(
        executed_batch_id, "805"
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{relative_path}"
    )

    assert actual.columns == [
        "METERINGPOINTID",
        "TYPEOFMP",
        "STARTDATETIME",
        *[f"ENERGYQUANTITY{i+1}" for i in range(96)],
    ]


def test__creates_quarter_csv_per_grid_area(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    basis_data_relative_path_805 = infra.get_time_series_quarter_for_total_ga_relative_path(
        executed_batch_id, "805"
    )
    basis_data_relative_path_806 = infra.get_time_series_quarter_for_total_ga_relative_path(
        executed_batch_id, "806"
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    basis_data_805 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_relative_path_805}"
    )

    basis_data_806 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_relative_path_806}"
    )

    assert (
        basis_data_805.count() >= 1
    ), "Calculator job failed to write basis data files for grid area 805"

    assert (
        basis_data_806.count() >= 1
    ), "Calculator job failed to write basis data files for grid area 806"


def test__creates_hour_csv_per_grid_area(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    basis_data_relative_path_805 = infra.get_time_series_hour_for_total_ga_relative_path(
        executed_batch_id, "805"
    )
    basis_data_relative_path_806 = infra.get_time_series_hour_for_total_ga_relative_path(
        executed_batch_id, "806"
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    basis_data_805 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_relative_path_805}"
    )

    basis_data_806 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_relative_path_806}"
    )

    assert (
        basis_data_805.count() >= 1
    ), "Calculator job failed to write basis data files for grid area 805"

    assert (
        basis_data_806.count() >= 1
    ), "Calculator job failed to write basis data files for grid area 806"


def test__master_data_csv_with_expected_columns_names(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    basis_data_path = infra.get_master_basis_data_for_total_ga_relative_path(
        executed_batch_id, "805"
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_path}"
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
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    basis_data_path_805 = infra.get_master_basis_data_for_total_ga_relative_path(
        executed_batch_id, "805"
    )
    basis_data_path_806 = infra.get_master_basis_data_for_total_ga_relative_path(
        executed_batch_id, "806"
    )

    # Act: Executed in fixture executed_calculation_job

    # Assert
    master_basis_data_805 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_path_805}"
    )

    master_basis_data_806 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_path_806}"
    )

    assert (
        master_basis_data_805.count() >= 1
    ), "Calculator job failed to write master basis data files for grid area 805"

    assert (
        master_basis_data_806.count() >= 1
    ), "Calculator job failed to write master basis data files for grid area 806"


def test__master_basis_data_for_total_ga_filepath_matches_contract(
    data_lake_path: str,
    worker_id: str,
    contracts_path: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    master_basis_data_path = infra.get_master_basis_data_for_total_ga_relative_path(
        executed_batch_id, "805"
    )

    # Act: Executed in fixture executed_calculation_job

    # Assert
    actual_file_path = find_file(
        f"{data_lake_path}/{worker_id}/",
        f"{master_basis_data_path}/part-*.csv",
    )
    assert_file_path_match_contract(
        contracts_path, actual_file_path, CalculationFileType.MasterBasisDataForTotalGa
    )


def test__hourly_basis_data_for_total_ga_filepath_matches_contract(
    data_lake_path: str,
    worker_id: str,
    contracts_path: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    relative_output_path = infra.get_time_series_hour_for_total_ga_relative_path(
        executed_batch_id, "805"
    )

    # Act: Executed in fixture executed_calculation_job

    # Assert
    actual_file_path = find_file(
        f"{data_lake_path}/{worker_id}", f"{relative_output_path}/part-*.csv"
    )
    assert_file_path_match_contract(
        contracts_path, actual_file_path, CalculationFileType.TimeSeriesHourBasisData
    )


def test__quarterly_basis_data_for_total_ga_filepath_matches_contract(
    data_lake_path: str,
    worker_id: str,
    contracts_path: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    relative_output_path = infra.get_time_series_quarter_for_total_ga_relative_path(
        executed_batch_id, "805"
    )

    # Act: Executed in fixture executed_calculation_job

    # Assert
    actual_file_path = find_file(
        f"{data_lake_path}/{worker_id}", f"{relative_output_path}/part-*.csv"
    )
    assert_file_path_match_contract(
        contracts_path, actual_file_path, CalculationFileType.TimeSeriesQuarterBasisDataForTotalGa
    )


def test__master_basis_data_for_es_per_ga_filepath_matches_contract(
    data_lake_path: str,
    worker_id: str,
    contracts_path: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    master_basis_data_path = infra.get_master_basis_data_for_es_per_ga_relative_path(
        executed_batch_id, "805", energy_supplier_gln_a
    )

    # Act: Executed in fixture executed_calculation_job

    # Assert
    actual_file_path = find_file(
        f"{data_lake_path}/{worker_id}/",
        f"{master_basis_data_path}/part-*.csv",
    )
    assert_file_path_match_contract(
        contracts_path, actual_file_path, CalculationFileType.MasterBasisDataForEsPerGa
    )


def test__hourly_basis_data_for_es_per_ga_filepath_matches_contract(
    data_lake_path: str,
    worker_id: str,
    contracts_path: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    relative_output_path = infra.get_time_series_hour_for_es_per_ga_relative_path(
        executed_batch_id, "805", energy_supplier_gln_a
    )

    # Act: Executed in fixture executed_calculation_job

    # Assert
    actual_file_path = find_file(
        f"{data_lake_path}/{worker_id}", f"{relative_output_path}/part-*.csv"
    )
    assert_file_path_match_contract(
        contracts_path, actual_file_path, CalculationFileType.TimeSeriesHourBasisDataForEsPerGa
    )


def test__quarterly_basis_data_for_es_per_ga_filepath_matches_contract(
    data_lake_path: str,
    worker_id: str,
    contracts_path: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    relative_output_path = infra.get_time_series_quarter_for_es_per_ga_relative_path(
        executed_batch_id, "805", energy_supplier_gln_a
    )

    # Act: Executed in fixture executed_calculation_job

    # Assert
    actual_file_path = find_file(
        f"{data_lake_path}/{worker_id}", f"{relative_output_path}/part-*.csv"
    )
    assert_file_path_match_contract(
        contracts_path, actual_file_path, CalculationFileType.TimeSeriesQuarterBasisDataForEsPerGa
    )


@patch("package.calculator_job._get_valid_args_or_throw")
@patch("package.calculator_job.islocked")
def test__when_data_lake_is_locked__return_exit_code_3(
    mock_islocked: Mock, mock_args_parser: Mock
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
    dummy_job_parameters: list[str],
) -> None:
    # Arrange
    mock_is_locked.return_value = False

    # Act
    _start(dummy_job_parameters)

    # Assert
    mock_start_calculator.assert_called_once()
