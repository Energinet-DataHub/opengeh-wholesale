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
from unittest.mock import patch, ANY

from package.datamigration.migration import _migrate_data_lake, _get_valid_args_or_throw

from package.datamigration.uncommitted_migrations import (
    _get_all_migrations,
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
    ]

    # Act and Assert
    _get_valid_args_or_throw(command_line_args)


@patch("package.datamigration.migration.initialize_spark")
@patch("package.datamigration.migration.DataLakeFileManager")
@patch("package.datamigration.migration.get_uncommitted_migrations")
@patch("package.datamigration.migration.upload_committed_migration")
def test__migrate_datalake__when_script_not_found__raise_exception(
    mock_upload_committed_migration,
    mock_uncommitted_migrations,
    mock_file_manager,
    mock_spark,
):
    # Arrange
    mock_uncommitted_migrations.return_value = ["not_a_module"]

    # Act and Assert
    with pytest.raises(Exception):
        _migrate_data_lake("dummy_storage_name", "dummy_storage_key")


@patch("package.datamigration.migration.initialize_spark")
@patch("package.datamigration.migration.DataLakeFileManager")
@patch("package.datamigration.migration.get_uncommitted_migrations")
@patch("package.datamigration.migration.upload_committed_migration")
def test__migrate_datalake__all_migration_scripts_can_be_imported(
    mock_upload_committed_migration,
    mock_uncommitted_migrations,
    mock_file_manager,
    mock_spark,
):
    # Arrange
    all_migrations = _get_all_migrations()
    mock_uncommitted_migrations.return_value = all_migrations

    # Act and Assert (if the module cannot be imported or if the signature is incorrect, an exception will be raise)
    _migrate_data_lake("dummy_storage_name", "dummy_storage_key")


@patch("package.datamigration.migration.initialize_spark")
@patch("package.datamigration.migration.DataLakeFileManager")
@patch("package.datamigration.migration.get_uncommitted_migrations")
@patch("package.datamigration.migration.upload_committed_migration")
def test__migrate_datalake__upload_called_with_correct_name(
    mock_upload_committed_migration,
    mock_uncommitted_migrations,
    mock_file_manager,
    mock_spark,
):
    # Arrange
    all_migrations = _get_all_migrations()
    mock_uncommitted_migrations.return_value = all_migrations

    # Act
    _migrate_data_lake("dummy_storage_name", "dummy_storage_key")

    # Assert
    for name in all_migrations:
        mock_upload_committed_migration.assert_called_once_with(ANY, name)
