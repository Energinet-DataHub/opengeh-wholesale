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
from azure.core.paging import ItemPaged
from package.datamigration.migration_script_args import MigrationScriptArgs
from os import path


def apply(args: MigrationScriptArgs) -> None:
    """
    Migrate from (example)
       `<container>/calculation-output/batch={id}/basis_data/{time-series-type}/grid_area=806/gln=grid_area/basis_data/time-series-hour/grouping=total_ga/grid_area=806/`
    to
       `<container>/calculation-output/batch={id}/basis_data/{time-series-type}/grouping=total_ga/grid_area=806/
    where time-series-type := time_series_hour | time_series_quarter | master_basis_data
    """
    container = "wholesale"
    calculation_output_path = "calculation-output"

    file_system_client = FileSystemClient(
        account_url=args.storage_account_url,
        file_system_name=container,
        credential=args.storage_account_key,
    )

    if not file_system_client.exists():
        error = f"Unable to create file system client. File system '{container}' does not exist"
        raise FileSystemNotFoundException(error)

    batch_directories = file_system_client.get_paths(
        path=calculation_output_path, recursive=False
    )
    for batch_directory in batch_directories:
        migrate_batch(batch_directory, file_system_client)


def migrate_batch(
    batch_directory: ItemPaged,
    file_system_client: FileSystemClient,
) -> None:
    time_series_types = ["time_series_hour", "time_series_quarter", "master_basis_data"]

    for time_series_type in time_series_types:
        time_series_type_path = path.join(
            batch_directory.name, f"basis_data/{time_series_type}"
        )

        grid_area_directories = file_system_client.get_paths(
            path=time_series_type_path, recursive=False
        )

        for grid_area_directory in grid_area_directories:
            source_path = path.join(grid_area_directory.name, "gln=grid_area")
            target_path = path.join(
                time_series_type_path,
                "grouping=total_ga",
                path.dirname(grid_area_directory.name),
            )
            move_and_rename_folder(file_system_client, source_path, target_path)


def move_and_rename_folder(
    file_system_client: FileSystemClient, source_path: str, target_path: str
) -> None:
    directory_client = file_system_client.get_directory_client(source_path)
    if not directory_client.exists():
        print(
            f"Skipping migration ({__file__}). Source directory not found:{source_path}"
        )
        return

    target_path = path.join(file_system_client.file_system_name, target_path)
    directory_client.rename_directory(new_name=target_path)


class FileSystemNotFoundException(Exception):
    pass
