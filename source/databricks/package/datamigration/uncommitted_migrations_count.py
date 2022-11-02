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
from azure.storage.filedatalake import DataLakeServiceClient, FileSystemClient
import csv
import configargparse
from io import StringIO
from os.path import isfile, join

MIGRATION_STATE_FILE_NAME = "migration_state.csv"


def _get_valid_args_or_throw():
    p = configargparse.ArgParser(
        description="Returns number of uncommitted data migrations",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    p.add("--data-storage-account-name", type=str, required=True)
    p.add("--data-storage-account-key", type=str, required=True)
    p.add("--wholesale-container-name", type=str, required=True)
    p.add(
        "--only-validate-args",
        type=bool,
        required=False,
        default=False,
        help="Instruct the script to exit after validating input arguments.",
    )

    args, unknown_args = p.parse_known_args()
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return args


def _get_file_system_client(
    storage_account_name: str, storage_account_key: str, container_name: str
) -> FileSystemClient:
    datalake_service_client = DataLakeServiceClient(
        storage_account_name, storage_account_key
    )
    return datalake_service_client.get_file_system_client(container_name)


def _download_file(
    data_storage_account_name: str,
    data_storage_account_key: str,
    container_name: str,
    file_name: str,
) -> bytes:
    file_system_client = _get_file_system_client(
        data_storage_account_name,
        data_storage_account_key,
        container_name,
    )

    file_client = file_system_client.get_file_client(file_name)
    download = file_client.download_file()
    downloaded_bytes = download.readall()
    return downloaded_bytes


def _read_csv(bytes_data: bytes):
    string_data = StringIO(bytes_data.decode())
    return csv.reader(string_data, dialect="excel")


def _download_committed_migrations(
    storage_account_name: str, storage_account_key: str, container_name: str
) -> list[str]:
    """Download file with migration state from datalake and return a list of aldready committed migrations"""

    downloaded_bytes = _download_file(
        storage_account_name,
        storage_account_key,
        container_name,
        MIGRATION_STATE_FILE_NAME,
    )

    csv_reader = _read_csv(downloaded_bytes)

    committed_migrations = [row[0] for row in csv_reader]

    return committed_migrations


def _get_all_migrations() -> list[str]:
    return []


def _get_uncommitted_migrations_count(
    storage_account_name: str, storage_account_key: str, container_name: str
) -> int:
    """Get the number of migrations that have not yet been committed"""

    committed_migrations = _download_committed_migrations(
        storage_account_name, storage_account_key, container_name
    )

    all_migrations = _get_all_migrations()

    uncommitted_migrations = [
        m for m in all_migrations if m not in committed_migrations
    ]

    return len(uncommitted_migrations)


# This method must remain parameterless because it will be called from the entry point when deployed.
def start():
    args = _get_valid_args_or_throw()
    if args.only_validate_args:
        exit(0)

    uncommitted_migrations_count = _get_uncommitted_migrations_count(
        args.data_storage_account_name,
        args.data_storage_account_key,
        args.wholesale_container_name,
    )

    # This format is fixed as it is being used by external tools
    print(f"uncommitted_migrations_count={uncommitted_migrations_count}")


if __name__ == "__main__":
    start()
