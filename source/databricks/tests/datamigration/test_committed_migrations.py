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

from package.datamigration.committed_migrations import download_committed_migrations


@patch("package.datamigration.committed_migrations.DataLakeFileManager")
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
    assert migrations[0] == migration_name_1
    assert migrations[1] == migration_name_2


@patch("package.datamigration.committed_migrations.DataLakeFileManager")
def test__download_committed_migrations__when_empty_file__returns_empty_list(
    mock_file_manager,
):
    # Arrange
    mock_file_manager.download_csv.return_value = []

    # Act
    migrations = download_committed_migrations(mock_file_manager)

    # Assert
    assert len(migrations) == 0
