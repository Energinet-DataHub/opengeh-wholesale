# Databricks notebook source
# MAGIC %md 
# MAGIC # Autoloader Hackathon
# MAGIC The intended use of Autoloader is to get events coming in from a Service Bus and picked up by an Azure function into a Delta Lake so it can be used in various jobs running in Databricks.
# MAGIC 
# MAGIC _NB: The Azure function converts the event (Protobuf) to JSON and stores it in an Azure Blob where Autoloader picks it up._

# COMMAND ----------

# MAGIC %md
# MAGIC ### Setup storage account, spark and paths

# COMMAND ----------

import json
from pyspark.sql.types import *
from pyspark.sql.functions import *
from pyspark import SparkConf
from delta.tables import *
from datetime import datetime

storage_account_name = "timeseriesdatajouless" # this must be changed to your storage account name
storage_account_key = "<INSERT>"
containerName = "messagedata"
source = "abfss://messagedata@timeseriesdatajouless.dfs.core.windows.net/hackathon/"
print(source)

spark.conf.set("fs.azure.account.key.{0}.dfs.core.windows.net".format(storage_account_name),storage_account_key)

#InputDirectory and Checkpoint Location
SourceFilePath = "abfss://messagedata@timeseriesdatajouless.dfs.core.windows.net/hackathon/Raw"
TargetFilePath = "abfss://messagedata@timeseriesdatajouless.dfs.core.windows.net/hackathon/Output" 
CheckpointPath = "abfss://messagedata@timeseriesdatajouless.dfs.core.windows.net/hackathon/StreamCheckpoint"

# COMMAND ----------

# MAGIC %md
# MAGIC ### Set schema for input (event) and output (delta table)

# COMMAND ----------

input_schema = StructType([StructField("mp_id", StringType()),StructField("effective_date", TimestampType()),StructField("max_date", TimestampType()),StructField("_rescued_data", StringType())])
output_schema = StructType([StructField("mp_id", StringType()),StructField("from_date", TimestampType()),StructField("to_date", TimestampType()),StructField("_rescued_data", StringType())])

# COMMAND ----------

# MAGIC %md
# MAGIC ### Create Target Delta Table
# MAGIC 
# MAGIC Should only be run if table does not exist

# COMMAND ----------

spark.createDataFrame([], output_schema).write.format('delta').partitionBy("mp_id").save(TargetFilePath)

# COMMAND ----------

# MAGIC %md
# MAGIC ### Start Autoloader stream

# COMMAND ----------


def munge(microBatchOutputDF, batchId):
    
    targetTable = DeltaTable.forPath(spark, TargetFilePath)
        
    targetTable.alias("oldData") \
    .merge( 
      microBatchOutputDF.alias("newData"), \
      "oldData.mp_id = newData.mp_id AND oldData.from_date < newData.effective_date AND oldData.to_date > newData.effective_date") \
    .whenMatchedUpdate(set = { "to_date" : "newData.effective_date", "_rescued_data": "newData._rescued_data" }) \
    .whenNotMatchedInsert(values =
      {
        "mp_id": "newData.mp_id",
        "from_date": "newData.effective_date",
        "to_date": "newData.max_date",
        "_rescued_data": "newData._rescued_data"
      }
    ) \
    .execute()
  
df = spark.readStream.format("cloudFiles") \
    .option("cloudFiles.format", "json") \
    .option("cloudFiles.schemaEvolutionMode", "rescue") \
    .option("multiLine", "True") \
    .schema(input_schema) \
    .load(SourceFilePath) \

df.writeStream \
  .trigger(processingTime="1 second") \
  .foreachBatch(munge) \
  .option("checkpointLocation", CheckpointPath) \
  .outputMode("append") \
  .start()

# COMMAND ----------

test_df = spark \
  .read \
  .format("delta") \
  .load(TargetFilePath)

test_df.show()

# COMMAND ----------


