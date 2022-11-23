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
from pyspark.sql import SparkSession


def apply(
    storage_account_url: str, storage_account_key: str, spark: SparkSession
) -> None:

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

    if not directory_client.exists():
        source_path = source_container + "/" + source_directory
        print(
            f"Skipping migration ({__file__}). Source directory not found:{source_path}"
        )
        return

    directory_client.rename_directory(
        new_name=destination_container + "/" + destination_directory
    )


# def _get_valid_args_or_throw(command_line_args: list[str]):
#     p = configargparse.ArgParser(
#         formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
#     )

#     p.add("--data-storage-account-name", type=str, required=True)
#     p.add("--data-storage-account-key", type=str, required=True)

#     args, unknown_args = p.parse_known_args(command_line_args)
#     if len(unknown_args):
#         unknown_args_text = ", ".join(unknown_args)
#         raise Exception(f"Unknown args: {unknown_args_text}")

#     return args


# if __name__ == "__main__":
#     args = _get_valid_args_or_throw(sys.argv[1:])
#     file_system_name = "processes"
#     dir_name = "testdir2"
#     directory_client = DataLakeDirectoryClient(
#         args.data_storage_account_name,
#         file_system_name,
#         dir_name,
#         args.data_storage_account_key,
#     )

#     destination_file_system_name = "wholesale"
#     new_dir_name = "testdir3"
#     print("Renaming the directory named '{}' to '{}'.".format(dir_name, new_dir_name))
#     new_directory = directory_client.rename_directory(
#         new_name=destination_file_system_name + "/" + new_dir_name
#     )
