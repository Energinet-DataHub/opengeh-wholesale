# Databricks notebook source
# COMMAND ----------

# MAGIC %md
# MAGIC ### Anoymise dataset for test environments
# MAGIC This notebook transforms the data from the b001 to an anonymised version that could be transfered to test-001 environment.
# MAGIC We use this method to ensure we can not recalculate the anonymised ids via any function and the data in the test environment is safe
# MAGIC We should keep this notebook to ensure we can anonymise data if it should be needed again.

# COMMAND ----------

import pyspark.sql.functions as F
from pyspark.sql.window import Window
from pyspark.sql.types import DecimalType

# Source variables
source_database = "hive_metastore.wholesale_input"  # FILL IN
source_mp_table_name = "metering_point_periods"
source_ts_table_name = "time_series_points"
source_gl_table_name = "grid_loss_metering_points"

# Target variables
target_database = "hive_metastore.wholesale_input_anonymised"  # FILL IN
target_mp_table_name = "metering_point_periods"
target_ts_table_name = "time_series_points"
target_gl_table_name = "grid_loss_metering_points"
target_storage_account_name = "stdatalakeshresdwe001"
target_delta_table_root_path = (
    f"abfss://wholesale@{target_storage_account_name}.dfs.core.windows.net/calculation_input_anonymised"
)

# Source columns variables
metering_point_id_column_name = "metering_point_id"
parent_metering_point_id_column_name = "parent_metering_point_id"
balance_responsible_id_column_name = "balance_responsible_id"
energy_supplier_id_column_name = "energy_supplier_id"

# Anonymised columns variables
anonymised_balance_or_supplier_id_column_name = "anonymised_balance_or_supplier_id"
anonymised_mp_id_column_name = "anonymised_mp_id"

# Date variables
anonymisation_start_date = "2021-01-31T23:00:00Z"

# Anonymisation MP IDs
mps_to_anonymise = []  # Fill in with MP IDs, this list will be used to anonymise the 'quantity' of the specified MPs

# COMMAND ----------

# Read source tables
df_source_mp_table = (
    spark.read.table(f"{source_database}.{source_mp_table_name}")
    .filter(f"'{anonymisation_start_date}' <= from_date")
    .filter(f"'{anonymisation_start_date}' <= to_date")
)

df_source_ts_table = spark.read.table(f"{source_database}.{source_ts_table_name}").filter(
    f"'{anonymisation_start_date}' <= observation_time"
)

df_source_gl_table = spark.read.table(f"{source_database}.{source_gl_table_name}")

# COMMAND ----------

# MAGIC %md
# MAGIC ### Step 1: Create the target schema + tables

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
CREATE TABLE IF NOT EXISTS {target_database}.{target_mp_table_name}
LIKE {source_database}.{source_mp_table_name}
LOCATION '{target_delta_table_root_path}/{target_mp_table_name}'
"""
print(query)
spark.sql(query)

# COMMAND ----------

# Add location
query = f"""
CREATE TABLE IF NOT EXISTS {target_database}.{target_ts_table_name}
LIKE {source_database}.{source_ts_table_name}
LOCATION '{target_delta_table_root_path}/{target_ts_table_name}'
"""
print(query)
spark.sql(query)

# COMMAND ----------

# Add location
query = f"""
CREATE TABLE IF NOT EXISTS {target_database}.{target_gl_table_name}
LIKE {source_database}.{source_gl_table_name} 
LOCATION '{target_delta_table_root_path}/{target_gl_table_name}'
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
    .union(df_source_mp_table.select(parent_metering_point_id_column_name))
    .union(df_source_gl_table)
    .distinct()
).cache()

count_distinct_mpids = len(str(df_all_metering_point_ids.count()))
window_random_order = Window.orderBy(F.rand())

df_anonymised_metering_points = (
    df_all_metering_point_ids.withColumn(
        anonymised_mp_id_column_name,
        F.rpad(
            F.concat(
                F.lit("5"),
                F.lpad(F.row_number().over(window_random_order), count_distinct_mpids, "0"),
                F.lit("5"),
            ),
            18,
            "0",
        ),
    )
    .withColumn(
        anonymised_mp_id_column_name,
        F.when(
            F.col(metering_point_id_column_name).isNull(),
            F.lit(None),
        ).otherwise(F.col(anonymised_mp_id_column_name)),
    )
    .na.drop()
).cache()

