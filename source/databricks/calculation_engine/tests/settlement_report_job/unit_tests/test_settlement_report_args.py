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
from datetime import datetime
from unittest.mock import patch

import pytest

from package.settlement_report_job.settlement_report import (
    parse_job_arguments,
    parse_command_line_arguments,
)

from package.settlement_report_job.environment_variables import EnvironmentVariable
from package.settlement_report_job.calculation_type import CalculationType

DEFAULT_REPORT_ID = "the-report-id"


def _get_contract_parameters(filename: str) -> list[str]:
    """Get the parameters as they are expected to be received from the settlement report invoker."""
    with open(filename) as file:
        text = file.read()
        text = text.replace("{report-id}", DEFAULT_REPORT_ID)
        lines = text.splitlines()
        return list(
            filter(lambda line: not line.startswith("#") and len(line) > 0, lines)
        )


def _substitute_period(
    sys_argv: list[str], period_start: datetime, period_end: datetime
) -> list[str]:
    for i, item in enumerate(sys_argv):
        if item.startswith("--period-start"):
            sys_argv[i] = (
                f"--period-start={period_start.strftime('%Y-%m-%dT%H:%M:%SZ')}"
            )
        elif item.startswith("--period-end"):
            sys_argv[i] = f"--period-end={period_end.strftime('%Y-%m-%dT%H:%M:%SZ')}"

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
        f"{contracts_path}/settlement-report-job-parameters-reference.txt"
    )

    return job_parameters


@pytest.fixture(scope="session")
def sys_argv_from_contract(contract_parameters) -> list[str]:
    return ["dummy_script_name"] + contract_parameters


@pytest.fixture(scope="session")
def job_environment_variables() -> dict:
    return {
        EnvironmentVariable.CATALOG_NAME.name: "some_catalog",
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
        This test ensures that the settlement report job accepts
        the arguments that are provided by the client.
        """
        # Arrange
        with patch("sys.argv", sys_argv_from_contract):
            with patch.dict("os.environ", job_environment_variables):
                command_line_args = parse_command_line_arguments()
                # Act
                actual_args = parse_job_arguments(command_line_args)

        # Assert

        # Assert - settlement report arguments
        assert actual_args.report_id == DEFAULT_REPORT_ID
        assert actual_args.period_start == datetime(2022, 5, 31, 22)
        assert actual_args.period_end == datetime(2022, 6, 1, 22)
        assert actual_args.calculation_type == CalculationType.BALANCE_FIXING
        assert actual_args.time_zone == "Europe/Copenhagen"


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
