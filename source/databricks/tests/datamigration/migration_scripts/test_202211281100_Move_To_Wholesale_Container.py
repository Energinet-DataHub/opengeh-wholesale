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
from unittest.mock import patch, call, ANY

from package.datamigration.migration_scripts._202211281100_Move_To_Wholesale_Container import (
    apply,
)

from package.datamigration.migration_script_args import MigrationScriptArgs


# @patch(
#     "package.datamigration.migration_scripts._202211281100_Move_To_Wholesale_Container.DataLakeDirectoryClient"
# )
# def test__apply__directory_client_contructed_with_correct_arguments(
#     mock_directory_client,
# ):

#     # Arrange
#     migration_args = MigrationScriptArgs("", "", None)
#     processes_container = "processes"
#     events_container = "integration-events"
#     results_source_directory = "results"
#     events_source_directory = "events"
#     events_checkpoint_source_directory = "events-checkpoint"

#     result_call = call(ANY, processes_container, results_source_directory, ANY)
#     event_call = call(ANY, events_container, events_source_directory, ANY)
#     events_checkpoint_call = call(
#         ANY, events_container, events_checkpoint_source_directory, ANY
#     )

#     expected_calls = [result_call, event_call, events_checkpoint_call]

#     # Act
#     apply(migration_args)

#     # Assert
#     mock_directory_client.assert_has_calls(expected_calls, any_order=True)


# @patch(
#     "package.datamigration.migration_scripts._202211281100_Move_To_Wholesale_Container.DataLakeDirectoryClient"
# )
# def test__apply__calls_rename_directory_with_correct_arguments(
#     mock_directory_client,
# ):

#     # Arrange
#     migration_args = MigrationScriptArgs("", "", None)
#     mock_directory_client.return_value.exists.return_value = True
#     expected_calls = [
#         call(new_name="wholesale/results"),
#         call(new_name="wholesale/events"),
#         call(new_name="wholesale/events-checkpoint"),
#     ]

#     # Act
#     apply(migration_args)

#     # Assert
#     mock_directory_client.return_value.rename_directory.assert_has_calls(expected_calls)


# @patch(
#     "package.datamigration.migration_scripts._202211281100_Move_To_Wholesale_Container.DataLakeDirectoryClient"
# )
# def test__apply__when_source_directory_not_exist__never_call_rename_directory(
#     mock_directory_client,
# ):

#     # Arrange
#     migration_args = MigrationScriptArgs("", "", None)
#     mock_directory_client.return_value.exists.return_value = False

#     # Act
#     apply(migration_args)

#     # Assert
#     mock_directory_client.return_value.rename_directory.assert_not_called()
