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
from importlib.resources import contents

from package.datamigration.uncommitted_migrations import (
    _get_all_migrations,
    download_committed_migrations,
    get_uncommitted_migrations,
)

from tests.helpers.type_utils import qualname

DUMMY_STORAGE_ACCOUNT_NAME = "my_storage"
DUMMY_STORAGE_KEY = "my_storage"
DUMMY_CONTAINER_NAME = "my_container"


@patch(qualname(_get_all_migrations))
@patch(qualname(download_committed_migrations))
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
    mock_file_manager = Mock()

    # Act
    migrations = get_uncommitted_migrations(mock_file_manager)

    # Assert
    assert len(migrations) == 0


@patch(qualname(_get_all_migrations))
@patch(qualname(download_committed_migrations))
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
    mock_file_manager = Mock()

    # Act
    migrations = get_uncommitted_migrations(mock_file_manager)

    # Assert
    assert len(migrations) == 1


def test__get_all_migrations__returns_some() -> None:
    # Act
    migrations = _get_all_migrations()

    # Assert: This test will fail if there are in fact no migrations
    assert len(migrations) > 0


def test__get_all_migrations__returns_correct_names():
    # Act
    migrations = _get_all_migrations()

    # Assert
    for migration in migrations:
        assert not migration.endswith(".sql")


@patch(qualname(contents))
def test__get_all_migrations__returns_expected_migrations(mock_importlib):
    # Arrange
    mock_importlib.return_value = ["my_migration1.sql", "my_migration2.sql"]
    expected_migrations = ["my_migration1", "my_migration2"]

    # Act
    actual_migrations = _get_all_migrations()

    # Assert
    assert actual_migrations == expected_migrations
