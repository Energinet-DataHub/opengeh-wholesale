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

from io import StringIO
import pytest
import unittest
from unittest.mock import patch

from package.datamigration.committed_migrations import (
    download_committed_migrations,
)

from package.datamigration.migration import (
    _apply_migrations,
)


# @patch("package.datamigration.committed_migrations.DataLakeFileManager")
def test___apply_migrations__when_script_not_found__raise_exception(
    # mock_file_manager,
):

    # Arrange
    unexpected_module = "not_a_module"
    # mock_file_manager.download_csv.return_value = []

    # Act
    _apply_migrations([unexpected_module])

    # migrations = download_committed_migrations(mock_file_manager)

    # # Assert
    # assert len(migrations) == 0


def test___apply_migrations_(
    # mock_file_manager,
):

    # Arrange
    unexpected_module = "dummy_migration_1"
    # mock_file_manager.download_csv.return_value = []

    # Act
    _apply_migrations([unexpected_module])

    # migrations = download_committed_migrations(mock_file_manager)

    # # Assert
    # assert len(migrations) == 0
