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

from unittest.mock import ANY, patch, call, Mock

import pytest
from package.datamigration.migration import (
    _migrate_data_lake,
    initialize_spark,
    DataLakeFileManager,
    get_uncommitted_migrations,
    upload_committed_migration,
    _apply_migration,
)
from package.datamigration.uncommitted_migrations import _get_all_migrations

from tests.helpers.type_utils import qualname


@patch(qualname(initialize_spark))
@patch(qualname(get_uncommitted_migrations))
@patch(qualname(upload_committed_migration))
def test__migrate_datalake__when_script_not_found__raise_exception(
    mock_upload_committed_migration,
    mock_uncommitted_migrations,
    mock_spark,
):
    # Arrange
    mock_uncommitted_migrations.return_value = ["not_a_module"]
    mock_credential = Mock()

    # Act and Assert
    with pytest.raises(Exception):
        _migrate_data_lake("dummy_storage_name", mock_credential)


@patch(qualname(initialize_spark))
@patch(qualname(DataLakeFileManager))
@patch(qualname(get_uncommitted_migrations))
@patch(qualname(upload_committed_migration))
@patch(qualname(_apply_migration))
def test__migrate_datalake__upload_called_with_correct_name(
    mock_apply_migration,
    mock_upload_committed_migration,
    mock_uncommitted_migrations,
    mock_file_manager,
    mock_spark,
):
    # Arrange
    mock_credential = Mock()
    all_migrations = _get_all_migrations()
    mock_uncommitted_migrations.return_value = all_migrations

    calls = []
    for name in all_migrations:
        calls.append(call(ANY, name))

    # Act
    _migrate_data_lake("dummy_storage_name", mock_credential)

    # Assert
    mock_upload_committed_migration.assert_has_calls(calls)
