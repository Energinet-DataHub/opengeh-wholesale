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
# MAGIC This notebook transforms the data from the b001 to an anonymised version that could be transfered to test-001 environment.
# MAGIC We use this method to ensure we can not recalculate the anonymised ids via any function and the data in the test environment is safe
# MAGIC We should keep this notebook to ensure we can anonymise data if it should be needed again.

# COMMAND ----------

import pyspark.sql.functions as F
from pyspark.sql.window import Window

# Source variables
database = "hive_metastore.wholesale_input" # FILL IN
source_mp_table_name = "metering_point_periods"
source_ts_table_name = "time_series_points"
source_gl_table_name = "grid_loss_metering_points"

# Target variables
target_database = database
target_mp_table_name = "metering_point_periods"
target_ts_table_name = "time_series_points"
target_gl_table_name = "grid_loss_metering_points"

# Columns variables
metering_point_id_column_name = "metering_point_id"
parent_metering_point_id_column_name = "parent_metering_point_id"
balance_responsible_id_column_name = "balance_responsible_id"
energy_supplier_id_column_name = "energy_supplier_id"

# Date variables
anonymisation_start_date = '2010-08-19T22:00:00Z'
anonymisation_end_date = '2023-06-01T22:00:00Z'

# COMMAND ----------

# Read source tables
df_source_mp_table = (
    spark.read.table(f"{database}.{source_mp_table_name}")
    .filter(f"'{anonymisation_start_date}' <= from_date")
    .filter(f"from_date <= '{anonymisation_end_date}'")
    .filter(f"'{anonymisation_start_date}' <= to_date")
    .filter(f"to_date <= '{anonymisation_end_date}'")
)

df_source_ts_table = (
    spark.read.table(f"{database}.{source_ts_table_name}")
    .filter(f"'{anonymisation_start_date}' <= observation_time")
    .filter(f"observation_time <= '{anonymisation_end_date}'")
)

df_source_gl_table = spark.read.table(f"{database}.{source_gl_table_name}")

# COMMAND ----------

# MAGIC %md
# MAGIC ### Step 1: Create the target tables

# COMMAND ----------

# Add location
query = f"""
CREATE TABLE IF NOT EXISTS {target_database}.{target_mp_table_name}
LIKE {target_database}.{source_mp_table_name} 
"""
print(query)
spark.sql(query)

# COMMAND ----------

# Add location
query = f"""
CREATE TABLE IF NOT EXISTS {target_database}.{target_ts_table_name}
LIKE {target_database}.{source_ts_table_name} 
"""
print(query)
spark.sql(query)

# COMMAND ----------

# Add location
query = f"""
CREATE TABLE IF NOT EXISTS {target_database}.{target_gl_table_name}
LIKE {target_database}.{source_gl_table_name} 
"""
print(query)
spark.sql(query)

# COMMAND ----------

# MAGIC %md
# MAGIC ## Step 2: Find and anonymised all metering_point_id's and energy_supplier_ids + balance_supplier_ids

# COMMAND ----------

# MAGIC %md
# MAGIC ###Anonymisation algorithm for MP IDs:
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
    df_source_ts_table.select(metering_point_id_column_name)
    .union(df_source_mp_table.select(metering_point_id_column_name))
    .union(
        df_source_mp_table.select(
            parent_metering_point_id_column_name
        )
    )
    .distinct()
).cache()

count_distinct_mpids = len(str(df_all_metering_point_ids.count()))
window_random_order = Window.orderBy(F.rand())

df_anonymised_metering_points = (
    df_all_metering_point_ids.withColumn(
        "anonymised_mp_id",
        F.rpad(
            F.concat(
                F.lit("5"), F.lpad(F.row_number().over(window_random_order), count_distinct_mpids, "0"), F.lit("5")
            ),
            18,
            "0",
        ),
    )
    .withColumn(
        "anonymised_mp_id",
        F.when(
            F.col(metering_point_id_column_name).isNull(),
            F.lit(None),
        ).otherwise(F.col("anonymised_mp_id")),
    )
    .na.drop()
).cache()

# COMMAND ----------

# MAGIC %md
# MAGIC Assert that there are no duplicates in the new anonymised MP IDs, meaning that we have a 1:1 relationship between original MP IDs to anonymised MP IDs.

# COMMAND ----------

