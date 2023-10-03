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


from unittest.mock import patch, Mock

from package.infrastructure.storage_account_access.lock_storage import (
    _LOCK_FILE_NAME,
    lock,
    unlock,
)
from package.infrastructure.storage_account_access.data_lake_file_manager import (
    DataLakeFileManager,
)

from tests.helpers.type_utils import qualname


@patch(qualname(DataLakeFileManager))
@patch("package.infrastructure.storage_account_access.lock_storage.env_vars")
def test__lock__create_file_called_with_correct_name(
    mock_env_vars: Mock, mock_file_manager: Mock
):
    # Arrange
    mock_create_file = Mock()
    mock_file_manager.return_value.create_file = mock_create_file

    # Act
    lock()

    # Assert
    mock_create_file.assert_called_once_with(_LOCK_FILE_NAME)


@patch(qualname(DataLakeFileManager))
@patch("package.infrastructure.storage_account_access.lock_storage.env_vars")
def test__unlock__delete_file_called_with_correct_name(
    mock_env_vars: Mock, mock_file_manager: Mock
):
    # Arrange
    mock_delete_file = Mock()
    mock_file_manager.return_value.delete_file = mock_delete_file

    # Act
    unlock()

    # Assert
    mock_delete_file.assert_called_once_with(_LOCK_FILE_NAME)
