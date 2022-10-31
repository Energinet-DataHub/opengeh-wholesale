# Databricks notebook source
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, from_json, explode
from pyspark.sql.types import DecimalType, StructType, StructField, StringType, TimestampType, ArrayType, BinaryType, IntegerType
import json
import datetime
from delta.tables import *

connectionString = "<INSERT>"
conf = {}
conf["eventhubs.connectionString"] = sc._jvm.org.apache.spark.eventhubs.EventHubsUtils.encrypt(connectionString)

storage_account_name = "timeseriesdatajouless" # this must be changed to your storage account name
storage_account_key = "<INSERT>"
container_name = "masterdata"
spark.conf.set(
  "fs.azure.account.key.{0}.dfs.core.windows.net".format(storage_account_name),
  storage_account_key)

mps = "abfss://" + container_name + "@" + storage_account_name + ".dfs.core.windows.net/delta/metering-point"
charges = "abfss://" + container_name + "@" + storage_account_name + ".dfs.core.windows.net/delta/charge"


MeteringPointPeriod = StructType([
      StructField("metering_point_id", StringType(), False),
      StructField("valid_from", TimestampType(), False),
      StructField("valid_to", TimestampType(), False)
])

def foreach_batch_function(df, epoch_id):
    if len(df.head(1)) > 0:
      df.show()

# inputDF = spark.readStream.option("header", "true").schema(userSchema).csv(inputPath)
mpDf = spark.readStream.format("delta").load(mps)
chargeDf = spark.readStream.format("delta").load(charges)
query = mpDf.writeStream.format("console").foreachBatch(foreach_batch_function).start()
query2 = chargeDf.writeStream.format("console").foreachBatch(foreach_batch_function).start()

spark.streams.awaitAnyTermination()

# COMMAND ----------


