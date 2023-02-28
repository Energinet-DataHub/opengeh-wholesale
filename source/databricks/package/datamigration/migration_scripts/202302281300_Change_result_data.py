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
from pyspark.sql.functions import col, lit, when
from pyspark.sql import DataFrame


def apply(args: MigrationScriptArgs) -> None:
    container = "wholesale"
    directory_name = "calculation-output-test"
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
            result_path = path.join(directory.name, "result")
            result_temp_path = path.join(directory.name, "result_temp")

            directory_client = file_system_client.get_directory_client(
                directory=result_path
            )

            if directory_client.exists():
                directory_client_temp = file_system_client.get_directory_client(
                    directory=result_temp_path
                )

                if directory_client_temp.exists():
                    continue

                move_and_rename_folder(
                    directory_client=directory_client,
                    current_directory_name=result_path,
                    new_directory_name=result_temp_path,
                    container=container,
                )

                result_path = f"abfss://wholesale@{args.storage_account_name}.dfs.core.windows.net/{result_path}"
                result_temp_path = f"abfss://wholesale@{args.storage_account_name}.dfs.core.windows.net/{result_temp_path}"
                df = spark.read.json(result_temp_path)
                new_df = _map_cim_quality_to_wholesale_quality(df)

                df_production = new_df.filter(col("grouping") == "total_ga").drop("gln")
                df_everything_else = df.filter(col("grouping") != "total_ga")

                # write the dataframe back into the datalake with new partition
                (
                    df_production.repartition("grid_area")
                    .write.mode("append")
                    .partitionBy("grouping", "time_series_type", "grid_area")
                    .json(result_path)
                )

                (
                    df_everything_else.repartition("grid_area", "gln")
                    .write.mode("append")
                    .partitionBy("grouping", "time_series_type", "grid_area", "gln")
                    .json(result_path)
                )


def _map_cim_quality_to_wholesale_quality(df: DataFrame) -> DataFrame:
    "Map input CIM Quality names to wholesale Quality names"
    return df.withColumn(
        "Quality",
        when(
            col("Quality") == "A02",
            "missing",
        )
        .when(
            col("Quality") == "A03",
            "estimated",
        )
        .when(
            col("Quality") == "A04",
            "measured",
        )
        .when(
            col("Quality") == "A05",
            "incomplete",
        )
        .when(
            col("Quality") == "A06",
            "calculated",
        )
        .otherwise("UNKNOWN"),
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