assert (
    df_anonymised_metering_points.groupBy("anonymised_mp_id")
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

df_all_supplier_and_balancers = (
    df_source_mp_table.select(energy_supplier_id_column_name)
    .union(
        df_source_mp_table.select(balance_responsible_id_column_name)
    )
    .distinct()
).cache()

count_distinct_suppliers_and_balancers = len(str(df_all_supplier_and_balancers.count()))
window_random_order = Window.orderBy(F.rand())

df_anonymised_suppliers_and_balancers = (
    df_all_supplier_and_balancers.withColumn(
        "anonymised_balance_or_supplier_id",
        F.rpad(
            F.concat(
                F.lit("4"), F.lpad(F.row_number().over(window_random_order), count_distinct_suppliers_and_balancers, "0"), F.lit("4")
            ),
            13,
            "0",
        ),
    )
    .withColumn(
        "anonymised_balance_or_supplier_id",
        F.when(F.col("energy_supplier_id").isNull(), F.lit(None)).otherwise(
            F.col("anonymised_balance_or_supplier_id")
        ),
    )
    .na.drop()
).cache()

# COMMAND ----------

# MAGIC %md
# MAGIC Assert that there are no duplicates in the new anonymised Balance or Supplier IDs, meaning that we have a 1:1 relationship between original Balance or Supplier IDs to anonymised Balance or Supplier IDs.

# COMMAND ----------

assert (
    df_anonymised_suppliers_and_balancers.groupBy("anonymised_balance_or_supplier_id")
    .agg(F.sum(F.lit(1)).alias("id_count"))
    .filter("id_count > 1")
    .count()
    == 0
)

# COMMAND ----------

# MAGIC %md
# MAGIC ### Create the anonymised metering_point table

# COMMAND ----------

df_source_mp_table_anonymised = (
    df_source_mp_table.join(df_anonymised_metering_points, [metering_point_id_column_name], "left")
    .withColumn(metering_point_id_column_name, F.col("anonymised_mp_id"))
    .drop("anonymised_mp_id")
    .join(
        df_anonymised_metering_points.select(
            F.col(metering_point_id_column_name).alias(parent_metering_point_id_column_name),
            "anonymised_mp_id",
        ),
        [parent_metering_point_id_column_name],
        "left",
    )
    .withColumn(parent_metering_point_id_column_name, F.col("anonymised_mp_id"))
    .drop("anonymised_mp_id")
    .join(df_anonymised_suppliers_and_balancers, [energy_supplier_id_column_name], "left")
    .withColumn(energy_supplier_id_column_name, F.col("anonymised_balance_or_supplier_id"))
    .drop("anonymised_balance_or_supplier_id")
    .join(
        df_anonymised_suppliers_and_balancers.select(
            F.col(energy_supplier_id_column_name).alias(balance_responsible_id_column_name),
            "anonymised_balance_or_supplier_id",
        ),
        [balance_responsible_id_column_name],
        "left",
    )
    .withColumn(balance_responsible_id_column_name, F.col("anonymised_balance_or_supplier_id"))
    .drop("anonymised_balance_or_supplier_id")
    .select(df_source_mp_table.columns)
).cache()

# COMMAND ----------

assert (
    df_source_mp_table_anonymised.select(metering_point_id_column_name, "type")
    .distinct()
    .groupBy(metering_point_id_column_name)
    .agg(F.sum(F.lit(1)).alias("mp_id_count"))
    .filter("mp_id_count > 1")
    .count()
    == 0
)

# COMMAND ----------

# MAGIC %md
# MAGIC ### Create the anonymised time_series_points table
# MAGIC Beware, this might be a very costly operation, and as such might be better done with chunking or something.
# MAGIC However, since the current wholesale ts source table isn't partitioned, it is hard to do.

# COMMAND ----------

# TODO
mps_to_anonymise = ["fill in when running"]

df_source_ts_table_anonymised = (
    df_source_ts_table.withColumn(
        "quantity",
        F.when(
            F.col(metering_point_id_column_name).isin(mps_to_anonymise), F.rand() * 100
        ).otherwise(F.col("quantity")),
    )
    .join(df_anonymised_metering_points, metering_point_id_column_name)
    .withColumn(metering_point_id_column_name, F.col("anonymised_mp_id"))
    .select(df_source_ts_table.columns)
).cache()


# COMMAND ----------

assert (
    df_source_ts_table_anonymised.select(metering_point_id_column_name).distinct().count()
    == df_source_ts_table.select(metering_point_id_column_name).distinct().count()
)

# COMMAND ----------

# MAGIC %md
# MAGIC ### Create the anonymised grid_loss_metering_points table

# COMMAND ----------

df_source_gl_table_anonymised = (
    df_source_gl_table.join(df_anonymised_metering_points, metering_point_id_column_name)
    .withColumn(metering_point_id_column_name, F.col("anonymised_mp_id"))
    .select(df_source_gl_table.columns)
)

# COMMAND ----------

assert (
    df_source_gl_table_anonymised.select(metering_point_id_column_name).distinct().count()
    == df_source_gl_table_anonymised.select(metering_point_id_column_name).distinct().count()
)

# COMMAND ----------

df_source_mp_table_anonymised.write.format("delta").mode("append").saveAsTable(
    f"{target_database}.{target_mp_table_name}"
)

# COMMAND ----------

df_source_ts_table_anonymised.write.format("delta").mode("append").saveAsTable(
    f"{target_database}.{target_ts_table_name}"
)

# COMMAND ----------

df_source_gl_table_anonymised.write.format("delta").mode("append").saveAsTable(
    f"{target_database}.{target_gl_table_name}"
)
