# Databricks notebook source
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
from pyspark.sql.functions import col, date_format
from pyspark.sql import SparkSession

# COMMAND ----------

cosmos_account_endpoint = "https://cosmos-masterdata-aggregations-endk-u.documents.azure.com:443/"
cosmos_account_key = "<INSERT>"
cosmos_database = "master-data"

# COMMAND ----------

from pyspark.sql.types import StructType, StructField, StringType, TimestampType, IntegerType

cosmos_container_name = "metering-points"
schema = StructType([
      StructField("metering_point_id", StringType(), False),
      StructField("metering_point_type", StringType(), False),
      StructField("settlement_method", StringType()),
      StructField("grid_area", StringType(), False),
      StructField("connection_state", StringType(), False),
      StructField("resolution", StringType(), False),
      StructField("in_grid_area", StringType()),
      StructField("out_grid_area", StringType()),
      StructField("metering_method", StringType(), False),
      StructField("net_settlement_group", StringType()),
      StructField("parent_metering_point_id", StringType()),
      StructField("unit", StringType(), False),
      StructField("product", StringType()),
      StructField("from_date", TimestampType(), False),
      StructField("to_date", TimestampType(), False)
])

# COMMAND ----------

from datetime import datetime
from pyspark.sql.functions import col

date_from = datetime(2018,1,1,0,0,0).timestamp()
date_to = datetime(2018,1,4,0,0,0).timestamp()
print(date_from)
config = {
        "spark.cosmos.accountEndpoint": cosmos_account_endpoint,
        "spark.cosmos.accountKey": cosmos_account_key,
        "spark.cosmos.database": cosmos_database,
        "spark.cosmos.container": cosmos_container_name,
        "spark.cosmos.read.inferSchema.forceNullableProperties": False
    }
df = spark.read.schema(schema).format("cosmos.oltp").options(**config).load()
df = df.filter(col("from_date").cast("long") >= date_from).filter(col("to_date").cast("long") < date_to)

# COMMAND ----------

df.display()

# COMMAND ----------


