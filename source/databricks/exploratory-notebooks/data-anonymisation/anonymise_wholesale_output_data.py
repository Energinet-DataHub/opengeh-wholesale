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
# MAGIC ### Anoymise dataset for test environments
# MAGIC This notebook transforms the data from the p-001 to an anonymised version that could be transfered to t001 environment.
# MAGIC We use this method to ensure we can not recalculate the anonymised ids via any function and the data in the test environment is safe
# MAGIC We should keep this notebook to ensure we can anonymise data if it should be needed again.

# COMMAND ----------

import pyspark.sql.functions as F
from pyspark.sql.window import Window

# Source variables
source_database = "hive_metastore.wholesale_output" # FILL IN
source_energy_results_table_name = "energy_results"
source_calculation_id_to_use = "1fba2899-7ea5-4d36-8330-67022bdcac14" # FILL IN

# Target variables
target_database = "hive_metastore.wholesale_output_anonymised" # FILL IN
target_energy_results_table_name = "energy_results"
target_storage_account_name = "stdatalakeshresdwe001" # FILL IN
target_delta_table_root_path = f"abfss://wholesale@{target_storage_account_name}.dfs.core.windows.net/wholesale_output_anonymised" # FILL IN

# Source columns variables
grid_area_code_column_name = "grid_area_code"
metering_point_id_column_name = "metering_point_id"
balance_responsible_id_column_name = "balance_responsible_id"
energy_supplier_id_column_name = "energy_supplier_id"
calculation_id_column_name = "calculation_id"

# Anonymised columns variables
anonymised_grid_area_code_column_name = "anonymised_grid_area_code"
anonymised_balance_or_supplier_id_column_name = "anonymised_balance_or_supplier_id"
anonymised_metering_point_id_column_name = "anonymised_metering_point_id"

# COMMAND ----------

# MAGIC %md
# MAGIC # Step 1: Create the target schema + tables

# COMMAND ----------

# Add schema
query = f"""
CREATE SCHEMA IF NOT EXISTS {target_database}
"""
print(query)
spark.sql(query)

# COMMAND ----------

# Add location
query = f"""
CREATE TABLE IF NOT EXISTS {target_database}.{target_energy_results_table_name}
LIKE {source_database}.{source_energy_results_table_name}
LOCATION '{target_delta_table_root_path}/{target_energy_results_table_name}'
"""
print(query)
spark.sql(query)

# COMMAND ----------

# MAGIC %md
# MAGIC # Step 2: Read source tables

# COMMAND ----------

# Read source tables
df_source_energy_results_table = (
    spark.read.table(f"{source_database}.{source_energy_results_table_name}")
    .filter(f"{calculation_id_column_name} == '{source_calculation_id_to_use}'")
)

# COMMAND ----------

# MAGIC %md
# MAGIC # Step 3: Find and anonymised all grid_area_codes, and metering_point_id's and energy_supplier_ids + balance_supplier_ids

# COMMAND ----------

# MAGIC %md
# MAGIC ###Anonymisation algorithm for Grid Area Codes:
# MAGIC 1) Concat random row_number over each row of unique grid area codes, and left-pad with "0" with until 3 digits
# MAGIC
# MAGIC #### Example
# MAGIC **1)**
# MAGIC
# MAGIC Original (fake) grid area code: 350 (1st after random order)
# MAGIC
# MAGIC Anonymised grid area code: 001
# MAGIC
# MAGIC **2)**
# MAGIC
# MAGIC Original (fake) grid area code: 120 (543th after random order)
# MAGIC
# MAGIC Anonymised grid area code: 543

# COMMAND ----------

df_all_grid_area_codes = (
    df_source_energy_results_table.select(grid_area_code_column_name)
    .distinct()
).cache()

count_distinct_mpids = len(str(df_all_grid_area_codes.count()))
window_random_order = Window.orderBy(F.rand())

