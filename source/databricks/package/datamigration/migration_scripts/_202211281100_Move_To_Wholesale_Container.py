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

from azure.storage.filedatalake import DataLakeDirectoryClient
from package.datamigration.migration_script_args import MigrationScriptArgs

WHOLESALE_CONTAINER_NAME = "wholesale"


def apply(args: MigrationScriptArgs) -> None:

    move_process_container_content_to_wholesale_container(
        args.storage_account_url, args.storage_account_key
    )

    move_integration_events_container_content_to_wholesale_container(
        args.storage_account_url, args.storage_account_key
    )


def move_process_container_content_to_wholesale_container(
    storage_account_url: str, storage_account_key: str
) -> None:

    source_container = "processes"
    source_directory = "results"
    destination_directory = source_directory

    move_directory(
        storage_account_url,
        storage_account_key,
        source_container,
        source_directory,
        WHOLESALE_CONTAINER_NAME,
        destination_directory,
    )


def move_integration_events_container_content_to_wholesale_container(
    storage_account_url: str, storage_account_key: str
) -> None:

    source_container = "integration-events"

    # move 'events' folder
    source_directory = "events"
    destination_directory = source_directory

    move_directory(
        storage_account_url,
        storage_account_key,
        source_container,
        source_directory,
        WHOLESALE_CONTAINER_NAME,
        destination_directory,
    )

    # move 'events-checkpoint'
    source_directory = "events-checkpoint"
    destination_directory = source_directory

    move_directory(
        storage_account_url,
        storage_account_key,
        source_container,
        source_directory,
        WHOLESALE_CONTAINER_NAME,
        destination_directory,
    )


def move_directory(
    storage_account_url: str,
    storage_account_key: str,
    source_container: str,
    source_directory: str,
    destination_container: str,
    destination_directory: str,
) -> None:
    directory_client = DataLakeDirectoryClient(
        storage_account_url,
        source_container,
        source_directory,
        storage_account_key,
    )

    if not directory_client.exists():
        source_path = source_container + "/" + source_directory
        print(
            f"Skipping migration ({__file__}). Source directory not found:{source_path}"
        )
        return

    directory_client.rename_directory(
        new_name=destination_container + "/" + destination_directory
    )
