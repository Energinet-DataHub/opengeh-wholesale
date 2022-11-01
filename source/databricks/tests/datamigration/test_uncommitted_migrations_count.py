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
import subprocess
import unittest
from unittest.mock import patch, Mock, MagicMock
import mock
from package.datamigration.uncommitted_migrations_count import (
    _get_file_system_client,
    _download_file,
    start,
)


def test_uncommitted_migrations_count_when_invoked_with_incorrect_parameters_fails(
    databricks_path,
):
    exit_code = subprocess.call(
        [
            "python",
            f"{databricks_path}/package/datamigration/uncommitted_migrations_count.py",
            "--unexpected-arg",
        ]
    )

    assert (
        exit_code != 0
    ), "Expected to return non-zero exit code when invoked with bad arguments"


@patch("package.datamigration.uncommitted_migrations_count.DataLakeServiceClient")
def test__get_file_system_client__calls_service_client_with_container_name(
    mock_data_lake_service_client,
):

    # Arrange
    dummy_storage_account_name = "my_storage"
    dummy_storage_key = "my_storage"
    dummy_container_name = "my_container"

    # Act
    _get_file_system_client(
        dummy_storage_account_name, dummy_storage_key, dummy_container_name
    )

    # Assert
    mock_data_lake_service_client.return_value.get_file_system_client.assert_called_once_with(
        dummy_container_name
    )


@patch("package.datamigration.uncommitted_migrations_count.DataLakeServiceClient")
def test__QQQ(
    mock_data_lake_service_client,
):
    # Arrange
    mock_file_system_client = Mock()
    mock_file_client = Mock()
    mock_file_system_client.get_file_client = Mock(return_value=mock_file_client)

    # Act
    start()

    # Assert
    mock_file_client.download_file.assert_called()


@patch("package.datamigration.uncommitted_migrations_count.DataLakeServiceClient")
def test__DDD(
    mock_data_lake_service_client,
):
    # Arrange
    mock_file_system_client = Mock()
    mock_file_client = Mock()
    mock_file_system_client.get_file_client = Mock(return_value=mock_file_client)

    # Act
    _read_migration_state_from_file_system(mock_file_system_client)

    # Assert
    mock_file_client.download_file.assert_called()


@patch("package.datamigration.uncommitted_migrations_count.DataLakeServiceClient")
def test__download_csv__(
    mock_data_lake_service_client,
):
    # Arrange
    mock_file_system_client = Mock()
    mock_file_client = Mock()
    mock_file_system_client.get_file_client = Mock(return_value=mock_file_client)

    # Act
    _read_migration_state_from_file_system(mock_file_system_client)

    # Assert
    mock_file_client.download_file.assert_called()


@patch("package.datamigration.uncommitted_migrations_count.DataLakeServiceClient")
def test__get_committed_migrations__(
    mock_data_lake_service_client,
):
    # Arrange
    mock_file_system_client = Mock()
    mock_file_client = Mock()
    mock_file_system_client.get_file_client = Mock(return_value=mock_file_client)

    # Act
    _read_migration_state_from_file_system(mock_file_system_client)

    # Assert
    mock_file_client.download_file.assert_called()


@patch("package.datamigration.uncommitted_migrations_count.DataLakeServiceClient")
def test__start__(
    mock_data_lake_service_client,
):
    # Arrange
    mock_file_system_client = Mock()
    mock_file_client = Mock()
    mock_file_system_client.get_file_client = Mock(return_value=mock_file_client)

    # Act
    _read_migration_state_from_file_system(mock_file_system_client)

    # Assert
    mock_file_client.download_file.assert_called()


# def test_MMMMM():
#     # Create a mock to return for MyClass.
#     m = MagicMock()
#     # Patch my_method's return value.
#     m.get_file_system_client = Mock(return_value=None)

#     # Patch MyClass. Here, we could use autospec=True for more
#     # complex classes.
#     with patch(
#         "package.datamigration.uncommitted_migrations_count.DataLakeServiceClient",
#         return_value=m,
#     ) as p:
#         value = experiment()

#     assert value == "dummy"


# @patch("package.datamigration.uncommitted_migrations_count.DataLakeServiceClient")
# def test_uncommitted_migration_KKKK(mock_data_lake_service_client):
#     mock_data_lake_service_client.return_value.get_file_system_client.return_value = (
#         None
#     )
#     assert experiment() == "2"


# @patch("package.datamigration.uncommitted_migrations_count.DataLakeServiceClient")
# def test_uncommitted_migration_AAAAA(mock_data_lake_service_client):
#     experiment()
#     mock_data_lake_service_client.return_value.get_file_system_client.assert_called_once_with(
#         ""
#     )


# @patch("package.datamigration.uncommitted_migrations_count.path")
# def test_uncommitted_migration_BBBB(path_mock):
#     path_mock.isfile.return_value = True
#     assert experiment() == "dummy"


# @patch("package.datamigration.uncommitted_migrations_count.os")
# def test_uncommitted_migration_XXXXXXXXXX(os_mock):
#     os_mock.getcwd.return_value = "dummy1"
#     assert experiment() == "dummy"


# @patch("package.datamigration.uncommitted_migrations_count.os")
# def test_uncommitted_migration_YYYYYYY(os_mock):
#     os_mock.path.isfile.return_value = True
#     assert experiment() == "dummy"


# # @patch("package.datamigration.uncommitted_migrations_count.os")
# # def test_uncommitted_migration_XXXXXXXXXX(os_mock):
# #     os_mock.getcwd.return_value = "dummy"
# #     assert experiment() == "dummy"


# # class RmTestCase(unittest.TestCase):
# #     def test_uncommitted_migration_XXXXXXXXXX(self):
# #         with patch("package.datamigration.uncommitted_migrations_count.os") as my_mock:
# #             my_mock.getcwd.return_value = "dummy1"
# #             assert experiment() == "dummy"

# #         # my_mock.assert_called_with("file")
