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

import sys
import configargparse
from pyspark.sql import SparkSession
from azure.storage.filedatalake import DataLakeDirectoryClient


def apply(spark: SparkSession, storage_account_name: str, storage_account_key: str):

    storage_account_url = storage_account_name
    source_container = "processes"
    source_directory = "my_test_dir"
    destination_container = "wholesale"
    destination_directory = source_directory

    move_directory(
        storage_account_url,
        storage_account_key,
        source_container,
        source_directory,
        destination_container,
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

    directory_client.rename_directory(
        new_name=destination_container + "/" + destination_directory
    )

    # # connection_string = '' # The connection string for the source container
    # # account_key = '' # The account key for the source container
    # source_container_name = "processes"  # Name of container which has blob to be copied
    # destination_container_name = (
    #     "wholesale"  # Name of container where blob will be copied
    # )

    # # Create client
    # # client = BlobServiceClient.from_connection_string(connection_string)
    # service_client = BlobServiceClient(storage_account_name, storage_account_key)

    # # Create blob client for source blob
    # source_blob = BlobClient(
    #     service_client.url,
    #     container_name=source_container_name,
    #     blob_name=source_blob_name,
    #     # credential=sas_token,
    # )

    # # Create new blob and start copy operation.
    # new_blob = service_client.get_blob_client(
    #     destination_container_name, source_blob_name
    # )
    # new_blob.start_copy_from_url(source_blob.url)


def _get_valid_args_or_throw(command_line_args: list[str]):
    p = configargparse.ArgParser(
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    p.add("--data-storage-account-name", type=str, required=True)
    p.add("--data-storage-account-key", type=str, required=True)

    args, unknown_args = p.parse_known_args(command_line_args)
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return args


if __name__ == "__main__":
    args = _get_valid_args_or_throw(sys.argv[1:])
    file_system_name = "processes"
    dir_name = "testdir2"
    directory_client = DataLakeDirectoryClient(
        args.data_storage_account_name,
        file_system_name,
        dir_name,
        args.data_storage_account_key,
    )

    destination_file_system_name = "wholesale"
    new_dir_name = "testdir3"
    print("Renaming the directory named '{}' to '{}'.".format(dir_name, new_dir_name))
    new_directory = directory_client.rename_directory(
        new_name=destination_file_system_name + "/" + new_dir_name
    )

    # datalake_service_client = DataLakeServiceClient(
    #     args.data_storage_account_name, args.data_storage_account_key
    # )
    # file_systems = datalake_service_client.list_file_systems()
    # # for file_system in file_systems:
    # print(file_systems)

    # sas_token = generate_account_sas(
    #     account_name="stdatalakesharedresu001",
    #     account_key=args.data_storage_account_key,
    #     resource_types=ResourceTypes(service=True),
    #     permission=AccountSasPermissions(read=True),
    #     expiry=datetime.utcnow() + timedelta(hours=1),
    # )
    # service_client = BlobServiceClient(args.data_storage_account_name, sas_token)

    # credential = AzureNamedKeyCredential(
    #     "stdatalakesharedresu001", args.data_storage_account_key
    # )
    # service_client = BlobServiceClient(args.data_storage_account_name, credential)
