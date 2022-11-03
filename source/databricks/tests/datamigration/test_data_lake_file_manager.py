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
from unittest.mock import patch, Mock

from package.datamigration.data_lake_file_manager import (
    _get_file_system_client,
    download_file,
    foo,
)

# from package.datamigration.uncommitted_migrations_count import bar


@patch("package.datamigration.data_lake_file_manager.DataLakeServiceClient")
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


# @patch("package.datamigration.data_lake_file_manager.download_csv")
# def test__download_committed_migrations__returns_correct_items(mock_download_csv):
#     # Arrange
#     migration_name_1 = "my_migration1"
#     migration_name_2 = "my_migration2"
#     mock_download_csv.return_value = iter(
#         [[migration_name_1], [migration_name_1]]
#     )  # download_csv returns an iterator

#     # Act
#     migrations = _download_committed_migrations("", "", "")

#     # Assert
#     assert migrations[0] == migration_name_1 and migrations[1] == migration_name_2


# @patch("package.datamigration.data_lake_file_manager.download_csv")
# def test__download_committed_migrations__when_no_migrations__returns_empty_list(
#     mock_download_csv,
# ):
#     # Arrange
#     mock_download_csv.return_value = iter([])

#     # Act
#     migrations = _download_committed_migrations("", "", "")

#     # Assert
#     assert len(migrations) == 0


def test__XXX(
    # mock_obj,
):
    # Arrange
    # migration_name_1 = "my_migration1"
    # migration_name_2 = "my_migration2"
    # mock_download_csv.return_value = [
    #     [migration_name_1],
    #     [migration_name_2],
    # ]
    with patch("package.datamigration.uncommitted_migrations_count.bar") as mock_obj:
        mock_obj.return_value = "mocked"

        # Act
        # migrations = _download_committed_migrations("", "", "")
        # bar = download_csv("", "", "", "")
        # bar = _download_committed_migrations("", "", "")
        print(foo())

    assert False
