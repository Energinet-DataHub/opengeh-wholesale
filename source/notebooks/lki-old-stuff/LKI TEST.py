# Databricks notebook source
from pyspark import SparkConf
from pyspark.sql.session import SparkSession

storage_account_name = 'stdatalakesharedresu001'
storage_account_key = '<INSERT>'

delta_lake_container_name = 'timeseries-data'
timeseries_processed_blob_name = 'timeseries-processed'
timeseries_unprocessed_blob_name = 'timeseries-unprocessed'

timeseries_processed = f'abfss://{delta_lake_container_name}@{storage_account_name}.dfs.core.windows.net/{timeseries_processed_blob_name}'
timeseries_unprocessed = f'abfss://{delta_lake_container_name}@{storage_account_name}.dfs.core.windows.net/{timeseries_unprocessed_blob_name}'

spark_conf = SparkConf(loadDefaults=True) \
        .set(f'fs.azure.account.key.{storage_account_name}.dfs.core.windows.net', storage_account_key) \
        .set('spark.sql.session.timeZone', 'UTC') \
        .set('spark.databricks.io.cache.enabled', 'True') \
        .set('spark.databricks.delta.formatCheck.enabled', 'False')

spark = SparkSession \
        .builder \
        .config(conf=spark_conf)\
        .getOrCreate()

# COMMAND ----------

from pyspark.sql.functions import *

up_df = spark.read.format("delta").load(timeseries_unprocessed)
jsonStringDataframe = up_df.select(up_df.body.cast(StringType()).alias("body"))
jsonStringDataframe.display(False)

# COMMAND ----------

from delta.tables import *

processed_table = DeltaTable.forPath(spark, timeseries_processed)

history = processed_table.history()
history.display()

# COMMAND ----------

from pyspark.sql.functions import *
from delta.tables import *

# processed_table = DeltaTable.forPath(spark, timeseries_processed)
# processed_table.delete()

df = spark.read.format('delta').load(timeseries_processed)

#df_all_but_first = df.filter(df('Time').gt(lit("2022-08-15T22:00:00")))
#df_all_but_first.display()
df.orderBy("metering_point_id", "time").display(100)



# COMMAND ----------

from pyspark.sql.functions import *
max_df = df \
    .groupBy() \
    .agg(
        min("time").alias("min_time"),
        max("time").alias("max_time")) \
    .withColumn("min_year", year("min_time")) \
    .withColumn("min_month", month("min_time")) \
    .withColumn("min_day", dayofmonth("min_time")) \
    .withColumn("max_year", year("max_time")) \
    .withColumn("max_month", month("max_time")) \
    .withColumn("max_day", dayofmonth("max_time"))
row = max_df.first()
print(row["min_year"])
print(row["min_month"])
print(row["min_day"])
print(row["max_year"])
print(row["max_month"])
print(row["max_day"])
max_df.display()

# COMMAND ----------

df.printSchema()

# COMMAND ----------

# MAGIC %md
# MAGIC # Overwrite data in TimeseriesProcessed

# COMMAND ----------

from pyspark.sql.types import StructType, StructField, StringType, TimestampType, IntegerType, DecimalType
from datetime import datetime

schema = StructType([
    StructField('metering_point_id', StringType(), True),
    StructField('transaction_id', StringType(), True),
    StructField('quantity', DecimalType(18,3), True),
    StructField('quality', IntegerType(), True),
    StructField('time', TimestampType(), True),
    StructField('resolution', IntegerType(), True),
    StructField('year', IntegerType(), True),
    StructField('month', IntegerType(), True),
    StructField('day', IntegerType(), True),
    StructField('registration_date_time', TimestampType(), True),
])

data = [
    (
        '579999993331812349',
        'C1875000',
        Decimal(1.444),
        4,
        datetime(2022, 8, 16, 2, 0, 0),
        2,
        2022,
        8,
        16,
        datetime(2022, 12, 17, 7, 50, 0)
    )
]
df_to_merge = spark.createDataFrame(data=data, schema=schema)
df_to_merge.display()
df_to_merge.write.format("delta").mode("overwrite").save(timeseries_processed)

# COMMAND ----------

# MAGIC %md
# MAGIC # Merge data in TimeseriesProcessed

# COMMAND ----------

# TODO:
# 1. where clause on existing data in timeseries_processed table to account for max and min year, month and day
# 2. make partition work 

from pyspark.sql.functions import *
from pyspark.sql.types import *
from delta.tables import *
from datetime import datetime
from decimal import Decimal

schema = StructType([
    StructField('metering_point_id', StringType(), True),
    StructField('transaction_id', StringType(), True),
    StructField('quantity', DecimalType(18,3), True),
    StructField('quality', IntegerType(), True),
    StructField('time', TimestampType(), True),
    StructField('resolution', IntegerType(), True),
    StructField('year', IntegerType(), True),
    StructField('month', IntegerType(), True),
    StructField('day', IntegerType(), True),
    StructField('registration_date_time', TimestampType(), True),
])

data = [
    (
        '579999993331812350',
        'C1875000',
        Decimal(1.444),
        4,
        datetime(2022, 8, 16, 2, 0, 0),
        2,
        2022,
        8,
        16,
        datetime(2022, 12, 17, 7, 50, 0)
    )
]

