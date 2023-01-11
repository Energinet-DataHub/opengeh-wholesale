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
    DataLakeDirectoryClient,
)
from package.datamigration.migration_script_args import MigrationScriptArgs


def apply(args: MigrationScriptArgs) -> None:
    container = "wholesale"
    directory_name = "calculation-output"

    directory_client = DataLakeDirectoryClient(
        args.storage_account_url,
        container,
        directory_name,
        args.storage_account_key,
    )

    basis_data_dir = directory_client.get_sub_directory_client("basis-data")
    if basis_data_dir.exists():
        basis_data_dir.delete_directory()

    master_basis_data_dir = directory_client.get_sub_directory_client(
        "master-basis-data"
    )
    if master_basis_data_dir.exists():
        master_basis_data_dir.delete_directory()