df_anonymised_grid_area_codes = (
    df_all_grid_area_codes.withColumn(
        anonymised_grid_area_code_column_name, 
        F.lpad(F.row_number().over(window_random_order), 3, "0")
    )
    .withColumn(
        anonymised_grid_area_code_column_name,
        F.when(
            F.col(grid_area_code_column_name).isNull(),
            F.lit(None),
        ).otherwise(F.col(anonymised_grid_area_code_column_name)),
    )
    .na.drop()
).cache()

# COMMAND ----------

# MAGIC %md
# MAGIC Assert that:
# MAGIC 1) There are no duplicates in the new anonymised grid area codes, meaning that we have a 1:1 relationship between original grid area codes to anonymised grid area codes.

# COMMAND ----------

assert (
    df_anonymised_grid_area_codes.groupBy(anonymised_grid_area_code_column_name)
    .agg(F.sum(F.lit(1)).alias("grid_area_id_count"))
    .filter("grid_area_id_count > 1")
    .count()
    == 0
)

# COMMAND ----------

# MAGIC %md
# MAGIC ###Anonymisation algorithm for Metering Point IDs:
# MAGIC 1) Prefix anonymised ID with "5"
# MAGIC 2) Concat random row_number over each row of unique MP IDs, and left-pad with "0" with same length as unique MP IDs
# MAGIC 3) Concat "5"
# MAGIC 4) Right-pad with "0" until 18 characters in total
# MAGIC
# MAGIC #### Example
# MAGIC **1)**
# MAGIC
# MAGIC Original (fake) MP ID: 514526978536898745 (1st after random order)
# MAGIC
# MAGIC Anonymised MP ID: 500000015000000000
# MAGIC
# MAGIC **2)**
# MAGIC
# MAGIC Original (fake) MP ID: 525865741589334125 (532435th after random order)
# MAGIC
# MAGIC Anonymised MP ID: 505324355000000000

# COMMAND ----------

df_all_metering_point_ids = (
    df_source_energy_results_table.select(metering_point_id_column_name)
    .distinct()
).cache()

count_distinct_mpids = len(str(df_all_metering_point_ids.count()))
window_random_order = Window.orderBy(F.rand())

df_anonymised_metering_points = (
    df_all_metering_point_ids.withColumn(
        anonymised_metering_point_id_column_name,
        F.rpad(
            F.concat(
                F.lit("5"), F.lpad(F.row_number().over(window_random_order), count_distinct_mpids, "0"), F.lit("5")
            ),
            18,
            "0",
        ),
    )
    .withColumn(
        anonymised_metering_point_id_column_name,
        F.when(
            F.col(metering_point_id_column_name).isNull(),
            F.lit(None),
        ).otherwise(F.col(anonymised_metering_point_id_column_name)),
    )
    .na.drop()
).cache()

# COMMAND ----------

# MAGIC %md
# MAGIC Assert that:
# MAGIC 1) There are no duplicates in the new anonymised MP IDs, meaning that we have a 1:1 relationship between original MP IDs to anonymised MP IDs.

# COMMAND ----------

assert (
    df_anonymised_metering_points.groupBy(anonymised_metering_point_id_column_name)
    .agg(F.sum(F.lit(1)).alias("mp_id_count"))
    .filter("mp_id_count > 1")
    .count()
    == 0
)

# COMMAND ----------

# MAGIC %md
# MAGIC ### Anonymisation algorithm for Balance or Supplier IDs:
# MAGIC 1) Prefix anonymised ID with "4"
# MAGIC 2) Concat random row_number over each row of unique Balance or Supplier IDs, and left-pad with "0" with same length as unique Balance or Supplier IDs
# MAGIC 3) Concat "4"
# MAGIC 4) Right-pad with "0" until 13 characters in total
# MAGIC
# MAGIC #### Example
# MAGIC **1)**
# MAGIC
# MAGIC Original (fake) Balance or Supplier ID: 5582145332287 (1st after random order)
# MAGIC
# MAGIC Anonymised Balance or Supplier ID: 4014000000000
# MAGIC
# MAGIC **2)**
# MAGIC
# MAGIC Original (fake) Balance or Supplier ID: 5365866475198 (78th after random order)
# MAGIC
# MAGIC Anonymised Balance or Supplier ID: 4784000000000

