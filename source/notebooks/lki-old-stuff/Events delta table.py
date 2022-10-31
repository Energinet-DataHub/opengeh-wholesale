# Databricks notebook source
storage_account_name = "datasharedresendku" # this must be changed to your storage account name
storage_account_key = "<INSERT>"
container_name = "data-lake"
spark.conf.set(f"fs.azure.account.key.{storage_account_name}.dfs.core.windows.net",storage_account_key)
spark.conf.set("spark.databricks.delta.formatCheck.enabled", "False")
events_table = "events"

delta_path = f"abfss://{container_name}@{storage_account_name}.dfs.core.windows.net/{events_table}/"





# COMMAND ----------

from pyspark.sql import DataFrame
from pyspark.sql.functions import col
from pyspark.sql.types import StructType, StructField, StringType

schema = StructType([
      StructField("event_id", StringType(), True),
      StructField("event_name", StringType(), True),
      StructField("body", StringType(), True)
])

df  = spark.createDataFrame([], schema)
df.write \
            .format("delta") \
            .mode("append") \
            .partitionBy("event_name") \
            .save(delta_path)


# COMMAND ----------

from pyspark.sql.functions import col
read_df = spark.read.format('delta').load(delta_path)
read_df.display()

# COMMAND ----------