# COMMAND ----------

# MAGIC %md
# MAGIC Assert that:
# MAGIC 1) There are no duplicates in the new anonymised MP IDs, meaning that we have a 1:1 relationship between original MP IDs to anonymised MP IDs.

# COMMAND ----------

assert (
    df_anonymised_metering_points.groupBy(anonymised_mp_id_column_name)
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
    df_source_mp_table.select(F.col(energy_supplier_id_column_name).alias(tmp_balance_and_supplier_id_column_name))
    .union(
        df_source_mp_table.select(
            F.col(balance_responsible_id_column_name).alias(tmp_balance_and_supplier_id_column_name)
        )
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
                F.lit("4"),
                F.lpad(
                    F.row_number().over(window_random_order),
                    count_distinct_suppliers_and_balancers,
                    "0",
                ),
                F.lit("4"),
            ),
            13,
            "0",
        ),
    ).withColumn(
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
# MAGIC ### Create the anonymised metering_point table

# COMMAND ----------

# MAGIC %md
# MAGIC Join the anonymised metering points with the source MP table, and replace original columns (metering_point_id, parent_metering_point_id, balance_responsible_id, and energy_supplier_id)

# COMMAND ----------

df_source_mp_table_anonymised = (
    df_source_mp_table.join(df_anonymised_metering_points, [metering_point_id_column_name], "left")
    .withColumn(metering_point_id_column_name, F.col(anonymised_mp_id_column_name))
    .drop(anonymised_mp_id_column_name)
    .join(
        df_anonymised_metering_points.select(
            F.col(metering_point_id_column_name).alias(parent_metering_point_id_column_name),
            anonymised_mp_id_column_name,
        ),
        [parent_metering_point_id_column_name],
        "left",
    )
    .withColumn(parent_metering_point_id_column_name, F.col(anonymised_mp_id_column_name))
    .drop(anonymised_mp_id_column_name)
    .join(
        df_anonymised_suppliers_and_balancers,
        [
            (
                df_anonymised_suppliers_and_balancers[tmp_balance_and_supplier_id_column_name]
                == df_source_mp_table.energy_supplier_id
            )
            | (
                df_anonymised_suppliers_and_balancers[tmp_balance_and_supplier_id_column_name]
                == df_source_mp_table.balance_responsible_party_id
            )
        ],
        "left",
    )
    .withColumn(
        energy_supplier_id_column_name,
        F.col(anonymised_balance_or_supplier_id_column_name),
    )
    .drop(anonymised_balance_or_supplier_id_column_name)
    .join(
        df_anonymised_suppliers_and_balancers.select(
            F.col(tmp_balance_and_supplier_id_column_name).alias(balance_responsible_id_column_name),
            anonymised_balance_or_supplier_id_column_name,
        ),
        [balance_responsible_id_column_name],
        "left",
    )
    .withColumn(
        balance_responsible_id_column_name,
        F.col(anonymised_balance_or_supplier_id_column_name),
    )
    .drop(anonymised_balance_or_supplier_id_column_name)
    .select(df_source_mp_table.columns)
    .distinct()
).cache()

# COMMAND ----------

# MAGIC %md
# MAGIC Assert that:
# MAGIC 1. We have no duplicates of MP Ids in the anonymised table
# MAGIC 2. We have the same amount of unique MP Ids in the anonymised and source table
# MAGIC 3. We have the same amount of unique Parent MP Ids in the anonymised and source table
# MAGIC 4. We have the same amount of null Parent MP Ids in the anonymised and source table
# MAGIC 5. We have the same amount of unique Balance Responsible Ids in the anonymised and source table
# MAGIC 6. We have the same amount of null Parent MP Ids in the anonymised and source table
# MAGIC 7. We have the same amount of unique Energy Supplier Ids in the anonymised and source table
# MAGIC 8. We have the same amount of null Parent MP Ids in the anonymised and source table

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

assert (
    df_source_mp_table_anonymised.select(metering_point_id_column_name).distinct().count()
    == df_source_mp_table.select(metering_point_id_column_name).distinct().count()
)

# COMMAND ----------

assert (
    df_source_mp_table_anonymised.select(parent_metering_point_id_column_name).distinct().count()
    == df_source_mp_table.select(parent_metering_point_id_column_name).distinct().count()
)

# COMMAND ----------

assert (
    df_source_mp_table_anonymised.filter(F.col(parent_metering_point_id_column_name).isNull()).count()
    == df_source_mp_table.filter(F.col(parent_metering_point_id_column_name).isNull()).count()
)

# COMMAND ----------

assert (
    df_source_mp_table_anonymised.select(balance_responsible_id_column_name).distinct().count()
    == df_source_mp_table.select(balance_responsible_id_column_name).distinct().count()
)

# COMMAND ----------

assert (
    df_source_mp_table_anonymised.filter(F.col(balance_responsible_id_column_name).isNull()).count()
    == df_source_mp_table.filter(F.col(balance_responsible_id_column_name).isNull()).count()
)

# COMMAND ----------

assert (
    df_source_mp_table_anonymised.select(energy_supplier_id_column_name).distinct().count()
    == df_source_mp_table.select(energy_supplier_id_column_name).distinct().count()
)

# COMMAND ----------

assert (
    df_source_mp_table_anonymised.filter(F.col(energy_supplier_id_column_name).isNull()).count()
    == df_source_mp_table.filter(F.col(energy_supplier_id_column_name).isNull()).count()
)

# COMMAND ----------

# MAGIC %md
# MAGIC ### Create the anonymised time_series_points table
# MAGIC Beware, this might be a very costly operation, and as such might be better done with chunking or something.
# MAGIC However, since the current wholesale ts source table isn't partitioned, it is hard to do.
# MAGIC
# MAGIC All MPs in the list mps_to_anonymise will have their 'quantity' randomised to make them harder to identity. Throws an exception if no IDs are selected, as this is most likely an error.

# COMMAND ----------

if not mps_to_anonymise:
    raise Exception("Non MPs have been selected for having their quantity anoynmised")

df_source_ts_table_anonymised = (
    df_source_ts_table.withColumn(
        "quantity",
        F.when(F.col(metering_point_id_column_name).isin(mps_to_anonymise), F.rand() * 100).otherwise(
            F.col("quantity").cast(DecimalType(18, 6))
        ),
    )
    .join(df_anonymised_metering_points, metering_point_id_column_name)
    .withColumn(metering_point_id_column_name, F.col(anonymised_mp_id_column_name))
    .select(df_source_ts_table.columns)
).cache()

# COMMAND ----------

# MAGIC %md
# MAGIC Asser that:
# MAGIC 1. We have the same amount of MP Ids in the anonymised and source TS table.

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
    .withColumn(metering_point_id_column_name, F.col(anonymised_mp_id_column_name))
    .select(df_source_gl_table.columns)
)

