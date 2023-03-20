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

from pyspark.sql.functions import lit, when, col, lead, last, coalesce, explode, from_json, row_number, expr, struct, to_json
import pyspark.sql.functions as F
from pyspark.sql.types import MapType, StringType, StructType, TimestampType, StructField
from pyspark.sql.window import Window

import os

storage_account_name = "stdatalakesharedresu001" 
storage_account_key = os.environ["STORAGE_ACCOUNT_KEY"]

spark.conf.set("fs.azure.account.key.stdatalakesharedresu001.dfs.core.windows.net",  storage_account_key)

# COMMAND ----------

schema = StructType(
    [
        StructField("GsrnNumber", StringType(), True),
        StructField("GridAreaCode", StringType(), True),
        StructField("GridAreaId", StringType(), True),
        StructField("GridAreaLinkId", StringType(), True),
        StructField("SettlementMethod", StringType(), True),
        StructField("ConnectionState", StringType(), True),
        StructField("EffectiveDate", TimestampType(), True),
        StructField("MeteringPointType", StringType(), True),
        StructField("MeteringPointId", StringType(), True),
        StructField("Resolution", StringType(), True),
        StructField("CorrelationId", StringType(), True),
        StructField("MessageType", StringType(), True),
        StructField("OperationTime", TimestampType(), True),
    ]
)

GridAreaTest = (spark.read.parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/gridareaEvents-805-806").union(spark.read.option("mergeSchema", "true").parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/events")
  .select(
    "partition",
    "offset",
    "sequenceNumber",
    "enqueuedTime",
    "publisher",
    "partitionKey",
    "properties",
    "systemProperties",
    "body",
    "year",
    "month",
    "day"
   )
  .withColumn("body", col("body").cast("string"))
  .withColumn("body", from_json(col("body"), schema))
  #.where(col("body.MessageType").startswith("Grid"))
  #.select("enqueuedTime", "body.MessageType", "body.MeteringPointId", "body.GsrnNumber", "body.MeteringPointType", "body.GridAreaLinkId", "body.ConnectionState", "body.EffectiveDate", "body.Resolution", "body")
               )

display(GridAreaTest)

# COMMAND ----------

#spark.read.parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/events").printSchema()
#display(spark.read.parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/events"))
#return

body_schema = StructType(
    [
        StructField("GsrnNumber", StringType(), True),
        StructField("GridAreaCode", StringType(), True),
        StructField("GridAreaId", StringType(), True),
        StructField("GridAreaLinkId", StringType(), True),
        StructField("SettlementMethod", StringType(), True),
        StructField("ConnectionState", StringType(), True),
        StructField("EffectiveDate", TimestampType(), True),
        StructField("MeteringPointType", StringType(), True),
        StructField("MeteringPointId", StringType(), True),
        StructField("Resolution", StringType(), True),
        StructField("CorrelationId", StringType(), True),
        StructField("MessageType", StringType(), True),
        StructField("OperationTime", TimestampType(), True),
    ]
)

