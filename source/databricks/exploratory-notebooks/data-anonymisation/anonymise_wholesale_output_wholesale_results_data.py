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
source_wholesale_results_table_name = "wholesale_results"
source_metering_points_database_and_table_name = "wholesale_input.metering_point_periods"
source_calculation_id_to_use = "a0a6f7a3-57c0-4657-ba3e-c2d18e480678" # FILL IN

# Target variables
target_database = "hive_metastore.wholesale_output_anonymised" # FILL IN
target_wholesale_results_table_name = "wholesale_results"
target_storage_account_name = "stdatalakeshresdwe002" # FILL IN
target_delta_table_root_path = f"abfss://wholesale@{target_storage_account_name}.dfs.core.windows.net/wholesale_output_anonymised" # FILL IN

# Source columns variables
grid_area_code_column_name = "grid_area_code"
charge_owner_id_column_name = "charge_owner_id"
energy_supplier_id_column_name = "energy_supplier_id"
calculation_id_column_name = "calculation_id"

# Anonymised columns variables
anonymised_grid_area_code_column_name = "anonymised_grid_area_code"
anonymised_charge_owner_or_supplier_id_column_name = "anonymised_charge_owner_or_supplier_id"
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
CREATE TABLE IF NOT EXISTS {target_database}.{target_wholesale_results_table_name}
LIKE {source_database}.{source_wholesale_results_table_name}
LOCATION '{target_delta_table_root_path}/{target_wholesale_results_table_name}'
"""
print(query)
spark.sql(query)

# COMMAND ----------

# MAGIC %md
# MAGIC # Step 2: Read source tables

# COMMAND ----------

# Read source tables
df_source_wholesale_results_table = (
    spark.read.table(f"{source_database}.{source_wholesale_results_table_name}")
    .filter(f"{calculation_id_column_name} == '{source_calculation_id_to_use}'")
)

# COMMAND ----------

# Read all grid area codes
df_source_grid_area_codes_table = (
    spark.read.table(f"{source_metering_points_database_and_table_name}")
    .select(grid_area_code_column_name)
    .distinct()
)

# COMMAND ----------

# MAGIC %md
# MAGIC # Step 3: Find and anonymised all grid_area_codes, and metering_point_id's and energy_supplier_ids + charge_owner_id

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

# MAGIC %md
# MAGIC For each grid area code give it a new 3 digit anonymized code, however if the anoynmized grid area code is identical to a real one, then we skip it and create a new one

# COMMAND ----------

df_unique_grid_area_codes = df_source_grid_area_codes_table.union(df_source_wholesale_results_table.select(grid_area_code_column_name).distinct()).distinct()

anonymised_grid_area_codes = []
list_unique_grid_area_codes = [row[grid_area_code_column_name] for row in df_unique_grid_area_codes.collect()]

first_anonymized_id_iteration = 1
for grid_area_code in list_unique_grid_area_codes:
    str_i = str(first_anonymized_id_iteration).rjust(3, '0')
    first_anonymized_id_iteration += 1

    # Keep going until we reach a new unique grid area code
    while str_i in list_unique_grid_area_codes:
        str_i = str(first_anonymized_id_iteration).rjust(3, '0')
        first_anonymized_id_iteration += 1
    
    anonymised_grid_area_codes.append((grid_area_code, str_i))

# COMMAND ----------

df_anonymised_grid_area_codes = spark.createDataFrame(anonymised_grid_area_codes, [grid_area_code_column_name, anonymised_grid_area_code_column_name]).cache()

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
# MAGIC ### Anonymisation algorithm for Charge Owner or Supplier IDs:
# MAGIC Use randomized GLN Generator provided by Raccoons. It generates a valid GLN with the correct length of 12 characters, it makes sure that the checksum is valid and thus provides a valid GLN.

# COMMAND ----------

tmp_charge_owner_and_supplier_id_column_name = "charge_and_supplier_id"

df_all_supplier_and_charge_owner = (
    df_source_wholesale_results_table.select(F.col(energy_supplier_id_column_name).alias(tmp_charge_owner_and_supplier_id_column_name))
    .union(
        df_source_wholesale_results_table.select(F.col(charge_owner_id_column_name).alias(tmp_charge_owner_and_supplier_id_column_name))
    )
    .distinct()
)

list_of_gln_numbers = [row[tmp_charge_owner_and_supplier_id_column_name] for row in df_all_supplier_and_charge_owner.collect()]

# COMMAND ----------

# MAGIC %md
# MAGIC The functions below are a direct translation from C# to Python, and is a translation of Raccoons GLN Generator

# COMMAND ----------

import random
import math


def create_random_gln():
    rng = random.Random()
    location = ''.join(str(rng.randint(0, 9)) for _ in range(12))
    
    for check_digit in range(10):
        gln = location + str(check_digit)
        if calculate_checksum(gln) == check_digit:
            return gln
    
    raise Exception("Should not happen")


def calculate_checksum(gln_number):
    sum_of_odd_numbers = 0
    sum_of_even_numbers = 0

    for i in range(1, len(gln_number)):
        current_number = int(gln_number[i - 1])

        if i % 2 == 0:
            sum_of_even_numbers += current_number
        else:
            sum_of_odd_numbers += current_number
    
    sum = sum_of_even_numbers * 3 + sum_of_odd_numbers

    return (math.ceil(sum / 10.0) * 10) - sum

# COMMAND ----------

list_of_anonymised_gln_numbers = []
anonymised_gln_numbers_mapping = []
for gln_number in list_of_gln_numbers:
    if gln_number is None:
        list_of_anonymised_gln_numbers.append(None)
        anonymised_gln_numbers_mapping.append((None, None))
        continue

    anonymised_gln_number = create_random_gln()

    # Create a new GLN number until we get one we haven't seen yet
    while anonymised_gln_number in list_of_anonymised_gln_numbers:
        anonymised_gln_number = create_random_gln()
    
    # Add to list of anonymised GLN numbers as well as the mapping
    list_of_anonymised_gln_numbers.append(anonymised_gln_number)
    anonymised_gln_numbers_mapping.append((gln_number, anonymised_gln_number))

# COMMAND ----------

df_anonymised_suppliers_and_charge_owner = spark.createDataFrame(anonymised_gln_numbers_mapping, [tmp_charge_owner_and_supplier_id_column_name, anonymised_charge_owner_or_supplier_id_column_name]).cache()

# COMMAND ----------

# MAGIC %md
# MAGIC Assert that:
# MAGIC 1. There are no duplicates in the new anonymised Charge Owner or Supplier IDs, meaning that we have a 1:1 relationship between original Charge Owner or Supplier IDs to anonymised Charge Owner or Supplier IDs.
# MAGIC 2. That we have the same amount of distinct IDs before and after.
# MAGIC 3. That we have the same amount of nulls before and after anonymisation.

# COMMAND ----------

assert (
    df_anonymised_suppliers_and_charge_owner.groupBy(anonymised_charge_owner_or_supplier_id_column_name)
    .agg(F.sum(F.lit(1)).alias("id_count"))
    .filter("id_count > 1")
    .count()
    == 0
)

# COMMAND ----------

assert (
    df_anonymised_suppliers_and_charge_owner.select(anonymised_charge_owner_or_supplier_id_column_name).distinct().count()
    == df_all_supplier_and_charge_owner.select(tmp_charge_owner_and_supplier_id_column_name).distinct().count()
)

# COMMAND ----------

assert (
    df_anonymised_suppliers_and_charge_owner.filter(F.col(anonymised_charge_owner_or_supplier_id_column_name).isNull()).count()
    == df_all_supplier_and_charge_owner.filter(F.col(tmp_charge_owner_and_supplier_id_column_name).isNull()).count()
)

# COMMAND ----------

# MAGIC %md
# MAGIC # Step 4: Create the anonymised wholesale_results table

# COMMAND ----------

# MAGIC %md
# MAGIC Join the anonymised metering points with the source MP table, and replace original columns (metering_point_id, parent_metering_point_id, charge_owner_id, and energy_supplier_id)

# COMMAND ----------

df_anonymised_suppliers_and_charge_owner = df_anonymised_suppliers_and_charge_owner.select(
    F.col(tmp_charge_owner_and_supplier_id_column_name).alias(charge_owner_id_column_name),
    F.col(tmp_charge_owner_and_supplier_id_column_name).alias(energy_supplier_id_column_name),
    anonymised_charge_owner_or_supplier_id_column_name,
)

# COMMAND ----------

df_source_wholesale_results_table_anonymised = (
    df_source_wholesale_results_table
    # Anonymise Energy Supplier Id
    .join(
        df_anonymised_suppliers_and_charge_owner,
        df_anonymised_suppliers_and_charge_owner[energy_supplier_id_column_name].eqNullSafe(df_source_wholesale_results_table[energy_supplier_id_column_name]),
        "left",
    )
    .drop(df_anonymised_suppliers_and_charge_owner[energy_supplier_id_column_name], df_anonymised_suppliers_and_charge_owner[charge_owner_id_column_name])
    .withColumn(energy_supplier_id_column_name, F.col(anonymised_charge_owner_or_supplier_id_column_name))
    .drop(anonymised_charge_owner_or_supplier_id_column_name)
    # Anonymise Charge Owner Id
    .join(
        df_anonymised_suppliers_and_charge_owner,
        df_anonymised_suppliers_and_charge_owner[charge_owner_id_column_name].eqNullSafe(df_source_wholesale_results_table[charge_owner_id_column_name]),
        "left",
    )
    .drop(df_anonymised_suppliers_and_charge_owner[energy_supplier_id_column_name], df_anonymised_suppliers_and_charge_owner[charge_owner_id_column_name])
    .withColumn(charge_owner_id_column_name, F.col(anonymised_charge_owner_or_supplier_id_column_name))
    .drop(anonymised_charge_owner_or_supplier_id_column_name)
    # Anonymise Grid Area Code
    .join(
        df_anonymised_grid_area_codes,
        df_source_wholesale_results_table[grid_area_code_column_name].eqNullSafe(df_anonymised_grid_area_codes[grid_area_code_column_name]),
        "left",
    )
    .drop(df_anonymised_grid_area_codes[grid_area_code_column_name])
    .withColumn(grid_area_code_column_name, F.col(anonymised_grid_area_code_column_name))
    .drop(anonymised_grid_area_code_column_name)
    # Select and Distinct
    .select(df_source_wholesale_results_table.columns)
    .distinct()
).cache()

# COMMAND ----------

# MAGIC %md
# MAGIC Assert that:
# MAGIC 1. We have the same amount of unique Charge Owner Ids in the anonymised and source table
# MAGIC 2. We have the same amount of null Charge Owner Ids in the anonymised and source table
# MAGIC 3. We have the same amount of unique Energy Supplier Ids in the anonymised and source table
# MAGIC 4. We have the same amount of null Energy Supplier Ids in the anonymised and source table
# MAGIC 5. We have the same amount of unique Grid Area Codes in the anonymised and source table
# MAGIC 6. We have the same amount of null Grid Area Codes in the anonymised and source table

# COMMAND ----------

assert (
    df_source_wholesale_results_table_anonymised.select(charge_owner_id_column_name).distinct().count() == df_source_wholesale_results_table.select(charge_owner_id_column_name).distinct().count()
)

# COMMAND ----------

assert (
    df_source_wholesale_results_table_anonymised.filter(F.col(charge_owner_id_column_name).isNull()).count()
    == df_source_wholesale_results_table.filter(F.col(charge_owner_id_column_name).isNull()).count()
)

# COMMAND ----------

assert (
    df_source_wholesale_results_table_anonymised.select(energy_supplier_id_column_name).distinct().count() == df_source_wholesale_results_table.select(energy_supplier_id_column_name).distinct().count()
)

# COMMAND ----------

assert (
    df_source_wholesale_results_table_anonymised.filter(F.col(energy_supplier_id_column_name).isNull()).count()
    == df_source_wholesale_results_table.filter(F.col(energy_supplier_id_column_name).isNull()).count()
)

# COMMAND ----------

assert (
    df_source_wholesale_results_table_anonymised.select(grid_area_code_column_name).distinct().count() == df_source_wholesale_results_table.select(grid_area_code_column_name).distinct().count()
)

# COMMAND ----------

assert (
    df_source_wholesale_results_table_anonymised.filter(F.col(grid_area_code_column_name).isNull()).count()
    == df_source_wholesale_results_table.filter(F.col(grid_area_code_column_name).isNull()).count()
)

# COMMAND ----------

# MAGIC %md
# MAGIC # Step 5: Write the table

# COMMAND ----------

df_source_wholesale_results_table_anonymised.write.format("delta").mode("overwrite").saveAsTable(
    f"{target_database}.{target_wholesale_results_table_name}"
)

# COMMAND ----------


