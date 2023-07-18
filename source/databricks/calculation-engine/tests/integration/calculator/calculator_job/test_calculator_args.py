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
from package.calculator_args import _get_valid_args_or_throw


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
    process_manager_parameters = _get_job_parameters(
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


def test__get_valid_args_or_throw__raise_exception_on_unknown_process_type(dummy_job_parameters: list[str]) -> None:

    # Arrange
    unknown_process_type = "unknown_process_type"
    pattern = r'--batch-process-type=(\w+)'

    for i, item in enumerate(dummy_job_parameters):
        if re.search(pattern, item):
            dummy_job_parameters[i] = re.sub(pattern, f'--batch-process-type={unknown_process_type}', item)
            break

    with pytest.raises(SystemExit):
        _get_valid_args_or_throw(dummy_job_parameters)
