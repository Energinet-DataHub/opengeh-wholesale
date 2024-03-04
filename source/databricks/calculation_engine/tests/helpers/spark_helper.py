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

from pyspark.sql import SparkSession


def reset_spark_catalog(spark: SparkSession) -> None:
    schemas = spark.catalog.listDatabases()
    for schema in schemas:
        if schema.name != "default":
            spark.sql(f"DROP DATABASE IF EXISTS {schema.name} CASCADE")


def drop_schema_migration_table(spark: SparkSession) -> None:
    spark.sql("DROP DATABASE IF EXISTS schema_migration CASCADE")
