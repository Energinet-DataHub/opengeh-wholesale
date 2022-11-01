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

from azure.identity import DefaultAzureCredential
from azure.storage.filedatalake import DataLakeServiceClient
import csv
import configargparse
from io import StringIO
import os
from os.path import isfile, join
from os import path

MIGRATION_STATE_FILE_NAME = "migration_state.csv"


def _get_valid_args_or_throw():
    p = configargparse.ArgParser(
        description="Returns number of uncommitted data migrations",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    p.add("--data-storage-account-name", type=str, required=True)
    p.add("--data-storage-account-key", type=str, required=True)
    p.add("--wholesale-container-name", type=str, required=True)

    args, unknown_args = p.parse_known_args()
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return args


def _get_file_system_client(
    storage_account_name: str, storage_account_key: str, container_name: str
):
    datalake_service_client = DataLakeServiceClient(
        storage_account_name, storage_account_key
    )
    return datalake_service_client.get_file_system_client(container_name)


def _download_file(file_system_client, filename: str) -> bytes:
    file_client = file_system_client.get_file_client()
    download = file_client.download_file()
    downloaded_bytes = download.readall()
    return downloaded_bytes


def _bytes_to_csv(bytes: bytes):
    string_data = StringIO(bytes.decode())
    return csv.reader(string_data, dialect="excel")


def _get_committed_migrations(csv_reader) -> list[str]:
    try:
        completed_migrations = [row[0] for row in csv_reader]
        return completed_migrations

    except Exception as ex:
        print("Exception:")
        print(ex)

    return None


# This method must remain parameterless because it will be called from the entry point when deployed.
def start():
    args = _get_valid_args_or_throw()

    file_system_client = _get_file_system_client(
        args.data_storage_account_name,
        args.data_storage_account_key,
        args.wholesale_container_name,
    )

    downloaded_bytes = _download_file(file_system_client, MIGRATION_STATE_FILE_NAME)

    csv_reader = _bytes_to_csv(downloaded_bytes)

    committed_migrations = _get_committed_migrations(csv_reader)

    if committed_migrations:
        print("Committed migrations:")
        print(committed_migrations)
    else:
        print("No committed migrations")

    uncommitted_migrations_count = 0

    # This format is fixed as it is being used by external tools
    print(f"uncommitted_migrations_count={uncommitted_migrations_count}")


if __name__ == "__main__":
    start()
