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

from pyspark.sql.types import StructType, StructField, StringType, TimestampType
from package.datamigration.migration_script_args import MigrationScriptArgs
from azure.storage.filedatalake import FileSystemClient


def apply(args: MigrationScriptArgs) -> None:
    """
    Add two new columns (ProcessType and ExecutionTime) in Delta Lake.
    The table is overiwritten with an empty dataframe. This means that the table's existing data will be deleted
    """

    schema = StructType(
        [
            StructField("grid_area", StringType(), False),
            StructField("EnergySupplierId", StringType(), True),
            StructField("BalanceResponsibleId", StringType(), True),
            StructField("quantity", StringType(), False),
            StructField("QuantityQuality", StringType(), False),
            StructField("time", TimestampType(), False),
            StructField("AggregationLevel", StringType(), False),
            StructField("time_series_type", StringType(), False),
            StructField("batch_id", StringType(), False),
            StructField("BatchProcessType", StringType(), False),
            StructField("BatchExecutionTimeStart", TimestampType(), False),
        ]
    )

    df = args.spark.createDataFrame([], schema=schema)

    table_name = "result_table"
    container_name = "wholesale"
    base_path = f"abfss://{container_name}@stdatalakesharedresu001.dfs.core.windows.net/calculation-output/"

    if directory_exists(args, container_name, base_path):
        df.write.format("delta").mode("overwrite").option(
            "overwriteSchema", "true"
        ).option("path", f"{base_path}/{table_name}").saveAsTable(table_name)


def directory_exists(args: MigrationScriptArgs, container_name: str, path: str) -> bool:
    file_system_client = FileSystemClient(
        account_url=args.storage_account_url,
        file_system_name=container_name,
        credential=args.storage_account_key,
    )

    if file_system_client.exists():
        directory_client = file_system_client.get_directory_client(directory=path)
        if directory_client.exists():
            return True

    return False
