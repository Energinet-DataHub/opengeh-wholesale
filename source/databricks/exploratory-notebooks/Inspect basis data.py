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

# COMMAND ----------

# MAGIC %md # Notebook Configuration

# COMMAND ----------

from pyspark.sql.functions import lit, when, col, lead, last, coalesce, explode, from_json, row_number, expr
import pyspark.sql.functions as F
from pyspark.sql.types import MapType, StringType, StructType, StructField, TimestampType
from pyspark.sql.window import Window
import os

storage_account_name = os.environ["STORAGE_ACCOUNT_NAME"]
storage_account_key = os.environ["STORAGE_ACCOUNT_KEY"]
spark.conf.set(f"fs.azure.account.key.{storage_account_name}.dfs.core.windows.net",  storage_account_key)
path = f"abfss://integration-events@{storage_account_name}.dfs.core.windows.net/events"

print(storage_account_name, storage_account_key)

# COMMAND ----------

# MAGIC %md # All Integration Events

# COMMAND ----------

integration_events = (
    spark.read.option("mergeSchema", "true").parquet(path)
    .withColumn("body", col("body").cast("string"))
    .orderBy(col("storedTime").desc())
)
display(integration_events)
#integration_events.printSchema()

# COMMAND ----------

# MAGIC %md # Grid Areas
# MAGIC Grid areas are derived from the received `GridAreaUpdated` integration events.

# COMMAND ----------

grid_area_event_schema = StructType(
    [
        StructField("GridAreaCode", StringType(), True),
        StructField("GridAreaLinkId", StringType(), True),
        StructField("EffectiveDate", TimestampType(), True),
        StructField("MessageType", StringType(), True),
    ]
)

grid_area_events = (
    integration_events
    .withColumn("body", from_json(col("body"), grid_area_event_schema))
    .select("*", "body.*")
    .where(col("MessageType") == "GridAreaUpdated")
)

display(grid_area_events.orderBy(col("enqueuedTime").desc()))

# COMMAND ----------

# MAGIC %md # Time Series Raw
# MAGIC Each bundle is stored in a single JSON file.
# MAGIC Each line of the file contains a single-line serialized JSON string representing a transaction/time-series of the bundle.

# COMMAND ----------

#from pyspark.sql.types import MapType, StringType, StructType, StructField, TimestampType, LongType
#time_series_raw_schema = StructType([StructField("GsrnNumber", StringType(), True), StructField("CreatedDateTime", TimestampType(), True)])
timeseries_raw_df = spark.read.json("abfss://timeseries-data@stdatalakesharedresu001.dfs.core.windows.net/timeseries-raw/")              
display(timeseries_raw_df)

# COMMAND ----------

# MAGIC %md # Time Series Unprocessed
# MAGIC Unprocessed time-series are stored in parquet files. Each time-series is represented by a single row.

# COMMAND ----------

timeseries_unprocssed_df = (spark.read.option("mergeSchema", "true").parquet("abfss://timeseries-data@stdatalakesharedresu001.dfs.core.windows.net/timeseries-unprocessed/")
                 #.where(col("storedTime") <= snapshot_datetime)
                 #.where(col("time") >= period_start_datetime)
                 #.where(col("time") < period_end_datetime)
                 # Quantity of time series points should have 3 digits. Calculations, however, must use 6 digit precision to reduce rounding errors
                 #.withColumn("quantity", col("quantity").cast("decimal(18,6)"))
                 #.orderBy(col("storedTime").desc())
                )
timeseries_unproccesd_df

display(timeseries_unprocssed_df)

# COMMAND ----------

# MAGIC %md # Published Time Series Points
# MAGIC After validation of time-series the valid time-series are exploded into rows representing a single point each.
# MAGIC 
# MAGIC The points are enriched with some time-series data.

# COMMAND ----------

timeseries_points_df = (spark.read.option("mergeSchema", "true").parquet("abfss://timeseries-data@stdatalakesharedresu001.dfs.core.windows.net/time-series-points/")
                 #.where(col("storedTime") <= snapshot_datetime)
                 #.where(col("time") >= period_start_datetime)
                 #.where(col("time") < period_end_datetime)
                 # Quantity of time series points should have 3 digits. Calculations, however, must use 6 digit precision to reduce rounding errors
                 #.withColumn("quantity", col("quantity").cast("decimal(18,6)"))
                 .orderBy(col("storedTime").desc())
                )

display(timeseries_points_df)
