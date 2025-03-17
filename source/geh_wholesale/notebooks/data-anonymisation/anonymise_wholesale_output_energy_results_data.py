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
# MAGIC # Step 0: Run the main notebook
# MAGIC Do not run this notebook directly, as it has dependecies that is not included in this notebook. Instead run "**anonymise_wholesale_output_main**" as it will generate the anonymised GLNs to be used later in this notebook.

# COMMAND ----------

if not df_anonymised_gln_numbers_with_mps:
    raise Exception("Please run anonymise_wholesale_output_main instead!")

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

# Drop table to clear anonymisation
query = f"""
DROP TABLE IF EXISTS {target_database}.{target_energy_results_table_name}
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
df_source_energy_results_table = spark.read.table(
    f"{source_database}.{source_energy_results_table_name}"
).filter(f"{calculation_id_column_name} == '{source_calculation_id_to_use}'")

# COMMAND ----------

# Read all grid area codes
df_source_grid_area_codes_table = (
    spark.read.table(f"{source_metering_points_database_and_table_name}")
    .select(grid_area_code_column_name)
    .distinct()
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

# MAGIC %md
# MAGIC For each grid area code give it a new 3 digit anonymized code, however if the anoynmized grid area code is identical to a real one, then we skip it and create a new one

# COMMAND ----------

df_unique_grid_area_codes = df_source_grid_area_codes_table.union(
    df_source_energy_results_table.select(grid_area_code_column_name).distinct()
).distinct()

anonymised_grid_area_codes = []
list_unique_grid_area_codes = [
    row[grid_area_code_column_name] for row in df_unique_grid_area_codes.collect()
]

first_anonymized_id_iteration = 1
for grid_area_code in list_unique_grid_area_codes:
    str_i = str(first_anonymized_id_iteration).rjust(3, "0")
    first_anonymized_id_iteration += 1

    # Keep going until we reach a new unique grid area code
    while str_i in list_unique_grid_area_codes:
        str_i = str(first_anonymized_id_iteration).rjust(3, "0")
        first_anonymized_id_iteration += 1

    anonymised_grid_area_codes.append((grid_area_code, str_i))

# COMMAND ----------

df_anonymised_grid_area_codes = spark.createDataFrame(
    anonymised_grid_area_codes,
    [grid_area_code_column_name, anonymised_grid_area_code_column_name],
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
# MAGIC # Step 4: Create the anonymised energy_results table

# COMMAND ----------

# MAGIC %md
# MAGIC Join the anonymised metering points with the source MP table, and replace original columns (metering_point_id, parent_metering_point_id, balance_responsible_id, and energy_supplier_id)

# COMMAND ----------

df_anonymised_suppliers_balancers_and_metering_points = (
    df_anonymised_gln_numbers_with_mps.select(
        F.col(gln_original_column_name).alias(metering_point_id_column_name),
        F.col(gln_original_column_name).alias(balance_responsible_id_column_name),
        F.col(gln_original_column_name).alias(energy_supplier_id_column_name),
        gln_anonymised_column_name,
    )
)

# COMMAND ----------

df_source_energy_results_table_anonymised = (
    df_source_energy_results_table
    # Anonymise Metering Point Id
    .join(
        df_anonymised_suppliers_balancers_and_metering_points.select(
            metering_point_id_column_name, gln_anonymised_column_name
        ),
        df_source_energy_results_table[metering_point_id_column_name].eqNullSafe(
            df_anonymised_suppliers_balancers_and_metering_points[
                metering_point_id_column_name
            ]
        ),
        "left",
    )
    .drop(
        df_anonymised_suppliers_balancers_and_metering_points[
            metering_point_id_column_name
        ]
    )
    .withColumn(metering_point_id_column_name, F.col(gln_anonymised_column_name))
    .drop(gln_anonymised_column_name)
    # Anonymise Energy Supplier Id
    .join(
        df_anonymised_suppliers_balancers_and_metering_points.select(
            energy_supplier_id_column_name, gln_anonymised_column_name
        ),
        df_anonymised_suppliers_balancers_and_metering_points[
            energy_supplier_id_column_name
        ].eqNullSafe(df_source_energy_results_table[energy_supplier_id_column_name]),
        "left",
    )
    .drop(
        df_anonymised_suppliers_balancers_and_metering_points[
            energy_supplier_id_column_name
        ],
        df_anonymised_suppliers_balancers_and_metering_points[
            balance_responsible_id_column_name
        ],
    )
    .withColumn(energy_supplier_id_column_name, F.col(gln_anonymised_column_name))
    .drop(gln_anonymised_column_name)
    # Anonymise Balance Supplier Id
    .join(
        df_anonymised_suppliers_balancers_and_metering_points.select(
            balance_responsible_id_column_name, gln_anonymised_column_name
        ),
        df_anonymised_suppliers_balancers_and_metering_points[
            balance_responsible_id_column_name
        ].eqNullSafe(
            df_source_energy_results_table[balance_responsible_id_column_name]
        ),
        "left",
    )
    .drop(
        df_anonymised_suppliers_balancers_and_metering_points[
            energy_supplier_id_column_name
        ],
        df_anonymised_suppliers_balancers_and_metering_points[
            balance_responsible_id_column_name
        ],
    )
    .withColumn(balance_responsible_id_column_name, F.col(gln_anonymised_column_name))
    .drop(gln_anonymised_column_name)
    # Anonymise Grid Area Code
    .join(
        df_anonymised_grid_area_codes,
        df_source_energy_results_table[grid_area_code_column_name].eqNullSafe(
            df_anonymised_grid_area_codes[grid_area_code_column_name]
        ),
        "left",
    )
    .drop(df_anonymised_grid_area_codes[grid_area_code_column_name])
    .withColumn(
        grid_area_code_column_name, F.col(anonymised_grid_area_code_column_name)
    )
    .drop(anonymised_grid_area_code_column_name)
    # Select and Distinct
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
    df_source_energy_results_table_anonymised.select(metering_point_id_column_name)
    .distinct()
    .count()
    == df_source_energy_results_table.select(metering_point_id_column_name)
    .distinct()
    .count()
)

# COMMAND ----------

assert (
    df_source_energy_results_table_anonymised.filter(
        F.col(metering_point_id_column_name).isNull()
    ).count()
    == df_source_energy_results_table.filter(
        F.col(metering_point_id_column_name).isNull()
    ).count()
)

# COMMAND ----------

assert (
    df_source_energy_results_table_anonymised.select(balance_responsible_id_column_name)
    .distinct()
    .count()
    == df_source_energy_results_table.select(balance_responsible_id_column_name)
    .distinct()
    .count()
)

# COMMAND ----------

assert (
    df_source_energy_results_table_anonymised.filter(
        F.col(balance_responsible_id_column_name).isNull()
    ).count()
    == df_source_energy_results_table.filter(
        F.col(balance_responsible_id_column_name).isNull()
    ).count()
)

# COMMAND ----------

assert (
    df_source_energy_results_table_anonymised.select(energy_supplier_id_column_name)
    .distinct()
    .count()
    == df_source_energy_results_table.select(energy_supplier_id_column_name)
    .distinct()
    .count()
)

# COMMAND ----------

assert (
    df_source_energy_results_table_anonymised.filter(
        F.col(energy_supplier_id_column_name).isNull()
    ).count()
    == df_source_energy_results_table.filter(
        F.col(energy_supplier_id_column_name).isNull()
    ).count()
)

# COMMAND ----------

assert (
    df_source_energy_results_table_anonymised.select(grid_area_code_column_name)
    .distinct()
    .count()
    == df_source_energy_results_table.select(grid_area_code_column_name)
    .distinct()
    .count()
)

# COMMAND ----------

assert (
    df_source_energy_results_table_anonymised.filter(
        F.col(grid_area_code_column_name).isNull()
    ).count()
    == df_source_energy_results_table.filter(
        F.col(grid_area_code_column_name).isNull()
    ).count()
)

# COMMAND ----------

# MAGIC %md
# MAGIC # Step 5: Write the table

# COMMAND ----------

df_source_energy_results_table_anonymised.write.format("delta").mode(
    "overwrite"
).saveAsTable(f"{target_database}.{target_energy_results_table_name}")

# COMMAND ----------
