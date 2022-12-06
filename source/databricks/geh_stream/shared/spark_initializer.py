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
from pyspark.sql import SparkSession


def initialize_spark(data_storage_account_name: str, data_storage_account_key: str) -> SparkSession:
    # Set spark config with storage account names/keys
    # and the session timezone so that datetimes are
    # displayed consistently (in UTC)

    # We use optimizeWrite and autoCompact check what that means here:
    # https://docs.microsoft.com/en-us/azure/databricks/delta/optimizations/auto-optimize

    spark_conf = SparkConf(loadDefaults=True) \
        .set('fs.azure.account.key.{0}.dfs.core.windows.net'.format(data_storage_account_name), data_storage_account_key) \
        .set("spark.sql.session.timeZone", "UTC") \
        .set("spark.sql.decimalOperations.allowPrecisionLoss", "False") \
        .set("spark.databricks.io.cache.enabled", "True") \
        .set("spark.databricks.delta.optimizeWrite.enabled", "True") \
        .set("spark.databricks.delta.autoCompact.enabled", "True")

    return SparkSession \
        .builder \
        .config(conf=spark_conf) \
        .getOrCreate()
