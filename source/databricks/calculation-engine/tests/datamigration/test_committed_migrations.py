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
import csv
from unittest.mock import patch
from io import StringIO

from package.infrastructure import storage_account_access
from package.datamigration.committed_migrations import (
    COMMITTED_MIGRATIONS_FILE_NAME,
    DataLakeFileManager,
    download_committed_migrations,
    upload_committed_migration,
)


@patch.object(storage_account_access, DataLakeFileManager.__name__)
def test__download_committed_migrations__when_file_does_not_exist__returns_empty_list(
    mock_file_manager,
):
    # Arrange
    mock_file_manager.exists_file.return_value = False

    # Act
    migrations = download_committed_migrations(mock_file_manager)

    # Assert
    mock_file_manager.download_csv.assert_not_called()
    assert len(migrations) == 0


@patch.object(storage_account_access, DataLakeFileManager.__name__)
def test__download_committed_migrations__returns_correct_items(
    mock_file_manager,
):
    # Arrange
    migration_name_1 = "my_migration1"
    migration_name_2 = "my_migration2"
    mock_file_manager.download_csv.return_value = [
        [migration_name_1],
        [migration_name_2],
    ]

    # Act
    migrations = download_committed_migrations(mock_file_manager)

    # Assert
    assert migrations[0] == migration_name_1 and migrations[1] == migration_name_2


@patch.object(storage_account_access, DataLakeFileManager.__name__)
def test__download_committed_migrations__when_empty_file__returns_empty_list(
    mock_file_manager,
):
    # Arrange
    mock_file_manager.download_csv.return_value = []

    # Act
    migrations = download_committed_migrations(mock_file_manager)

    # Assert
    assert len(migrations) == 0


@patch.object(storage_account_access, DataLakeFileManager.__name__)
def test__upload_committed_migration__when_migration_state_file_do_not_exist__create_file(
    mock_file_manager,
):
    # Arrange
    mock_file_manager.exists_file.return_value = False
    mock_file_manager.download_csv.return_value = []

    # Act
    upload_committed_migration(mock_file_manager, "dummy")

    # Assert
    mock_file_manager.create_file.assert_called_once_with(
        COMMITTED_MIGRATIONS_FILE_NAME
    )


@patch.object(storage_account_access, DataLakeFileManager.__name__)
def test__upload_committed_migrations__when_migration_state_file_exists__do_not_create_file(
    mock_file_manager,
):
    # Arrange
    mock_file_manager.exists_file.return_value = True
    string_data = "aaa"
    mock_file_manager.download_csv.return_value = create_csv_reader_with_data(
        string_data
    )

    # Act
    upload_committed_migration(mock_file_manager, "dummy")

    # Assert
    mock_file_manager.create_file.assert_not_called()


@patch.object(storage_account_access, DataLakeFileManager.__name__)
def test__upload_committed_migration__when_unexpected_columns_in_csv__raise_exception(
    mock_file_manager,
):
    # Arrange
    mock_file_manager.exists_file.return_value = True
    string_data = "aaa,bbb\r\nccc,ddd"
    mock_file_manager.download_csv.return_value = create_csv_reader_with_data(
        string_data
    )
    migration_name = "my_migration"

    # Act and Assert
    with pytest.raises(Exception):
        upload_committed_migration(mock_file_manager, migration_name)


@patch.object(storage_account_access, DataLakeFileManager.__name__)
def test__upload_committed_migration__when_file_is_empty__dont_try_downloading_csv(
    mock_file_manager,
):
    # Arrange
    mock_file_manager.exists_file.return_value = True
    mock_file_manager.get_file_size.return_value = 0

    # Act
    upload_committed_migration(mock_file_manager, "my_migration")

    # Assert
    mock_file_manager.download_csv.assert_not_called()


@patch.object(storage_account_access, DataLakeFileManager.__name__)
def test__upload_committed_migration__append_data_is_called_with_correct_string(
    mock_file_manager,
):
    # Arrange
    string_data = "aaa\r\n"
    mock_file_manager.download_csv.return_value = create_csv_reader_with_data(
        string_data
    )
    migration_name = "ccc"
    expected_csv_row = f"{migration_name}\r\n"

    # Act
    upload_committed_migration(mock_file_manager, migration_name)

    # Assert
    mock_file_manager.append_data.assert_called_once_with(
        COMMITTED_MIGRATIONS_FILE_NAME, expected_csv_row
    )


def create_csv_reader_with_data(string_data: str) -> None:
    text_stream = StringIO(string_data)
    return csv.reader(text_stream, dialect="excel")
