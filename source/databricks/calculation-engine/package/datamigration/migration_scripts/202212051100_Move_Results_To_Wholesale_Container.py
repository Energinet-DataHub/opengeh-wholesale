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


def apply(args: MigrationScriptArgs) -> None:
    source_container = "processes"
    source_directory = "results"
    destination_container = "wholesale"
    destination_directory = source_directory

    directory_client = DataLakeDirectoryClient(
        args.storage_account_url,
        source_container,
        source_directory,
        args.storage_credential,
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
