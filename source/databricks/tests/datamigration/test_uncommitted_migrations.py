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
from unittest.mock import patch
from package.datamigration.data_lake_file_manager import DataLakeFileManager

from package.datamigration.uncommitted_migrations import (
    _get_valid_args_or_throw,
    _get_all_migrations,
    get_uncommitted_migrations,
)


DUMMY_STORAGE_ACCOUNT_NAME = "my_storage"
DUMMY_STORAGE_KEY = "my_storage"
DUMMY_CONTAINER_NAME = "my_container"


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


@patch("package.datamigration.uncommitted_migrations._get_all_migrations")
@patch("package.datamigration.uncommitted_migrations.download_committed_migrations")
def test__get_uncommitted_migrations_count__when_no_migration_needed__returns_0(
    mock_download_committed_migrations,
    mock_get_all_migrations,
):
    # Arrange
    migration_name_1 = "my_migration1"
    migration_name_2 = "my_migration2"
    mock_download_committed_migrations.return_value = [
        migration_name_1,
        migration_name_2,
    ]
    mock_get_all_migrations.return_value = [migration_name_1, migration_name_2]

    file_manager = DataLakeFileManager(
        DUMMY_STORAGE_ACCOUNT_NAME, DUMMY_STORAGE_KEY, DUMMY_CONTAINER_NAME
    )

    # Act
    migrations = get_uncommitted_migrations(file_manager)

    # Assert
    assert len(migrations) == 0


@patch("package.datamigration.uncommitted_migrations._get_all_migrations")
@patch("package.datamigration.uncommitted_migrations.download_committed_migrations")
def test__get_uncommitted_migrations_count__when_one_migration_needed__returns_1(
    mock_download_committed_migrations,
    mock_get_all_migrations,
):
    # Arrange
    migration_name_1 = "my_migration1"
    migration_name_2 = "my_migration2"
    mock_download_committed_migrations.return_value = [migration_name_1]
    mock_get_all_migrations.return_value = [
        migration_name_1,
        migration_name_2,
    ]

    file_manager = DataLakeFileManager(
        DUMMY_STORAGE_ACCOUNT_NAME, DUMMY_STORAGE_KEY, DUMMY_CONTAINER_NAME
    )

    # Act
    migrations = get_uncommitted_migrations(file_manager)

    # Assert
    assert len(migrations) == 1


@patch("package.datamigration.uncommitted_migrations.listdir")
def test__get_all_migrations__returns_correct_names(mock_listdir):
    # Arrange
    expected_migrations = ["my_migration1", "my_migration2"]

    listdir_result = [
        f"/myfolder/mysubfolder/{expected_migrations[0]}.py",
        f"/myfolder/mysubfolder/{expected_migrations[0]}.csv",  # not good
        "/myfolder/mysubfolder/my_file.pyd",  # not good
        f"/myfolder/mysubfolder/{expected_migrations[1]}.py",
        "/myfolder/mysubfolder/my_file_py.txt",  # not good
    ]

    mock_listdir.return_value = listdir_result

    # Act
    migrations = _get_all_migrations()

    # Assert
    assert migrations == expected_migrations
