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

import csv
import configargparse
from os.path import isfile, join
import os
from azure.identity import DefaultAzureCredential
from azure.storage.blob import BlobServiceClient, BlobClient, ContainerClient
from azure.storage.filedatalake import DataLakeServiceClient
import pandas as pd
from io import BytesIO
from csv import reader

# from package import log, db_logging


def _get_valid_args_or_throw():
    p = configargparse.ArgParser(
        description="Performs domain calculations for submitted batches",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    # Infrastructure settings
    p.add(
        "--wholesale-container-url", type=str, required=True
    )  # "https://stdatalakesharedresu001.blob.core.windows.net/processes"
    p.add("--data-storage-account-key", type=str, required=True)

    args, unknown_args = p.parse_known_args()
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return args


# Further the method must remain parameterless because it will be called from the entry point when deployed.
def start():
    args = _get_valid_args_or_throw()
    # log(f"Job arguments: {str(args)}")
    # db_logging.loglevel = args.log_level
    # if args.only_validate_args:
    #     exit(0)

    # read file from blob

    if not isfile(migration_state_file):
        raise Exception(f"No migration state file found: {migration_state_file}")

    with open(migration_state_file) as f:
        completed_migrations = [row.split()[0] for row in f]

    print(f"Committed migrations: {completed_migrations}")

    uncommitted_migrations_count = 0
    # This format is fixed as it is being used by external tools
    print(f"uncommitted_migrations_count={uncommitted_migrations_count}")


def read_migration_state_from_container_1():
    try:
        container_url = (
            "https://stdatalakesharedresu001.blob.core.windows.net/processes"
        )
        key = "pb7Vx/b/aCMKcbp/sMpEZvrgCI/VKyf7kT91fVCTQGQMIPuZJZN6BczyCKM6UT+yJ3cRkKdMGdNy+AStkoiMcw=="

        container_client = ContainerClient.from_container_url(container_url, key)

        blob_client = container_client.get_blob_client("migration_state.csv")
        snapshot_blob = blob_client.create_snapshot()

        # Get the snapshot ID
        print(snapshot_blob.get("snapshot"))
        #   print("\nListing blobs...")

        #   # List the blobs in the container
        #   blob_list = container_client.list_blobs()
        #   for blob in blob_list:
        #      print("\t" + blob.name)

        #  migration_state_file = join(args.process_results_path, "migration_state.csv")
    except Exception as ex:
        print("Exception:")
        print(ex)


def read_migration_state_from_container():
    try:
        container_url = (
            "https://stdatalakesharedresu001.blob.core.windows.net/processes"
        )
        key = "pb7Vx/b/aCMKcbp/sMpEZvrgCI/VKyf7kT91fVCTQGQMIPuZJZN6BczyCKM6UT+yJ3cRkKdMGdNy+AStkoiMcw=="

        datalake_service_client = DataLakeServiceClient(
            "https://stdatalakesharedresu001.dfs.core.windows.net", key
        )
        file_system_client = datalake_service_client.get_file_system_client("processes")
        #        path_list = file_system_client.get_paths()
        #       for path in path_list:
        #          print(path.name + "\n")
        file_client = file_system_client.get_file_client("migration_state.csv")
        download = file_client.download_file()
        # print(download.readall())

        # csv_content = download.readall()
        # data = pd.read_csv(StringIO(csv_content))

        # csv_file = pd.read_csv(BytesIO(download.readall()))

        with BytesIO(download.readall()) as input_file:
            csv_reader = reader(input_file, delimiter=",", quotechar='"')
            for row in csv_reader:
                print(row)

        # data = pd.read_csv(download.readall())
        # reader = csv.reader(download.readall())
        # file_path = open(file_client, "r")
        # file_contents = file_path.read()

        #  migration_state_file = join(args.process_results_path, "migration_state.csv")
    except Exception as ex:
        print("Exception:")
        print(ex)


if __name__ == "__main__":
    read_migration_state_from_container()
