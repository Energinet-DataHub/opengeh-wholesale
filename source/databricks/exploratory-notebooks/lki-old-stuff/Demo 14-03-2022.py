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
# MAGIC %md
# MAGIC # Initial Setup

# COMMAND ----------

from pyspark import SparkConf
from pyspark.sql.session import SparkSession
from delta.tables import *
from pyspark.sql.functions import *

storage_account_name = 'stdatalakesharedresu001'
storage_account_key = '<INSERT>'

delta_lake_container_name = 'timeseries-data'
timeseries_processed_blob_name = 'timeseries-processed'
timeseries_unprocessed_blob_name = 'timeseries-unprocessed'

timeseries_processed = f'abfss://{delta_lake_container_name}@{storage_account_name}.dfs.core.windows.net/{timeseries_processed_blob_name}'
timeseries_unprocessed = f'abfss://{delta_lake_container_name}@{storage_account_name}.dfs.core.windows.net/{timeseries_unprocessed_blob_name}'

spark_conf = SparkConf(loadDefaults=True) \
        .set(f'fs.azure.account.key.{storage_account_name}.dfs.core.windows.net', storage_account_key) \
        .set('spark.sql.session.timeZone', 'UTC') \
        .set('spark.databricks.io.cache.enabled', 'True') \
        .set('spark.databricks.delta.formatCheck.enabled', 'False')

spark = SparkSession \
        .builder \
        .config(conf=spark_conf)\
        .getOrCreate()

def load_unprocessed_timeseries():
    unprocessed_df = spark.read.format('delta').load(timeseries_unprocessed)
    unprocessed_df.display()
    
def load_processed_timeseries(timestamp: str = None):
    processed_df = spark.read.format('delta')
    if timestamp is not None:
        processed_df = processed_df.option("timestampAsOf", timestamp)
    processed_df = processed_df.load(timeseries_processed)
    processed_df.orderBy("metering_point_id", "time").display(100)
    
def load_processed_timeseries_history():
    processed_table = DeltaTable.forPath(spark, timeseries_processed)

    history = processed_table.history()
    history.display()
    

# COMMAND ----------

# MAGIC %md
# MAGIC # Delete from processed and un-processed timeseries

# COMMAND ----------

#unprocessed_table = DeltaTable.forPath(spark, timeseries_unprocessed)
#unprocessed_table.delete()
#
#processed_table = DeltaTable.forPath(spark, timeseries_processed)
#processed_table.delete()

# COMMAND ----------

# MAGIC %md
# MAGIC # Timeseries domain diagram
# MAGIC Shows the flow we want to demo today

# COMMAND ----------

displayHTML("<img src ='https://raw.githubusercontent.com/Energinet-DataHub/geh-timeseries/main/docs/images/ARCHITECTURE.drawio.png'>")

# COMMAND ----------

# MAGIC %md
# MAGIC # 1: Load un-processed timeseries
# MAGIC - Table where timeseries are stored in a very raw format ready for further processing and validation

# COMMAND ----------

load_unprocessed_timeseries()

# COMMAND ----------

# MAGIC %md
# MAGIC # 2: Load processed timeseries
# MAGIC - Table where timeseries are ready for aggregations

# COMMAND ----------

load_processed_timeseries()

# COMMAND ----------

# MAGIC %md
# MAGIC (Postman)
# MAGIC # 3: Send in timeseries cim xml with 2 series for metering points:
# MAGIC - 579999993331812345
# MAGIC     - 6 points
# MAGIC - 579999993331812349
# MAGIC     - 4 points

# COMMAND ----------

# MAGIC %md
# MAGIC # 4: Show timeseries received in un-processed timeseries table
# MAGIC - We see that we have received the timeseries in a very raw format ready for further processing

# COMMAND ----------

load_unprocessed_timeseries()

# COMMAND ----------

# MAGIC %md
# MAGIC # 5: Show timeseries transformed from raw format into processed timeseries table
# MAGIC - At this point the timeseries are ready for pickup by aggregations

# COMMAND ----------

load_processed_timeseries()

# COMMAND ----------

# MAGIC %md
# MAGIC (Postman)
# MAGIC # 6: Update timeseries for metering point 579999993331812345
# MAGIC - Point 1 quantity updated with quantity from **4.444** to **7.777**
# MAGIC - Point 2 quantity updated with quantity from **242.000** to **8.888**

# COMMAND ----------

# MAGIC %md
# MAGIC # 7: Show updated timeseries in processed timeseries table
# MAGIC - We see that only the **two points** for metering point **579999993331812345** is updated

# COMMAND ----------

load_processed_timeseries()

# COMMAND ----------

# MAGIC %md
# MAGIC (Postman)
# MAGIC # 8: Send in timeseries for new metering point 579999993331812350
# MAGIC - 1 series with 1 point with quantity **7.777**
# MAGIC - Registered **2022-12-23T12:00:00Z**

# COMMAND ----------

# MAGIC %md
# MAGIC # 9: Show inserted timeseries in processed timeseries table
# MAGIC - We see that **a new point** is inserted for metering point **579999993331812350**

# COMMAND ----------

load_processed_timeseries()

# COMMAND ----------

# MAGIC %md
# MAGIC (Postman)
# MAGIC # 10: Send in timeseries for metering point 579999993331812350
# MAGIC - 1 series with 1 point with quantity **1.000**
# MAGIC - Registered **2022-12-23T10:00:00Z**
# MAGIC 
# MAGIC #### As timeseries has a registration date that lies before previously sent in timeseries for same period, the timeseries is **not updated** in processed timeseries table as we already received a newer registration in the past

# COMMAND ----------

# MAGIC %md
# MAGIC # 11: Show that timeseries point for metering point 579999993331812350 is not updated
# MAGIC - Timeseries point quantity is still **7.777**

# COMMAND ----------

load_processed_timeseries()

# COMMAND ----------

# MAGIC %md
# MAGIC # History of table changes in processed timeseries table

# COMMAND ----------

load_processed_timeseries_history()

# COMMAND ----------

# MAGIC %md
# MAGIC # Time travel in processed timeseries table

# COMMAND ----------

load_processed_timeseries("2022-03-11T10:50:38.000+0000")

# COMMAND ----------
