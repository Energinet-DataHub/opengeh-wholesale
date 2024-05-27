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

# MAGIC %md
# MAGIC # Move anonymised data table to target table
# MAGIC After having moved a anonymised table to a new environment, this notebook can then be used to create the anonymised table and move the data to a target table

# COMMAND ----------

# Write mode
target_table_write_mode = "append" # FILL IN

# Source variables
anonymised_database = "hive_metastore.wholesale_output_anonymised" # FILL IN
anonymised_table_name = "energy_results" # FILL IN
anonymised_storage_account_name = "stdatalakeshresdwe002" # FILL IN
anonymised_delta_table_root_path = f"abfss://wholesale@{anonymised_storage_account_name}.dfs.core.windows.net/wholesale_output_anonymised" # FILL IN

# Target variables
target_database = "hive_metastore.wholesale_output" # FILL IN
target_table_name = "energy_results" # FILL IN

# COMMAND ----------

# MAGIC %md
# MAGIC # Step 1: Create anonymised table in Catalog

# COMMAND ----------

# Add schema
query = f"""
CREATE SCHEMA IF NOT EXISTS {target_database}
"""
print(query)
spark.sql(query)

# COMMAND ----------

# Add table with location
query = f"""
CREATE TABLE IF NOT EXISTS {anonymised_database}.{anonymised_table_name}
LIKE {target_database}.{target_table_name}
LOCATION '{anonymised_delta_table_root_path}/{anonymised_table_name}'
"""
print(query)
spark.sql(query)

# COMMAND ----------

# MAGIC %md
# MAGIC # Step 2: Write anonymised to target table

# COMMAND ----------

df_anonymised = spark.read.table(f"{anonymised_database}.{anonymised_table_name}")
df_anonymised.write.mode(target_table_write_mode).saveAsTable(f"{target_database}.{target_table_name}")

# COMMAND ----------


