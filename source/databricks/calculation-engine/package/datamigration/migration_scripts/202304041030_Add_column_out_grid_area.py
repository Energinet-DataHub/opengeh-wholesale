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

from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import lit
from package.datamigration.migration_script_args import MigrationScriptArgs

CONTAINER = "wholesale"
DIRECTORY_NAME = "calculation-output"
RESULT_TABLE_NAME = "result"


def apply(args: MigrationScriptArgs) -> None:
    "Add column `out_grid_area`"

    container_path = _get_container_path(args.storage_account_name)
    delta_table_path = f"{container_path}/{DIRECTORY_NAME}/{RESULT_TABLE_NAME}"

    results_df = _get_results_df(args.spark, delta_table_path)

    results_df = results_df.withColumn("out_grid_area", lit(None).cast("string"))

    results_df.write.format("delta").mode("overwrite").option(
        "overwriteSchema", "true"
    ).save(delta_table_path)


def _get_results_df(spark: SparkSession, delta_table_path: str) -> DataFrame:
    return spark.read.format("delta").load(delta_table_path)


# Separate function in order to be able to mock
def _get_container_path(storage_account_name: str) -> str:
    return f"abfss://{CONTAINER}@{storage_account_name}.dfs.core.windows.net"
