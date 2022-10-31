# Databricks notebook source
from pyspark.sql.types import StringType, TimestampType, StructType, IntegerType,DecimalType
from pyspark.sql.functions import col, date_format
from pyspark.sql import SparkSession
import pyspark.sql.functions as F

# COMMAND ----------

storage_account_name = "timeseriesdatajouless" # this must be changed to your storage account name
storage_account_key = "<INSERT>"
container_name = "event-sourcing"
spark.conf.set(
  "fs.azure.account.key.{0}.dfs.core.windows.net".format(storage_account_name),
  storage_account_key)
# events in different folders matching events
metering_point_created = "abfss://" + container_name + "@" + storage_account_name + ".dfs.core.windows.net/events/metering-points/MeteringPointCreated/"
metering_point_connected = "abfss://" + container_name + "@" + storage_account_name + ".dfs.core.windows.net/events/metering-points/MeteringPointConnected/"

# events in one folder containing all events regardless of event type
metering_point_events = "abfss://" + container_name + "@" + storage_account_name + ".dfs.core.windows.net/events/metering-points/"

# COMMAND ----------

metering_point_id = "1"

# COMMAND ----------

# different dataframes created from different event sources
created = spark.read.option("multiLine", True).json(f"{metering_point_created}{metering_point_id}")
connected = spark.read.option("multiLine", True).json(f"{metering_point_connected}{metering_point_id}")

# COMMAND ----------

created.display()
connected.display()

# COMMAND ----------

# this results in æ
df = created.union(connected)

# COMMAND ----------

events = spark.read.option("multiLine", True).json(f"{metering_point_events}{metering_point_id}")

# COMMAND ----------

events.schema

# COMMAND ----------

events.display()

# COMMAND ----------


