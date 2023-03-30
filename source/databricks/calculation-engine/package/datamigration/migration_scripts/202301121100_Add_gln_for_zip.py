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
    DataLakeFileClient,
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
        credential=args.storage_credential,
    )

    if file_system_client.exists():
        # Enumerate the directories in the parent folder
        directories = file_system_client.get_paths(path=directory_name)
        parent_directory_client = file_system_client.get_directory_client(
            directory=directory_name
        )

        if parent_directory_client.exists():
            # Rename each directory
            for directory in directories:
                match = re.search(
                    r"calculation-output/(batch_id=\w{8}-\w{4}-\w{4}-\w{4}-\w{12})/zip/(batch_.*)",
                    directory.name,
                )
                if match:
                    batch_id = match.group(1)
                    file_name = match.group(2)
                    current_directory_name = directory.name
                    print(current_directory_name)

                    file_client = file_system_client.get_file_client(
                        file_path=current_directory_name
                    )

                    new_sub_directory_name = f"{batch_id}/zip/gln=grid_area"
                    parent_directory_client.create_sub_directory(new_sub_directory_name)
                    new_directory_name = (
                        f"{directory_name}/{new_sub_directory_name}/{file_name}"
                    )
                    move_and_rename_folder(
                        file_client=file_client,
                        current_directory_name=current_directory_name,
                        new_directory_name=new_directory_name,
                        container=container,
                    )


def move_and_rename_folder(
    file_client: DataLakeFileClient,
    current_directory_name: str,
    new_directory_name: str,
    container: str,
) -> None:
    source_path = f"{container}/{current_directory_name}"
    new_path = f"{container}/{new_directory_name}"

    if not file_client.exists():
        print(
            f"Skipping migration ({__file__}). Source directory not found:{source_path}"
        )
        return

    file_client.rename_file(new_name=new_path)