# COMMAND ----------

# MAGIC %md
# MAGIC Assert that:
# MAGIC 1. We have the same amount of MP Ids in the anonymised and source GL table
# MAGIC 2. We have the same amount of null is MP Ids in the anonymised and source GL table

# COMMAND ----------

assert (
    df_source_gl_table_anonymised.select(metering_point_id_column_name).distinct().count()
    == df_source_gl_table.select(metering_point_id_column_name).distinct().count()
)

# COMMAND ----------

assert (
    df_source_gl_table_anonymised.filter(F.col(metering_point_id_column_name).isNull()).count()
    == df_source_gl_table.filter(F.col(metering_point_id_column_name).isNull()).count()
)

# COMMAND ----------

# MAGIC %md
# MAGIC Assert that:
# MAGIC 1. Overall row count for MP table is the same before and after anonymisation.
# MAGIC 2. Overall row count for TS table is the same before and after anonymisation.
# MAGIC 3. Overall row count for GL table is the same before and after anonymisation.

# COMMAND ----------

assert df_source_mp_table_anonymised.count() == df_source_mp_table.count()

# COMMAND ----------

assert df_source_ts_table_anonymised.count() == df_source_ts_table.count()

# COMMAND ----------

assert df_source_gl_table_anonymised.count() == df_source_gl_table.count()

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
