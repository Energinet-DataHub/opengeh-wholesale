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

from package.datamigration.migration import _apply_migrations, _get_valid_args_or_throw

from package.datamigration.uncommitted_migrations import (
    _get_all_migrations,
)

from package.datamigration.committed_migrations import (
    upload_committed_migration,
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
        "--wholesale-container-name",
        "foo",
    ]

    # Act and Assert
    _get_valid_args_or_throw(command_line_args)


@patch("package.datamigration.migration.SparkSession")
def test___apply_migrations__when_script_not_found__raise_exception(
    mock_spark,
):
    # Arrange
    unexpected_module = "not_a_module"

    # Act and Assert
    with pytest.raises(Exception):
        _apply_migrations(mock_spark, [unexpected_module])


@patch("package.datamigration.migration.upload_committed_migration")
@patch("package.datamigration.migration.DataLakeFileManager")
@patch("package.datamigration.migration.SparkSession")
def test___apply_migrations__all_migrations_can_be_imported(
    mock_spark, mock_file_manager, mock_upload_migration
):
    # Arrange
    all_migrations = _get_all_migrations()

    # Act and Assert (if the module cannot be imported or if the signature is incorrect, an exception will be raise)
    _apply_migrations(mock_spark, mock_file_manager, all_migrations)


@patch("package.datamigration.migration.upload_committed_migration")
@patch("package.datamigration.migration.DataLakeFileManager")
@patch("package.datamigration.migration.SparkSession")
def test___apply_migrations__upload_called_with_correct_name(
    mock_spark, mock_file_manager, mock_committed_migration
):
    # Arrange
    all_migrations = _get_all_migrations()

    # Act
    _apply_migrations(mock_spark, mock_file_manager, all_migrations)

    # Assert
    for name in all_migrations:
        mock_committed_migration.assert_called_once_with(mock_file_manager, name)
