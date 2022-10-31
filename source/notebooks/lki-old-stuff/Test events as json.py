# Databricks notebook source
import json
from pyspark.sql.types import *
from pyspark.sql.functions import *
from pyspark import SparkConf
from delta.tables import *
from datetime import datetime

storage_account_name = "timeseriesdatajouless" # this must be changed to your storage account name
storage_account_key = "<INSERT>"
containerName = "messagedata"
source = "abfss://messagedata@timeseriesdatajouless.dfs.core.windows.net/test-json-diff/"
print(source)

spark.conf.set("fs.azure.account.key.{0}.dfs.core.windows.net".format(storage_account_name),storage_account_key)



# COMMAND ----------

df = spark.read.json(source)
df.show(100, False)

# COMMAND ----------


