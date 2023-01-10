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

from azure.storage.filedatalake import (
    FileSystemClient,
    DataLakeDirectoryClient,
)
from package.datamigration.migration_script_args import MigrationScriptArgs
import re


def apply(args: MigrationScriptArgs) -> None:
    container = "wholesale"
    directory_name = "calculation-output"

    # Get the file system client
    file_system_client = FileSystemClient(
        account_url=args.storage_account_url,
        file_system_name=container,
        credential=args.storage_account_key,
    )

    # Enumerate the directories in the parent folder
    directories = file_system_client.get_paths(path=directory_name)
    parent_directory_client = file_system_client.get_directory_client(
        directory=directory_name
    )

    # Rename each directory
    for directory in directories:
        match = re.search(
            r"calculation-output/master-basis-data/(batch_id=\w{8}-\w{4}-\w{4}-\w{4}-\w{12})/(grid_area=\d{3})",
            directory.name,
        )
        if match and directory.is_directory:
            batch_id = match.group(1)
            grid_area = match.group(2)
            current_directory_name = directory.name
            print(current_directory_name)

            directory_client = file_system_client.get_directory_client(
                directory=current_directory_name
            )
            new_sub_directory_name = (
                f"{batch_id}/basis_data/master_basis_data/{grid_area}"
            )
            parent_directory_client.create_sub_directory(new_sub_directory_name)
            new_directory_name = (
                f"{directory_name}/{new_sub_directory_name}/gln=grid_access_provider"
            )
            move_and_rename_folder(
                directory_client=directory_client,
                current_directory_name=current_directory_name,
                new_directory_name=new_directory_name,
                container=container,
            )


def move_and_rename_folder(
    directory_client: DataLakeDirectoryClient,
    current_directory_name: str,
    new_directory_name: str,
    container: str,
) -> None:
    source_path = f"{container}/{current_directory_name}"
    new_path = f"{container}/{new_directory_name}"

    if not directory_client.exists():
        print(
            f"Skipping migration ({__file__}). Source directory not found:{source_path}"
        )
        return

    directory_client.rename_directory(new_name=new_path)