old_events_df = (spark.read.parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/gridareaEvents-805-806")
  .union(spark.read.option("mergeSchema", "true").parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/events"))
  #.withColumn("GridAreaId", when(col("GridAreaCode") == 805, "89801EC1-AF12-46D9-B044-05A004A0D46C").otherwise("696E5DBA-B028-4A5D-B8B4-E0C01AFD3CFE"))
  .withColumn("body", col("body").cast("string"))
  .withColumn("body", from_json(col("body"), body_schema))
  .withColumn("body", 
      to_json(struct(
        col("GridAreaLinkId"),
        col("body.GridAreaCode"),
        col("GridAreaId"),
        col("CorrelationId"),
        col("MessageType"),
        col("OperationTime"),
        col("GsrnNumber"),
        col("SettlementMethod"),
        col("ConnectionState"),
        col("EffectiveDate"),
        col("MeteringPointType"),
        col("MeteringPointId"),
        col("Resolution"),
        col("CorrelationId"),
        col("MessageType"),
        col("OperationTime")
      )).cast("binary")
   )
  .select(
      col("partition"),
      col("offset"),
      col("sequenceNumber"),
      col("enqueuedTime"),
      col("publisher"),
      col("partitionKey"),
      col("properties"),
      col("systemProperties"),
      col("body"),
      col("year"),
      col("month"),
      col("day")
  )
)

#display(GridAreaEventsDf.withColumn(col("body")))
#return
(old_events_df
 .write
 .mode("overwrite")
 .partitionBy("year", "month", "day")
 #.option("checkpointLocation", "abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/events-checkpoint")
 .parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/bjm-test")
)

new_events_df = (spark.read.parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/bjm-test"))

display(new_events_df)
display(new_events_df
  .withColumn("body", col("body").cast("string"))
  .withColumn("body", from_json(col("body"), grid_schema))
)


# COMMAND ----------

GridAreaEventsDf = spark.read.parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/gridareaEvents-805-806")
display(GridAreaEventsDf)
GridAreaEventsDf.write.option("mergeSchema", "true").mode("append").parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/events")

# COMMAND ----------

grid_schema = StructType(
    [
        StructField("GridAreaCode", StringType(), True),
        StructField("GridAreaId", StringType(), True),
        StructField("GridAreaLinkId", StringType(), True),
        StructField("CorrelationId", StringType(), True),
        StructField("MessageType", StringType(), True),
        StructField("OperationTime", TimestampType(), True),
    ]
)

test_df = (spark.read.option("mergeSchema", "true").parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/events")
  .withColumn("body", col("body").cast("string"))
  .withColumn("body", from_json(col("body"), grid_schema))
  #.where(col("body.MessageType").startswith("Grid"))
          )

display(test_df)

# COMMAND ----------

spark.read.parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/gridareaEvents-805-806").printSchema()
spark.read.parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/events").printSchema()

# COMMAND ----------

spark.read.parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/gridareaEvents-805-806").union(spark.read.option("mergeSchema", "true").parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/events"disp

# COMMAND ----------

#display(spark.read.parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/events"))

new_df = (spark.read.option("mergeSchema", "true").parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/events")
  # Add body to schema (requires overwrite of all)
  .withColumn("body", col("body").cast("binary"))
  #.where(col("MessageType").isNotNull())
  .withColumn("GridAreaId", when(col("GridAreaCode") == 805, "89801EC1-AF12-46D9-B044-05A004A0D46C").otherwise("696E5DBA-B028-4A5D-B8B4-E0C01AFD3CFE"))
  .withColumn("body",
      when(col("MessageType").startswith("Grid"),
          to_json(struct(
            col("MessageType"),
            col("GridAreaLinkId"),
            col("GridAreaCode"),
            col("GridAreaId"),
            col("CorrelationId"),
            col("OperationTime"),
          )).cast("binary"))
      .when(col("MessageType").startswith("MeteringPoint"),
           to_json(struct(
            col("MessageType"),
            col("GsrnNumber"),
            col("MeteringPointId"),
            col("MeteringPointType"),
            col("GridAreaLinkId"),
            col("SettlementMethod"),
            col("ConnectionState"),
            col("EffectiveDate"),
            col("Resolution"),
            col("CorrelationId"),
            col("OperationTime"),
            col("CorrelationId"),
            col("OperationTime")
          )).cast("binary"))
      .otherwise(
          to_json(struct(
          col("MessageType"),
          col("GridAreaCode")
        )).cast("binary"))
   )
  .select(
      col("partition"),
      col("offset"),
      col("sequenceNumber"),
      col("enqueuedTime"),
      col("publisher"),
      col("partitionKey"),
      col("properties"),
      col("systemProperties"),
      col("body"),
      col("year"),
      col("month"),
      col("day")
  )
)

display(new_df.withColumn("body", col("body").cast("string")))
new_df.write.mode("overwrite").partitionBy("year", "month", "day").parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/bjm-test")