df_to_merge = spark.createDataFrame(data=data, schema=schema)
df_to_merge = df_to_merge.select(
    col('metering_point_id').alias('update_metering_point_id'),
    col('transaction_id').alias('update_transaction_id'),
    col('quantity').alias('update_quantity'),
    col('quality').alias('update_quality'),
    col('time').alias('update_time'),
    col('resolution').alias('update_resolution'),
    col('year').alias('update_year'),
    col('month').alias('update_month'),
    col('day').alias('update_day'),
    col('registration_date_time').alias('update_registration_date_time')
)

# Determine min and max year, month and day
min_max_df = df_to_merge \
    .groupBy() \
    .agg(
        min("update_time").alias("min_time"),
        max("update_time").alias("max_time")) \
    .withColumn("min_year", year("min_time")) \
    .withColumn("min_month", month("min_time")) \
    .withColumn("min_day", dayofmonth("min_time")) \
    .withColumn("max_year", year("max_time")) \
    .withColumn("max_month", month("max_time")) \
    .withColumn("max_day", dayofmonth("max_time"))
row = min_max_df.first()

# Fetch existing processed timeseries within min and max year, month and day
existing_df = spark.read.format("delta").load(timeseries_processed).where(f"(year >= {row['min_year']} AND month >= {row['min_month']} AND day >= {row['min_day']}) AND (year <= {row['max_year']} AND month <= {row['max_month']} AND day <= {row['max_day']})")

# Left join incomming dataframe with existing data on metering point id and time
determine_df = df_to_merge.join(existing_df, (df_to_merge["update_metering_point_id"] == existing_df["metering_point_id"]) & (df_to_merge["update_time"] == existing_df["time"]), "left")

# Determine if incomming data should be updated based on condition that checks that incomming data registration datetime is greater or equal to existing data
determine_df = determine_df.withColumn('should_update', (when(col('registration_date_time').isNotNull(), col('registration_date_time') <= col('update_registration_date_time')).otherwise(lit(False))).cast(BooleanType()))
determine_df.printSchema()

# Determine if incomming data should be inserted based on condition that "should_update" is False and there is no existing metering point in timeseries_processed table for the given time
to_insert = determine_df. \
    filter(col('should_update') == 'False') \
    .filter(col('metering_point_id').isNull()) \
    .select(
        col('update_metering_point_id').alias('metering_point_id'),
        col('update_transaction_id').alias('transaction_id'),
        col('update_quantity').alias('quantity'),
        col('update_quality').alias('quality'),
        col('update_time').alias('time'),
        col('update_resolution').alias('resolution'),
        col('update_year').alias('year'),
        col('update_month').alias('month'),
        col('update_day').alias('day'),
        col('update_registration_date_time').alias('registration_date_time')
    )

to_insert.display()

# # Filter out data that should be updated based on "should_update" column
# to_update = determine_df.filter(col('should_update') == True)
# 
# print("INSERT")
# to_insert.show()
# print("UPDATE")
# to_update.show()
# 
# # Insert data into timeseries_processed table
# to_insert \
#     .write \
#     .partitionBy(
#         "year",
#         "month",
#         "day") \
#     .format("delta") \
#     .mode("append") \
#     .save(timeseries_processed)
# 
# processed_table = DeltaTable.forPath(spark, timeseries_processed)
# 
# # Update existing data with incomming data
# processed_table.alias('source') \
#   .merge(
#     to_update.alias('updates'),
#     'source.metering_point_id = updates.metering_point_id AND source.time = updates.time'
#   ) \
#   .whenMatchedUpdate(set =
#     {
#         "transaction_id": "updates.update_transaction_id",
#         "quantity": "updates.update_quantity",
#         "quality": "updates.update_quality",
#         "resolution": "updates.update_resolution",
#         "registration_date_time": "updates.update_registration_date_time"
#     }
#   ) \
#   .execute()

# COMMAND ----------

from pyspark.sql.functions import *
from pyspark.sql.types import *
from delta.tables import *

df = spark.read.format('delta').load(timeseries_processed)

df = df.withColumn("expr", lit("INTERVAL 3 HOURS"))

df = df \
    .withColumn("added_hours",col("Time") + expr("expr")) \
    .withColumn("added_minutes",col("Time") + expr("INTERVAL 2 MINUTES")) \
    .withColumn("added_seconds",col("Time") + expr("INTERVAL 2 SECONDS")) \
    .withColumn("added_years",col("Time") + expr("INTERVAL 2 YEARS")) \
    .withColumn("added_days",col("Time") + expr("INTERVAL 2 DAYS")) \
    .withColumn("added_months",col("Time") + expr("INTERVAL 2 MONTHS")) \

df.display()

# COMMAND ----------

from pyspark.sql.functions import *
from pyspark.sql.types import *
from delta.tables import *

#processed_table = DeltaTable.forPath(spark, timeseries_processed)
#processed_table.delete()

df = spark.read.format('delta').load(timeseries_processed)


#.withColumn('time_epoch', col('Time').cast('long')) \
#.withColumn('time_from_epoch', to_utc_timestamp(to_timestamp(col('time_epoch')), 'UTC')) \
    

df = df \
    .withColumn('time_ms', unix_timestamp(col('Time')) * 1000) \
    .withColumn('time_ms_truncated_date', unix_timestamp(col('Time').cast(DateType())) * 1000) \
    .withColumn('time_diff_ms', col('time_ms') - col('time_ms_truncated_date')) \
    .withColumn('new_time', expr('add_months(time, 1)')) \
    .withColumn('new_time_ms', unix_timestamp(col("new_time")) * 1000 + col("time_diff_ms")) \
    .withColumn('final_time', to_utc_timestamp(to_timestamp(col('new_time_ms')/1000), 'UTC'))

df.display()


# COMMAND ----------


