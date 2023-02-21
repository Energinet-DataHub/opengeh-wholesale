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

from azure.storage.filedatalake import FileSystemClient, DataLakeDirectoryClient
from package.datamigration.migration_script_args import MigrationScriptArgs
from os import path


def apply(args: MigrationScriptArgs) -> None:
    container = "wholesale"
    directory_name = "calculation-output"
    spark = args.spark

    # Get the file system client
    file_system_client = FileSystemClient(
        account_url=args.storage_account_url,
        file_system_name=container,
        credential=args.storage_account_key,
    )

    if file_system_client.exists():
        # Get a list of paths inside the 'calculation-output' folder
        directories = file_system_client.get_paths(path=directory_name, recursive=False)

        for directory in directories:
            actors_path = path.join(directory.name, "actors")
            directory_client = file_system_client.get_directory_client(
                directory=actors_path
            )

            if directory_client.exists():
                actors_temp_path = path.join(directory.name, "actors_temp")

                rename_folder(
                    directory_client=directory_client,
                    current_directory_name=actors_path,
                    new_directory_name=actors_temp_path,
                    container=container,
                )
                directory_client = file_system_client.get_directory_client(
                    directory=actors_temp_path
                )
                if not directory_client.exists():
                    return  # renaming went wrong!?!

                actors_write_path = f"abfss://wholesale@{args.storage_account_name}.dfs.core.windows.net/{actors_path}"
                actors_read_path = f"abfss://wholesale@{args.storage_account_name}.dfs.core.windows.net/{actors_temp_path}"

                df = spark.read.json(actors_read_path)

                # rename gln column and remove market role (we don't want it as a directory level anymore)
                new_df = df.withColumnRenamed("gln", "energy_supplier_gln").drop(
                    "market_role"
                )

                # write the dataframe back into the datalake with new partition
                (
                    new_df.repartition("grid_area")
                    .write.mode("overwrite")
                    .partitionBy("time_series_type", "grid_area")
                    .json(actors_write_path)
                )


def rename_folder(
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
