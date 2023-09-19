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
import pytest
from unittest.mock import patch
from package.calculator_job_args import get_calculator_args
from package.infrastructure.environment_variables import EnvironmentVariable


def _get_job_parameters(filename: str) -> list[str]:
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
    job_parameters = _get_job_parameters(
        f"{contracts_path}/calculation-job-parameters-reference.txt"
    )

    return job_parameters


@pytest.fixture(scope="session")
def dummy_job_command_line_args(dummy_job_parameters: list[str]) -> list[str]:
    return ["dummy_script_name"] + dummy_job_parameters


@pytest.fixture(scope="session")
def dummy_environment_variabes() -> dict:
    return {
        EnvironmentVariable.TIME_ZONE.value: "some_time_zone",
        EnvironmentVariable.DATA_STORAGE_ACCOUNT_NAME.value: "some_storage_account_name",
        EnvironmentVariable.TENANT_ID.value: "550e8400-e29b-41d4-a716-446655440000",
        EnvironmentVariable.SPN_APP_ID.value: "some_spn_app_id",
        EnvironmentVariable.SPN_APP_SECRET.value: "some_spn_app_secret",
    }


def test__get_calculation_args__when_invoked_with_incorrect_parameters_fails(
    dummy_environment_variabes: dict,
) -> None:
    # Act
    with pytest.raises(SystemExit) as excinfo:
        with patch("sys.argv", ["dummy_script", "--unexpected-arg"]):
            with patch.dict("os.environ", dummy_environment_variabes):
                get_calculator_args()

    # Assert
    assert excinfo.value.code == 2


def test__get_calculator_args__accepts_parameters_from_process_manager(
    dummy_environment_variabes: dict,
    dummy_job_command_line_args: list[str],
) -> None:
    """
    This test works in tandem with a .NET test ensuring that the calculator job accepts
    the arguments that are provided by the calling process manager.
    """

    # Arrange

    # Act and Assert
    with patch("sys.argv", dummy_job_command_line_args):
        with patch.dict("os.environ", dummy_environment_variabes):
            get_calculator_args()


def test__get_calculator_args__raise_exception_on_unknown_process_type(
    dummy_environment_variabes: dict, dummy_job_command_line_args: list[str]
) -> None:
    # Arrange
    unknown_process_type = "unknown_process_type"
    pattern = r"--batch-process-type=(\w+)"

    for i, item in enumerate(dummy_job_command_line_args):
        if re.search(pattern, item):
            dummy_job_command_line_args[i] = re.sub(
                pattern, f"--batch-process-type={unknown_process_type}", item
            )
            break

    # Act and Assert
    with patch("sys.argv", dummy_job_command_line_args):
        with patch.dict("os.environ", dummy_environment_variabes):
            with pytest.raises(SystemExit):
                get_calculator_args()


def test__get_calculator_args__when_missing_env_variables__raise_exception(
    dummy_environment_variabes: dict, dummy_job_command_line_args: list[str]
) -> None:
    # Arrange
    with patch("sys.argv", dummy_job_command_line_args):
        for excluded_env_var in dummy_environment_variabes.keys():
            env_variabes_with_one_missing = {
                key: value
                for key, value in dummy_environment_variabes.items()
                if key != excluded_env_var
            }
            with patch.dict("os.environ", env_variabes_with_one_missing):
                # Act and Assert
                with pytest.raises(SystemExit):
                    get_calculator_args()
