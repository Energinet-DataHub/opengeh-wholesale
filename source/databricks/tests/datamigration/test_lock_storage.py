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

import pytest
import unittest
from unittest.mock import patch
from package.datamigration.lock_storage import (
    _LOCK_FILE_NAME,
    _get_valid_args_or_throw,
    lock,
    unlock,
)


def test__get_valid_args_or_throw__when_invoked_with_incorrect_parameters__fails():
    # Act
    with pytest.raises(SystemExit) as excinfo:
        _get_valid_args_or_throw("--unexpected-arg")

    # Assert
    assert excinfo.value.code == 2


def test__get_valid_args_or_throw__when_invoked_with_correct_parameters__succeeds():
    # Arrange
    command_line_args = [
        "--data-storage-account-name",
        "foo",
        "--data-storage-account-key",
        "foo",
        "--wholesale-container-name",
        "foo",
    ]

    # Act and Assert
    _get_valid_args_or_throw(command_line_args)


@patch("package.datamigration.lock_storage.Data_lake_file_manager")
@patch("package.datamigration.lock_storage._get_valid_args_or_throw")
def test__lock__(mock_arg_parser, mock_file_manager):

    # Arrange
    mock_arg_parser.returns_value(["my_name", "my_key", "my_container"])

    # Act
    lock()

    # Assert
    # mock_file_manager.create_file.assert_called_with(_LOCK_FILE_NAME)
    # mock_file_manager.create_file.assert_called_once()
    mock_arg_parser.assert_called_once()
