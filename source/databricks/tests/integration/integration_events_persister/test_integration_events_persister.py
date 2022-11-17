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

import os
import shutil
import pytest
from unittest.mock import patch, Mock, MagicMock
from package import integration_events_persister
from package.integration_events_persister_streaming import (
    start,
    _get_valid_args_or_throw,
)
from tests.integration.utils import streaming_job_asserter


@pytest.fixture(scope="session")
def integration_events_persister_tester(spark, data_lake_path):
    event_hub_streaming_simulation_path = (
        f"{data_lake_path}/../integration_events_persister/test_files"
    )

    integration_events_path = f"{data_lake_path}/integration_events"
    integration_events_checkpoint_path = (
        f"{data_lake_path}/integration_events_checkpoint"
    )

    # Remove test folders in order to avoid side effects from previous/other test runs
    if os.path.exists(integration_events_path):
        shutil.rmtree(integration_events_path)
    if os.path.exists(integration_events_checkpoint_path):
        shutil.rmtree(integration_events_checkpoint_path)

    streaming_df = (
        spark.readStream.option("startingOffsets", "earliest")
        .format("json")
        .load(event_hub_streaming_simulation_path)
    )

    return integration_events_persister(
        streaming_df,
        integration_events_path,
        integration_events_checkpoint_path,
    )


@pytest.mark.asyncio
async def test_process_json(parquet_reader, integration_events_persister_tester):
    def verification_function():
        data = parquet_reader("/integration_events")
        return data.count() > 0

    succeeded = streaming_job_asserter(
        integration_events_persister_tester, verification_function
    )
    assert succeeded, "No data was stored in Delta table"


valid_command_line_args = [
    "--data-storage-account-name",
    "any-account-name",
    "--data-storage-account-key",
    "any-key",
    "--event-hub-connectionstring",
    "any-connection-string",
    "--integration-events-path",
    "any-path",
    "--integration-events-checkpoint-path",
    "any-checkpoint-path",
    "--log-level",
    "information",
]


def test__get_valid_args_or_throw__accepts_correct_parameters(
    data_lake_path, source_path, databricks_path
):
    # Act and Assert
    _get_valid_args_or_throw(valid_command_line_args)


def test__get_valid_args_or_throw__when_missing_args__raises_exception(
    data_lake_path, source_path, databricks_path
):
    # Act
    with pytest.raises(SystemExit) as excinfo:
        _get_valid_args_or_throw([])

    # Assert
    assert excinfo.value.code == 2


# TODO: toggle comments below when islocked functionality works
# @patch("package.integration_events_persister_streaming._get_valid_args_or_throw")
# @patch("package.integration_events_persister_streaming.islocked")
# def test__when_data_lake_is_locked__return_exit_code_3(mock_islocked, mock_args_parser):
#     # Arrange
#     mock_islocked.return_value = True

#     # Act
#     with pytest.raises(SystemExit) as excinfo:
#         start()

#     # Assert
#     assert excinfo.value.code == 3
