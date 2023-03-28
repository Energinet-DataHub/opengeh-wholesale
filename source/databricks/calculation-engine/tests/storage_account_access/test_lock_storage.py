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
from unittest.mock import patch, MagicMock
from package.storage_account_access.lock_storage import (
    _LOCK_FILE_NAME,
    _get_valid_args_or_throw,
    lock,
    unlock,
)


def test__get_valid_args_or_throw__when_invoked_with_incorrect_parameters__fails():
    # Act and Assert
    with pytest.raises(Exception):
        _get_valid_args_or_throw("--unexpected-arg")


@patch("package.storage_account_access.lock_storage.DataLakeFileManagerFactory")
@patch("package.storage_account_access.lock_storage._get_valid_args_or_throw")
def test__lock__create_file_called_with_correct_name(
    mock_arg_parser, mock_file_manager_factory
):
    # Arrange
    mock_create_file = MagicMock()
    mock_file_manager = MagicMock()
    mock_file_manager.create_file = mock_create_file
    mock_file_manager_factory.create_instance.return_value = mock_file_manager

    # Act
    lock()

    # Assert
    mock_create_file.assert_called_once_with(_LOCK_FILE_NAME)


@patch("package.storage_account_access.lock_storage.DataLakeFileManagerFactory")
@patch("package.storage_account_access.lock_storage._get_valid_args_or_throw")
def test__lock__delete_file_called_with_correct_name(
    mock_arg_parser, mock_file_manager_factory
):
    # Arrange
    mock_delete_file = MagicMock()
    mock_file_manager = MagicMock()
    mock_file_manager.delete_file = mock_delete_file
    mock_file_manager_factory.create_instance.return_value = mock_file_manager

    # Act
    unlock()

    # Assert
    mock_delete_file.assert_called_once_with(_LOCK_FILE_NAME)