# COMMAND ----------

tmp_balance_and_supplier_id_column_name = "balance_and_supplier_id"

df_all_supplier_and_balancers = (
    df_source_energy_results_table.select(F.col(energy_supplier_id_column_name).alias(tmp_balance_and_supplier_id_column_name))
    .union(
        df_source_energy_results_table.select(F.col(balance_responsible_id_column_name).alias(tmp_balance_and_supplier_id_column_name))
    )
    .distinct()
).cache()

count_distinct_suppliers_and_balancers = len(str(df_all_supplier_and_balancers.count()))
window_random_order = Window.orderBy(F.rand())

df_anonymised_suppliers_and_balancers = (
    df_all_supplier_and_balancers.withColumn(
        anonymised_balance_or_supplier_id_column_name,
        F.rpad(
            F.concat(
                F.lit("4"), F.lpad(F.row_number().over(window_random_order), count_distinct_suppliers_and_balancers, "0"), F.lit("4")
            ),
            13,
            "0",
        ),
    )
    .withColumn(
        anonymised_balance_or_supplier_id_column_name,
        F.when(F.col(tmp_balance_and_supplier_id_column_name).isNull(), F.lit(None)).otherwise(
            F.col(anonymised_balance_or_supplier_id_column_name)
        ),
    )
).cache()

# COMMAND ----------

# MAGIC %md
# MAGIC Assert that:
# MAGIC 1. There are no duplicates in the new anonymised Balance or Supplier IDs, meaning that we have a 1:1 relationship between original Balance or Supplier IDs to anonymised Balance or Supplier IDs.
# MAGIC 2. That we have the same amount of distinct IDs before and after.
# MAGIC 3. That we have the same amount of nulls before and after anonymisation.

# COMMAND ----------

assert (
    df_anonymised_suppliers_and_balancers.groupBy(anonymised_balance_or_supplier_id_column_name)
    .agg(F.sum(F.lit(1)).alias("id_count"))
    .filter("id_count > 1")
    .count()
    == 0
)

# COMMAND ----------

assert (
    df_anonymised_suppliers_and_balancers.select(anonymised_balance_or_supplier_id_column_name).distinct().count()
    == df_all_supplier_and_balancers.select(tmp_balance_and_supplier_id_column_name).distinct().count()
)

# COMMAND ----------

assert (
    df_anonymised_suppliers_and_balancers.filter(F.col(anonymised_balance_or_supplier_id_column_name).isNull()).count()
    == df_all_supplier_and_balancers.filter(F.col(tmp_balance_and_supplier_id_column_name).isNull()).count()
)

# COMMAND ----------

# MAGIC %md
# MAGIC # Step 4: Create the anonymised energy_results table

# COMMAND ----------

# MAGIC %md
# MAGIC Join the anonymised metering points with the source MP table, and replace original columns (metering_point_id, parent_metering_point_id, balance_responsible_id, and energy_supplier_id)

# COMMAND ----------

df_source_energy_results_table_anonymised = (
    df_source_energy_results_table.join(df_anonymised_metering_points, [metering_point_id_column_name], "left")
    .withColumn(metering_point_id_column_name, F.col(anonymised_metering_point_id_column_name))
    .drop(anonymised_metering_point_id_column_name)
    .join(df_anonymised_suppliers_and_balancers, [(df_anonymised_suppliers_and_balancers[tmp_balance_and_supplier_id_column_name]==df_source_energy_results_table.energy_supplier_id) | (df_anonymised_suppliers_and_balancers[tmp_balance_and_supplier_id_column_name]==df_source_energy_results_table.balance_responsible_id)], "left")
    .withColumn(energy_supplier_id_column_name, F.col(anonymised_balance_or_supplier_id_column_name))
    .drop(anonymised_balance_or_supplier_id_column_name)
    .join(
        df_anonymised_suppliers_and_balancers.select(
            F.col(tmp_balance_and_supplier_id_column_name).alias(balance_responsible_id_column_name),
            anonymised_balance_or_supplier_id_column_name,
        ),
        [balance_responsible_id_column_name],
        "left",
    )
    .withColumn(balance_responsible_id_column_name, F.col(anonymised_balance_or_supplier_id_column_name))
    .drop(anonymised_balance_or_supplier_id_column_name)
    .join(
        df_anonymised_grid_area_codes,
        [grid_area_code_column_name],
        "left",
    )
    .withColumn(grid_area_code_column_name, F.col(anonymised_grid_area_code_column_name))
    .drop(anonymised_grid_area_code_column_name)
    .select(df_source_energy_results_table.columns)
    .distinct()
).cache()

