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
from datetime import datetime
import re

import pytest
from unittest.mock import patch
from package.calculator_job_args import (
    parse_job_arguments,
    parse_command_line_arguments,
)
from package.codelists import CalculationType
from package.infrastructure.environment_variables import EnvironmentVariable

DEFAULT_CALCULATION_ID = "the-calculation-id"


def _get_contract_parameters(filename: str) -> list[str]:
    """Get the parameters as they are expected to be received from the process manager."""
    with open(filename) as file:
        text = file.read()
        text = text.replace("{calculation-id}", DEFAULT_CALCULATION_ID)
        lines = text.splitlines()
        return list(
            filter(lambda line: not line.startswith("#") and len(line) > 0, lines)
        )


def _substitute_period(
    sys_argv: list[str], period_start_datetime: datetime, period_end_datetime: datetime
) -> list[str]:
    for i, item in enumerate(sys_argv):
        if item.startswith("--period-start-datetime"):
            sys_argv[i] = (
                f"--period-start-datetime={period_start_datetime.strftime('%Y-%m-%dT%H:%M:%SZ')}"
            )
        elif item.startswith("--period-end-datetime"):
            sys_argv[i] = (
                f"--period-end-datetime={period_end_datetime.strftime('%Y-%m-%dT%H:%M:%SZ')}"
            )

    return sys_argv


def _substitute_calculation_type(
    sys_argv: list[str], calculation_type: CalculationType
) -> list[str]:
    for i, item in enumerate(sys_argv):
        if item.startswith("--calculation-type="):
            sys_argv[i] = f"--calculation-type={calculation_type.value}"
            break
    return sys_argv


@pytest.fixture(scope="session")
def contract_parameters(contracts_path: str) -> list[str]:
    job_parameters = _get_contract_parameters(
        f"{contracts_path}/calculation-job-parameters-reference.txt"
    )

    return job_parameters


@pytest.fixture(scope="session")
def sys_argv_from_contract(contract_parameters) -> list[str]:
    return ["dummy_script_name"] + contract_parameters


@pytest.fixture(scope="session")
def job_environment_variables() -> dict:
    return {
        EnvironmentVariable.TIME_ZONE.name: "Europe/Copenhagen",
        EnvironmentVariable.DATA_STORAGE_ACCOUNT_NAME.name: "some_storage_account_name",
        EnvironmentVariable.CALCULATION_INPUT_FOLDER_NAME.name: "input",
        EnvironmentVariable.TENANT_ID.name: "550e8400-e29b-41d4-a716-446655440000",
        EnvironmentVariable.SPN_APP_ID.name: "some_spn_app_id",
        EnvironmentVariable.SPN_APP_SECRET.name: "some_spn_app_secret",
        EnvironmentVariable.QUARTERLY_RESOLUTION_TRANSITION_DATETIME.name: "2023-01-31T23:00:00Z",
    }


class TestWhenInvokedWithIncorrectParameters:
    def test_fails(
        self,
        job_environment_variables: dict,
    ) -> None:
        # Arrange
        with pytest.raises(SystemExit) as excinfo:
            with patch("sys.argv", ["dummy_script", "--unexpected-arg"]):
                with patch.dict("os.environ", job_environment_variables):
                    # Act
                    parse_command_line_arguments()

        # Assert
        assert excinfo.value.code == 2


