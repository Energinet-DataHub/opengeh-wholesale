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
from pyspark.sql.functions import col, when
from pyspark.sql.types import StringType
from pyspark.sql import DataFrame


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

                result_path = f"abfss://wholesale@{args.storage_account_name}.dfs.core.windows.net/{result_path}/"

                # Migrate brp_ga results
                brp_ga_dir = f"{result_temp_path}/grouping=brp_ga/"
                brp_ga_directory_client = file_system_client.get_directory_client(
                    directory=brp_ga_dir
                )
                if brp_ga_directory_client.exists():
                    df = spark.read.json(
                        f"abfss://wholesale@{args.storage_account_name}.dfs.core.windows.net/{brp_ga_dir}"
                    )
                    time_series_quality_migrated = (
                        _map_cim_quality_to_wholesale_quality(df)
                    )
                    time_series_quality_migrated.repartition("grid_area").write.mode(
                        "append"
                    ).partitionBy("time_series_type", "grid_area", "gln").json(
                        f"{result_path}/grouping=brp_ga"
                    )

                # Migrate es_ga results
                es_ga_dir = f"{result_temp_path}/grouping=es_ga"
                es_ga_directory_client = file_system_client.get_directory_client(
                    directory=es_ga_dir
                )
                if es_ga_directory_client.exists():
                    df = spark.read.json(
                        f"abfss://wholesale@{args.storage_account_name}.dfs.core.windows.net/{es_ga_dir}"
                    )
                    time_series_quality_migrated = (
                        _map_cim_quality_to_wholesale_quality(df)
                    )
                    time_series_quality_migrated.repartition("grid_area").write.mode(
                        "append"
                    ).partitionBy("time_series_type", "grid_area", "gln").json(
                        f"{result_path}/grouping=es_ga"
                    )

                # Migrate Quality for total ga
                total_ga_dir = f"{result_temp_path}/grouping=total_ga"
                total_ga_directory_client = file_system_client.get_directory_client(
                    directory=total_ga_dir
                )
                if total_ga_directory_client.exists():
                    df = spark.read.json(
                        f"abfss://wholesale@{args.storage_account_name}.dfs.core.windows.net/{total_ga_dir}"
                    )
                    total_ga_time_series_quality_migrated = (
                        _map_cim_quality_to_wholesale_quality(df)
                    )

                    total_ga_time_series_quality_migrated.repartition(
                        "grid_area"
                    ).write.mode("append").partitionBy(
                        "time_series_type",
                        "grid_area",
                    ).json(
                        f"{result_path}/grouping=total_ga"
                    )

                # Migrate Quality for es_brp_ga
                es_brp_ga_dir = f"{result_temp_path}/grouping=es_brp_ga"
                es_brp_ga_directory_client = file_system_client.get_directory_client(
                    directory=es_brp_ga_dir
                )
                if es_brp_ga_directory_client.exists():
                    df = spark.read.json(
                        f"abfss://wholesale@{args.storage_account_name}.dfs.core.windows.net/{es_brp_ga_dir}"
                    )
                    es_brp_ga_time_series_quality_migrated = (
                        _map_cim_quality_to_wholesale_quality(df)
                    )
                    es_brp_ga_time_series_quality_migrated.repartition(
                        "grid_area",
                    ).write.mode("append").partitionBy(
                        "time_series_type",
                        "grid_area",
                        "balance_responsible_party_gln",
                        "energy_supplier_gln",
                    ).json(
                        f"{result_path}/grouping=es_brp_ga"
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
        .otherwise("missing"),
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