# COMMAND ----------

# MAGIC %md
# MAGIC Assert that:
# MAGIC 1. We have no duplicates of MP Ids in the anonymised table
# MAGIC 2. We have the same amount of unique MP Ids in the anonymised and source table
# MAGIC 3. We have the same amount of null MP Ids in the anonymised and source table
# MAGIC 4. We have the same amount of unique Balance Responsible Ids in the anonymised and source table
# MAGIC 5. We have the same amount of null Balance Responsible Ids in the anonymised and source table
# MAGIC 6. We have the same amount of unique Energy Supplier Ids in the anonymised and source table
# MAGIC 7. We have the same amount of null Energy Supplier Ids in the anonymised and source table
# MAGIC 8. We have the same amount of unique Grid Area Codes in the anonymised and source table
# MAGIC 9. We have the same amount of null Grid Area Codes in the anonymised and source table

# COMMAND ----------

assert (
    df_source_energy_results_table_anonymised.select(metering_point_id_column_name)
    .distinct()
    .groupBy(metering_point_id_column_name)
    .agg(F.sum(F.lit(1)).alias("mp_id_count"))
    .filter("mp_id_count > 1")
    .count()
    == 0
)

# COMMAND ----------

assert (
    df_source_energy_results_table_anonymised.select(metering_point_id_column_name).distinct().count() == df_source_energy_results_table.select(metering_point_id_column_name).distinct().count()
)

# COMMAND ----------

assert (
    df_source_energy_results_table_anonymised.filter(F.col(metering_point_id_column_name).isNull()).count()
    == df_source_energy_results_table.filter(F.col(metering_point_id_column_name).isNull()).count()
)

# COMMAND ----------

assert (
    df_source_energy_results_table_anonymised.select(balance_responsible_id_column_name).distinct().count() == df_source_energy_results_table.select(balance_responsible_id_column_name).distinct().count()
)

# COMMAND ----------

assert (
    df_source_energy_results_table_anonymised.filter(F.col(balance_responsible_id_column_name).isNull()).count()
    == df_source_energy_results_table.filter(F.col(balance_responsible_id_column_name).isNull()).count()
)

# COMMAND ----------

assert (
    df_source_energy_results_table_anonymised.select(energy_supplier_id_column_name).distinct().count() == df_source_energy_results_table.select(energy_supplier_id_column_name).distinct().count()
)

# COMMAND ----------

assert (
    df_source_energy_results_table_anonymised.filter(F.col(energy_supplier_id_column_name).isNull()).count()
    == df_source_energy_results_table.filter(F.col(energy_supplier_id_column_name).isNull()).count()
)

# COMMAND ----------

assert (
    df_source_energy_results_table_anonymised.select(grid_area_code_column_name).distinct().count() == df_source_energy_results_table.select(grid_area_code_column_name).distinct().count()
)

# COMMAND ----------

assert (
    df_source_energy_results_table_anonymised.filter(F.col(grid_area_code_column_name).isNull()).count()
    == df_source_energy_results_table.filter(F.col(grid_area_code_column_name).isNull()).count()
)

# COMMAND ----------

# MAGIC %md
# MAGIC # Step 5: Write the table

# COMMAND ----------

df_source_energy_results_table_anonymised.write.format("delta").mode("append").saveAsTable(
    f"{target_database}.{target_energy_results_table_name}"
)
