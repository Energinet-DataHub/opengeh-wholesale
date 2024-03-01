# Databricks notebook source

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

# COMMAND ----------

# MAGIC %md
# MAGIC ### Anoymise dataset for test environments
# MAGIC This notebook transforms the data from the b001 to an anonymised version that could be transfered to test-001 environment.
# MAGIC We use this method to ensure we can not recalculate the anonymised ids via any function and the data in the test environment is safe
# MAGIC We should keep this notebook to ensure we can anonymise data if it should be needed again.

# COMMAND ----------
import package.constants.time_series_wholesale_col_name as time_series_wholesale_col_name
import package.constants.metering_point_wholesale_col_name as metering_point_wholesale_col_name
import pyspark.sql.functions as F
from pyspark.sql.window import Window

database = "..."
source_mp_table_name = "metering_point_periods"
source_ts_table_name = "time_series_points"
source_gl_table_name = "grid_loss_metering_points"
#
source_mp_table = (
    spark.read.table(f"{database}.{source_mp_table_name}")
    .filter("'2010-08-19T22:00:00Z' <= from_date")
    .filter("from_date <= '2023-06-01T21:00:00Z'")
    .filter("'2010-08-19T22:00:00Z' <= to_date")
    .filter("to_date <= '2023-06-01T21:00:00Z'")
)

source_ts_table = (
    spark.read.table(f"{database}.{source_ts_table_name}")
    .filter("'2010-08-19T22:00:00Z' <= observation_time")
    .filter("observation_time <= '2023-06-01T21:00:00Z'")
)

source_gl_table = spark.read.table(f"{database}.{source_gl_table_name}")

target_database = database
target_mp_table_name = "metering_point_periods_performance_test"
target_ts_table_name = "time_series_points_performance_test"
target_gl_table_name = "grid_loss_metering_points_performance_test"

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
# MAGIC ### Step 2: Find and anonymised all metering_point_id's and energy_supplier_ids + balance_supplier_ids

# COMMAND ----------

all_metering_point_ids = (
    source_ts_table.select(time_series_wholesale_col_name.metering_point_id)
    .union(source_mp_table.select(metering_point_wholesale_col_name.metering_point_id))
    .union(
        source_mp_table.select(
            metering_point_wholesale_col_name.parent_metering_points_id
        )
    )
    .distinct()
)

df_count = len(str(all_metering_point_ids.count()))
w = Window.orderBy(F.rand())
anonymised_metering_points = (
    all_metering_point_ids.withColumn(
        "anonymised_mp_id",
        F.rpad(
            F.concat(
                F.lit("5"), F.lpad(F.row_number().over(w), df_count, "0"), F.lit("5")
            ),
            18,
            "0",
        ),
    )
    .withColumn(
        "anonymised_mp_id",
        F.when(
            F.col(time_series_wholesale_col_name.metering_point_id).isNull(),
            F.lit(None),
        ).otherwise(F.col("anonymised_mp_id")),
    )
    .na.drop()
)

# COMMAND ----------

assert (
    anonymised_metering_points.groupBy("anonymised_mp_id")
    .agg(F.sum(F.lit(1)).alias("C"))
    .filter("C > 1")
    .count()
    == 0
)

# COMMAND ----------

all_supplier_and_balancers = (
    source_mp_table.select(metering_point_wholesale_col_name.energy_supplier_id)
    .union(
        source_mp_table.select(metering_point_wholesale_col_name.balance_responsible_id)
    )
    .distinct()
)

df_count = len(str(all_supplier_and_balancers.count()))
w = Window.orderBy(F.rand())
anonymised_suppliers_and_balancers = (
    all_supplier_and_balancers.withColumn(
        "anonymised_balance_or_supplier_id",
        F.rpad(
            F.concat(
                F.lit("4"), F.lpad(F.row_number().over(w), df_count, "0"), F.lit("4")
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
)
# COMMAND ----------

assert (
    anonymised_suppliers_and_balancers.groupBy("anonymised_balance_or_supplier_id")
    .agg(F.sum(F.lit(1)).alias("C"))
    .filter("C > 1")
    .count()
    == 0
)

# COMMAND ----------

# MAGIC %md
# MAGIC ### Create the anonymised metering_point table

# COMMAND ----------

source_mp_table_anonymised = (
    source_mp_table.join(anonymised_metering_points, ["metering_point_id"], "left")
    .withColumn("metering_point_id", F.col("anonymised_mp_id"))
    .drop("anonymised_mp_id")
    .join(
        anonymised_metering_points.select(
            F.col("metering_point_id").alias("parent_metering_point_id"),
            "anonymised_mp_id",
        ),
        ["parent_metering_point_id"],
        "left",
    )
    .withColumn("parent_metering_point_id", F.col("anonymised_mp_id"))
    .drop("anonymised_mp_id")
    .join(anonymised_suppliers_and_balancers, ["energy_supplier_id"], "left")
    .withColumn("energy_supplier_id", F.col("anonymised_balance_or_supplier_id"))
    .drop("anonymised_balance_or_supplier_id")
    .join(
        anonymised_suppliers_and_balancers.select(
            F.col("energy_supplier_id").alias("balance_responsible_id"),
            "anonymised_balance_or_supplier_id",
        ),
        ["balance_responsible_id"],
        "left",
    )
    .withColumn("balance_responsible_id", F.col("anonymised_balance_or_supplier_id"))
    .drop("anonymised_balance_or_supplier_id")
    .select(source_mp_table.columns)
)

# COMMAND ----------

assert (
    source_mp_table_anonymised.select("metering_point_id", "type")
    .distinct()
    .groupBy("metering_point_id")
    .agg(F.sum(F.lit(1)).alias("C"))
    .filter("C > 1")
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

source_ts_table_anonymised = (
    source_ts_table.join(anonymised_metering_points, "metering_point_id")
    .withColumn("metering_point_id", F.col("anonymised_mp_id"))
    .withColumn(
        "quantity",
        F.when(
            F.col("metering_point_id").isin(mps_to_anonymise), F.rand(seed=42) * 100
        ).otherwise(F.col("quantity")),
    )
    .select(source_ts_table.columns)
)


# COMMAND ----------

assert (
    source_ts_table_anonymised.select("metering_point_id").distinct().count()
    == source_ts_table.select("metering_point_id").distinct().count()
)

# COMMAND ----------

# MAGIC %md
# MAGIC ### Create the anonymised grid_loss_metering_points table

# COMMAND ----------

source_gl_table_anonymised = (
    source_gl_table.join(anonymised_metering_points, "metering_point_id")
    .withColumn("metering_point_id", F.col("anonymised_mp_id"))
    .select(source_gl_table.columns)
)

# COMMAND ----------

assert (
    source_gl_table_anonymised.select("metering_point_id").distinct().count()
    == source_gl_table_anonymised.select("metering_point_id").distinct().count()
)

# COMMAND ----------

source_mp_table_anonymised.write.format("delta").mode("append").saveAsTable(
    f"{target_database}.{target_mp_table_name}"
)
# COMMAND ----------

source_ts_table_anonymised.write.format("delta").mode("append").saveAsTable(
    f"{target_database}.{target_ts_table_name}"
)

# COMMAND ----------

source_gl_table_anonymised.write.format("delta").mode("append").saveAsTable(
    f"{target_database}.{target_gl_table_name}"
)
