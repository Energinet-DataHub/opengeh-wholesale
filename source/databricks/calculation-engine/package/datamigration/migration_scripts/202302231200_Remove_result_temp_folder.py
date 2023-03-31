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

from azure.storage.filedatalake import FileSystemClient
from package.datamigration.migration_script_args import MigrationScriptArgs
from os import path


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
        # Get a list of paths inside the 'calculation-output' folder
        directories = file_system_client.get_paths(path=directory_name, recursive=False)

        for directory in directories:
            result_temp_path = path.join(directory.name, "result_temp")

            directory_client_temp = file_system_client.get_directory_client(
                directory=result_temp_path
            )

            if directory_client_temp.exists():
                directory_client_temp.delete_directory()
