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
import datetime
import re
import pytest
from unittest.mock import patch
from package.calculator_job_args import get_calculator_args
from package.codelists import ProcessType
from package.infrastructure.environment_variables import EnvironmentVariable

DEFAULT_BATCH_ID = "the-batch-id"


def _get_job_parameters(filename: str) -> list[str]:
    """Get the parameters as they are expected to be received from the process manager."""
    with open(filename) as file:
        text = file.read()
        text = text.replace("{batch-id}", DEFAULT_BATCH_ID)
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
def dummy_environment_variables() -> dict:
    return {
        EnvironmentVariable.TIME_ZONE.value: "Europe/Copenhagen",
        EnvironmentVariable.DATA_STORAGE_ACCOUNT_NAME.value: "some_storage_account_name",
        EnvironmentVariable.TENANT_ID.value: "550e8400-e29b-41d4-a716-446655440000",
        EnvironmentVariable.SPN_APP_ID.value: "some_spn_app_id",
        EnvironmentVariable.SPN_APP_SECRET.value: "some_spn_app_secret",
    }


class TestWhenInvokedWithIncorrectParameters:
    def test_fails(
        self,
        dummy_environment_variables: dict,
    ) -> None:
        # Act
        with pytest.raises(SystemExit) as excinfo:
            with patch("sys.argv", ["dummy_script", "--unexpected-arg"]):
                with patch.dict("os.environ", dummy_environment_variables):
                    get_calculator_args()

        # Assert
        assert excinfo.value.code == 2


class TestWhenInvokedWithValidParameters:
    def test_parses_parameters_from_contract(
        self,
        dummy_environment_variables: dict,
        dummy_job_command_line_args: list[str],
    ) -> None:
        """
        This test works in tandem with a .NET test ensuring that the calculator job accepts
        the arguments that are provided by the client.
        """

        # Arrange

        # Act
        with patch("sys.argv", dummy_job_command_line_args):
            with patch.dict("os.environ", dummy_environment_variables):
                actual = get_calculator_args()

        # Assert

        # From the contract
        assert actual.batch_id == DEFAULT_BATCH_ID
        assert actual.batch_grid_areas == ["805", "806", "033"]
        assert actual.batch_period_start_datetime == datetime.datetime(2022, 5, 31, 22)
        assert actual.batch_period_end_datetime == datetime.datetime(2022, 6, 1, 22)
        assert actual.batch_process_type == ProcessType.BALANCE_FIXING
        assert actual.batch_execution_time_start == datetime.datetime(2022, 6, 4, 22)

        # TODO BJM: Add test of optional parameter time_series_periods_table_name

        # TODO BJM: Separate test
        # From infrastructure
        assert (
            actual.calculation_input_path
            == "abfss://wholesale@some_storage_account_name.dfs.core.windows.net/calculation_input/"
        )
        assert (
            actual.wholesale_container_path
            == "abfss://wholesale@some_storage_account_name.dfs.core.windows.net/"
        )
        assert actual.time_zone == "Europe/Copenhagen"


class TestWhenUnknownProcessType:
    def test_raise_system_exit_with_non_zero_code(
        self, dummy_environment_variables: dict, dummy_job_command_line_args: list[str]
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

        # Act
        with patch("sys.argv", dummy_job_command_line_args):
            with patch.dict("os.environ", dummy_environment_variables):
                with pytest.raises(SystemExit) as error:
                    get_calculator_args()

        # Assert
        assert error.value.code != 0


class TestWhenMissingEnvVariables:
    def test_raise_system_exit_with_non_zero_code(
        self, dummy_environment_variables: dict, dummy_job_command_line_args: list[str]
    ) -> None:
        # Arrange
        with patch("sys.argv", dummy_job_command_line_args):
            for excluded_env_var in dummy_environment_variables.keys():
                env_variabes_with_one_missing = {
                    key: value
                    for key, value in dummy_environment_variables.items()
                    if key != excluded_env_var
                }
                with patch.dict("os.environ", env_variabes_with_one_missing):
                    # Act
                    with pytest.raises(SystemExit) as error:
                        get_calculator_args()

        # Assert
        assert error.value.code != 0
