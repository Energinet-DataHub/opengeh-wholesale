# Databricks notebook source
storage_account_name = "datasharedresendku" # this must be changed to your storage account name
storage_account_key = "<INSERT>"
container_name = "data-lake"
spark.conf.set(
  f"fs.azure.account.key.{storage_account_name}.dfs.core.windows.net",
  storage_account_key)
master_data = "master-data"
metering_point = "meteringpoint"

delta_path = f"abfss://{container_name}@{storage_account_name}.dfs.core.windows.net/{master_data}/{metering_point}/"
print(delta_path)




# COMMAND ----------

read_df = spark.read.format("delta").load(delta_path)
read_df.count()

# COMMAND ----------