class TestWhenInvokedWithValidParameters:
    def test_parses_parameters_from_contract(
        self,
        job_environment_variables: dict,
        sys_argv_from_contract,
    ) -> None:
        """
        This test works in tandem with a .NET test ensuring that the calculator job accepts
        the arguments that are provided by the client.
        """
        # Arrange
        with patch("sys.argv", sys_argv_from_contract):
            with patch.dict("os.environ", job_environment_variables):
                command_line_args = parse_command_line_arguments()
                # Act
                actual_args, actual_settings = parse_job_arguments(command_line_args)

        # Assert

        # Assert - Calculation arguments
        assert actual_args.calculation_id == DEFAULT_CALCULATION_ID
        assert actual_args.calculation_grid_areas == ["805", "806", "033"]
        assert actual_args.calculation_period_start_datetime == datetime(
            2022, 5, 31, 22
        )
        assert actual_args.calculation_period_end_datetime == datetime(2022, 6, 1, 22)
        assert actual_args.calculation_type == CalculationType.BALANCE_FIXING
        assert actual_args.calculation_execution_time_start == datetime(2022, 6, 4, 22)
        assert actual_args.time_zone == "Europe/Copenhagen"

        # Assert - Infrastructure settings
        assert (
            actual_settings.calculation_input_path
            == "abfss://wholesale@some_storage_account_name.dfs.core.windows.net/input/"
        )
        assert (
            actual_settings.wholesale_container_path
            == "abfss://wholesale@some_storage_account_name.dfs.core.windows.net/"
        )
        assert (
            actual_settings.calculation_input_path
            == "abfss://wholesale@some_storage_account_name.dfs.core.windows.net/input/"
        )

    def test_parses_optional_time_series_points_table_name(
        self,
        job_environment_variables: dict,
        sys_argv_from_contract,
    ) -> None:
        # Arrange
        expected = "the_time_series_points_table_name"
        sys_argv_from_contract = sys_argv_from_contract + [
            f"--time_series_points_table_name={expected}"
        ]
        with patch("sys.argv", sys_argv_from_contract):
            with patch.dict("os.environ", job_environment_variables):
                command_line_args = parse_command_line_arguments()
                # Act
                _, actual_settings = parse_job_arguments(command_line_args)

        # Assert
        assert actual_settings.time_series_points_table_name == expected

    def test_returns_none_when_time_series_points_table_name_absent(
        self,
        job_environment_variables: dict,
        sys_argv_from_contract,
    ) -> None:
        # Arrange
        with patch("sys.argv", sys_argv_from_contract):
            with patch.dict("os.environ", job_environment_variables):
                command_line_args = parse_command_line_arguments()
                # Act
                _, actual = parse_job_arguments(command_line_args)

        # Assert
        assert actual.time_series_points_table_name is None


class TestWhenUnknownCalculationType:
    def test_raise_system_exit_with_non_zero_code(
        self, job_environment_variables: dict, sys_argv_from_contract
    ) -> None:
        # Arrange
        unknown_calculation_type = "unknown_calculation_type"
        pattern = r"--calculation-type=(\w+)"

        for i, item in enumerate(sys_argv_from_contract):
            if re.search(pattern, item):
                sys_argv_from_contract[i] = re.sub(
                    pattern, f"--calculation-type={unknown_calculation_type}", item
                )
                break

        with patch("sys.argv", sys_argv_from_contract):
            with patch.dict("os.environ", job_environment_variables):
                with pytest.raises(SystemExit) as error:
                    command_line_args = parse_command_line_arguments()
                    # Act
                    parse_job_arguments(command_line_args)

        # Assert
        assert error.value.code != 0


