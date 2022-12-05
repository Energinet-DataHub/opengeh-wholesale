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

import importlib
from unittest.mock import ANY, call, patch
from types import ModuleType
import pytest
from package.datamigration.migration_script_args import MigrationScriptArgs

@patch(
    "package.datamigration.migration_scripts.202212051200_Move_Events_To_Wholesale_Container.DataLakeDirectoryClient"
)
def test__apply__directory_client_contructed_with_correct_arguments(
    mock_directory_client,
) -> None:

    # Arrange
    sut = get_migration_script()
    migration_args = MigrationScriptArgs("", "", None)
    source_container = "integration-events"
    events_source_directory = "events"
    events_checkpoint_source_directory = "events-checkpoint"

    event_call = call(ANY, source_container, events_source_directory, ANY)
    events_checkpoint_call = call(
        ANY, source_container, events_checkpoint_source_directory, ANY
    )

    expected_calls = [event_call, events_checkpoint_call]

    # Act
    sut.apply(migration_args)

    # Assert
    mock_directory_client.assert_has_calls(expected_calls, any_order=True)


@patch(
    "package.datamigration.migration_scripts.202212051200_Move_Events_To_Wholesale_Container.DataLakeDirectoryClient"
)
def test__apply__calls_rename_directory_with_correct_arguments(
    mock_directory_client,
) -> None:

    # Arrange
    sut = get_migration_script()
    migration_args = MigrationScriptArgs("", "", ANY)
    mock_directory_client.return_value.exists.return_value = True
    expected_calls = [
        call(new_name="wholesale/events"),
        call(new_name="wholesale/events-checkpoint"),
    ]

    # Act
    sut.apply(migration_args)

    # Assert
    mock_directory_client.return_value.rename_directory.assert_has_calls(expected_calls)


@patch(
    "package.datamigration.migration_scripts.202212051200_Move_Events_To_Wholesale_Container.DataLakeDirectoryClient"
)
def test__apply__when_source_directory_not_exist__never_call_rename_directory(
    mock_directory_client,
) -> None:

    # Arrange
    sut = get_migration_script()
    migration_args = MigrationScriptArgs("", "", ANY)
    mock_directory_client.return_value.exists.return_value = False

    # Act
    sut.apply(migration_args)

    # Assert
    mock_directory_client.return_value.rename_directory.assert_not_called()


def get_migration_script() -> ModuleType:
    return importlib.import_module(
        "package.datamigration.migration_scripts.202212051200_Move_Events_To_Wholesale_Container"
    )
