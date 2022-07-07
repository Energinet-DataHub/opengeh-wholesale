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

from pyspark import SparkConf
from pyspark.sql.session import SparkSession


def initialize_spark(args):
    args_dict = vars(args)
    # Set spark config with storage account names/keys and the session timezone so that datetimes are displayed consistently (in UTC)
    spark_conf = (
        SparkConf(loadDefaults=True)
        .set(
            f"fs.azure.account.key.{args.data_storage_account_name}.dfs.core.windows.net",
            args.data_storage_account_key,
        )
        .set("spark.sql.session.timeZone", "UTC")
        .set("spark.databricks.io.cache.enabled", "True")
        .set("spark.databricks.delta.formatCheck.enabled", "False")
        .set("spark.databricks.delta.schema.autoMerge.enabled", "True")
    )
    if args_dict.get("shared_storage_account_name") is not None:
        spark_conf.set(
            f"fs.azure.account.key.{args.shared_storage_account_name}.dfs.core.windows.net",
            args.shared_storage_account_key,
        )

    return SparkSession.builder.config(conf=spark_conf).getOrCreate()
