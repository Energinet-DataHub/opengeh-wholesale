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
from os.path import isfile, join


def _get_valid_args_or_throw():
    p = configargparse.ArgParser(
        description="Performs domain calculations for submitted batches",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    # Infrastructure settings
    p.add("--data-storage-account-name", type=str, required=True)
    p.add("--data-storage-account-key", type=str, required=True)
    p.add("--wholesale-container-name", type=str, required=True)

    args, unknown_args = p.parse_known_args()
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return args


# Further the method must remain parameterless because it will be called from the entry point when deployed.
def start():
    args = _get_valid_args_or_throw()

    committed_migrations = read_migration_state_from_container(
        args.data_storage_account_name,
        args.data_storage_account_key,
        args.wholesale_container_name,
    )

    if committed_migrations:
        print("Committed migrations:")
        print(committed_migrations)
    else:
        print("No committed migrations")

    uncommitted_migrations_count = 0

    # This format is fixed as it is being used by external tools
    print(f"uncommitted_migrations_count={uncommitted_migrations_count}")


def read_migration_state_from_container(
    storage_account_name: str, storage_account_key: str, container_name: str
) -> list[str]:
    try:
        datalake_service_client = DataLakeServiceClient(
            storage_account_name, storage_account_key
        )
        file_system_client = datalake_service_client.get_file_system_client(
            container_name
        )
        file_client = file_system_client.get_file_client("migration_state.csv")
        download = file_client.download_file()
        bytes_data = download.readall()
        string_data = StringIO(bytes_data.decode())
        read_file = csv.reader(string_data, dialect="excel")
        completed_migrations = [row[0] for row in read_file]

        return completed_migrations

    except Exception as ex:
        print("Exception:")
        print(ex)


if __name__ == "__main__":
    start()
