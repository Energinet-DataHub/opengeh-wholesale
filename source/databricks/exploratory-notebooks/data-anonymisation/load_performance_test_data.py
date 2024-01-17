# Databricks notebook source
import pyspark.sql.functions as F
from pyspark.sql.window import Window
import time

# COMMAND ----------

database = "wholesale"
time_series_target_table_name = "time_series_points_for_performance_test"
metering_point_target_table_name = "metering_point_periods_performance_test"
grid_loss_responsible_target_table_name = "grid_loss_responsible_performance_test"

# COMMAND ----------

# MAGIC %md
# MAGIC ### Schemas for MP and TS

# COMMAND ----------

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

mp_schema_ddl = """(`metering_point_id` string NOT NULL, 
`type` string NOT NULL, 
`calculation_type` string, 
`settlement_method` string, 
`grid_area_code` string NOT NULL, 
`resolution` string NOT NULL, 
`from_grid_area_code` string, 
`to_grid_area_code` string, 
`parent_metering_point_id` string, 
`energy_supplier_id` string, 
`balance_responsible_id` string, 
`from_date` timestamp NOT NULL, 
`to_date` timestamp)""".replace(
    "\n", ""
)
print(mp_schema_ddl)

# COMMAND ----------

ts_schema_ddl = """(`metering_point_id` string NOT NULL, 
`quantity` decimal(18,6), 
`quality` string NOT NULL, 
`observation_time` timestamp NOT NULL)""".replace(
    "\n", ""
)
print(ts_schema_ddl)

# COMMAND ----------

grid_schema_ddl = """(`metering_point_id` string NOT NULL)""".replace("\n", "")
print(grid_schema_ddl)

# COMMAND ----------

# MAGIC %md
# MAGIC ## Grid loss responsible

# COMMAND ----------

# STEP 1: Create new table, pointing to correct empty location
query = f"""
    CREATE TABLE IF NOT EXISTS {database}.{grid_loss_responsible_target_table_name} {grid_schema_ddl}
    LOCATION 'abfss://wholesale@stdatalakeshrestwe001.dfs.core.windows.net/calculation_input/{grid_loss_responsible_target_table_name}'
"""
print(query)
spark.sql(query)

# COMMAND ----------

# Step 2: Append all data from location, to table
(
    spark.read.format("delta")
    .load(
        f"abfss://wholesale@stdatalakeshrestwe001.dfs.core.windows.net/calculation_input/{grid_loss_responsible_target_table_name}"
    )
    .select("metering_point_id")
    .write.option("mergeSchema", False)
    .mode("append")
    .saveAsTable("wholesale.grid_loss_responsible_performance_test")
)

# COMMAND ----------

# MAGIC %md
# MAGIC ## Metering points

# COMMAND ----------

# STEP 1: Create new table, pointing to correct empty location
query = f"""
    CREATE TABLE IF NOT EXISTS  {database}.{metering_point_target_table_name} {mp_schema_ddl}
    LOCATION 'abfss://wholesale@stdatalakeshrestwe001.dfs.core.windows.net/calculation_input/{metering_point_target_table_name}'
"""
print(query)
spark.sql(query)

# COMMAND ----------

# Step 2: Append all data from location, to table
(
    spark.read.format("delta")
    .load(
        f"abfss://wholesale@stdatalakeshrestwe001.dfs.core.windows.net/calculation_input/{metering_point_target_table_name}"
    )
    .write.option("mergeSchema", False)
    .mode("append")
    .saveAsTable(f"{database}.{metering_point_target_table_name}")
)

# COMMAND ----------

# MAGIC %md
# MAGIC ## Time series points

# COMMAND ----------

# STEP 1: Create new table, pointing to correct empty location
query = f"""
    CREATE TABLE IF NOT EXISTS  {database}.{time_series_target_table_name} {ts_schema_ddl}
    LOCATION 'abfss://wholesale@stdatalakeshrestwe001.dfs.core.windows.net/calculation_input/{time_series_target_table_name}'
"""
print(query)
spark.sql(query)

# COMMAND ----------

# Step 2: Append all data from location, to table
(
    spark.read.format("delta")
    .load(
        f"abfss://wholesale@stdatalakeshrestwe001.dfs.core.windows.net/calculation_input/{time_series_target_table_name}"
    )
    .write.option("mergeSchema", False)
    .mode("append")
    .saveAsTable(f"{database}.{time_series_target_table_name}")
)