class TestWhenCalculationPeriodIsNotOneCalendarMonth:
    @pytest.mark.parametrize(
        "period_start_datetime, period_end_datetime, calculation_type",
        [
            (start, end, calc_type)
            for start, end in [
                (  # Missing one day at the end
                    datetime(2022, 5, 31, 22),
                    datetime(2022, 6, 29, 22),
                ),
                (  # Missing one day in the beginning
                    datetime(2022, 6, 1, 22),
                    datetime(2022, 6, 30, 22),
                ),
                (datetime(2022, 5, 31, 22), datetime(2022, 7, 31, 22)),  # Two months
                (  # Entering daylights saving time - not ending at midnight
                    datetime(2020, 2, 29, 23, 0),
                    datetime(2020, 3, 31, 23, 0),
                ),
                (  # Exiting daylights saving time - not ending at midnight
                    datetime(2020, 9, 30, 22, 0),
                    datetime(2020, 10, 31, 22, 0),
                ),
            ]
            for calc_type in [
                CalculationType.WHOLESALE_FIXING,
                CalculationType.FIRST_CORRECTION_SETTLEMENT,
                CalculationType.SECOND_CORRECTION_SETTLEMENT,
                CalculationType.THIRD_CORRECTION_SETTLEMENT,
            ]
        ],
    )
    def test_raise_exception_for_wholesale_calculations(
        self,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
        calculation_type: CalculationType,
        job_environment_variables: dict,
        sys_argv_from_contract: list[str],
    ) -> None:
        # Arrange
        sys_argv = sys_argv_from_contract
        sys_argv = _substitute_calculation_type(sys_argv, calculation_type)
        sys_argv = _substitute_period(
            sys_argv, period_start_datetime, period_end_datetime
        )

        with patch("sys.argv", sys_argv):
            with patch.dict("os.environ", job_environment_variables):
                with pytest.raises(Exception) as error:
                    command_line_args = parse_command_line_arguments()
                    # Act
                    parse_job_arguments(command_line_args)

        # Assert
        actual_error_message = str(error.value)
        assert (
            "The calculation period for wholesale calculation types must be a full month"
            in actual_error_message
        )

    @pytest.mark.parametrize(
        "calculation_type",
        [
            CalculationType.AGGREGATION,
            CalculationType.BALANCE_FIXING,
        ],
    )
    def test_parse_arguments_without_exceptions_for_energy_calculations(
        self,
        calculation_type: CalculationType,
        job_environment_variables: dict,
        sys_argv_from_contract: list[str],
    ) -> None:
        # Arrange
        period_start_datetime = datetime(2022, 5, 31, 22)
        period_end_datetime = datetime(2022, 6, 30, 22)
        sys_argv = sys_argv_from_contract
        sys_argv = _substitute_calculation_type(sys_argv, calculation_type)
        sys_argv = _substitute_period(
            sys_argv, period_start_datetime, period_end_datetime
        )

        with patch("sys.argv", sys_argv):
            with patch.dict("os.environ", job_environment_variables):
                command_line_args = parse_command_line_arguments()
                # Act & Assert
                parse_job_arguments(command_line_args)


class TestWhenCalculationPeriodIsOneCalendarMonth:
    @pytest.mark.parametrize(
        "period_start_datetime, period_end_datetime, calculation_type",
        [
            (start, end, calc_type)
            for start, end in [
                (
                    datetime(2022, 5, 31, 22),
                    datetime(2022, 6, 30, 22),
                ),
                (  # New year
                    datetime(2021, 12, 31, 23),
                    datetime(2022, 1, 31, 23),
                ),
                (  # Enter daylight saving time
                    datetime(2022, 2, 28, 23),
                    datetime(2022, 3, 31, 22),
                ),
                (  # Exit daylight saving time
                    datetime(2022, 9, 30, 22),
                    datetime(2022, 10, 31, 23),
                ),
            ]
            for calc_type in CalculationType
        ],
    )
    def test_parse_arguments_without_exceptions(
        self,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
        calculation_type: CalculationType,
        job_environment_variables: dict,
        sys_argv_from_contract: list[str],
    ) -> None:
        # Arrange
        sys_argv = sys_argv_from_contract
        sys_argv = _substitute_calculation_type(sys_argv, calculation_type)
        sys_argv = _substitute_period(
            sys_argv, period_start_datetime, period_end_datetime
        )

        with patch("sys.argv", sys_argv):
            with patch.dict("os.environ", job_environment_variables):
                command_line_args = parse_command_line_arguments()
                # Act & Assert
                parse_job_arguments(command_line_args)


class TestWhenMissingEnvVariables:
    def test_raise_system_exit_with_non_zero_code(
        self, job_environment_variables: dict, sys_argv_from_contract
    ) -> None:
        # Arrange
        with patch("sys.argv", sys_argv_from_contract):
            for excluded_env_var in job_environment_variables.keys():
                env_variables_with_one_missing = {
                    key: value
                    for key, value in job_environment_variables.items()
                    if key != excluded_env_var
                }

                with patch.dict("os.environ", env_variables_with_one_missing):
                    with pytest.raises(ValueError) as error:
                        command_line_args = parse_command_line_arguments()
                        # Act
                        parse_job_arguments(command_line_args)

                assert str(error.value).startswith("Environment variable not found")
