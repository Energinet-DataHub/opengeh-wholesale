# Databricks notebook source
import package.constants.time_series_wholesale_col_name as time_series_wholesale_col_name
import package.constants.metering_point_wholesale_col_name as metering_point_wholesale_col_name

from pyspark.sql.window import Window
import pyspark.sql.functions as F

# COMMAND ----------

database = "..."
source_mp_table_name = "metering_point_periods"
source_ts_table_name = "time_series_points"
source_gl_table_name = "grid_loss_metering_points"
source_mp_table = spark.read.table(f"{database}.{source_mp_table_name}")
source_ts_table = spark.read.table(f"{database}.{source_ts_table_name}")
source_gl_table = spark.read.table(f"{database}.{source_gl_table_name}")

target_database = database
target_mp_table_name = "metering_point_periods_performance_test"
target_ts_table_name = "time_series_points_performance_test"
target_ts_table_name = "grid_loss_metering_points_performance_test"

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

w = Window.orderBy(F.rand())
anonymised_metering_points = (
    all_metering_point_ids.withColumn(
        "anonymised_mp_id", F.rpad(F.rank().over(w), 18, "0")
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

all_supplier_and_balancers = (
    source_mp_table.select(metering_point_wholesale_col_name.energy_supplier_id)
    .union(
        source_mp_table.select(metering_point_wholesale_col_name.balance_responsible_id)
    )
    .distinct()
)

w = Window.orderBy(F.rand())
anonymised_suppliers_and_balancers = (
    all_supplier_and_balancers.withColumn(
        "anonymised_balance_or_supplier_id", F.rpad(F.rank().over(w), 13, "0")
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

source_mp_table_anonymised.write.format("delta").mode("append").saveAsTable(
    f"{target_database}.{target_mp_table_name}"
)

# COMMAND ----------

# MAGIC %md
# MAGIC ### Create the anonymised time_series_points table
# MAGIC Beware, this might be a very costly operation, and as such might be better done with chunking or something.
# MAGIC However, since the current wholesale ts source table isn't partitioned, it is hard to do.

# COMMAND ----------

source_ts_table_anonymised = (
    source_ts_table.join(anonymised_metering_points, "metering_point_id").withColumn(
        "metering_point_id", F.col("anonymised_mp_id")
    )
    .select(source_ts_table.columns)
)

# COMMAND ----------

source_ts_table_anonymised.write.format("delta").mode("append").saveAsTable(
    f"{target_database}.{target_ts_table_name}"
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

source_gl_table_anonymised.write.format("delta").mode("append").saveAsTable(
    f"{target_database}.{target_gl_table_name}"
)
