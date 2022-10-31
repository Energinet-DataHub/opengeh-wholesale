# Databricks notebook source
from pyspark.sql.types import StringType

storage_account_name = "stdatalakesharedresu001" # this must be changed to your storage account name
storage_account_key = "<INSERT>"
container_name = "timeseries-data"
spark.conf.set(
  f"fs.azure.account.key.{storage_account_name}.dfs.core.windows.net",
  storage_account_key)

delta_path = f"abfss://{container_name}@{storage_account_name}.dfs.core.windows.net/timeseries-unprocessed/"
delta_path_processed = f"abfss://{container_name}@{storage_account_name}.dfs.core.windows.net/timeseries-processed/"
print(delta_path)
read_df = spark.read.format("delta").load(delta_path)
#read_df.display()
jsonDataFrame = read_df.select(read_df.body.cast(StringType()).alias("body"))
# jsonDataFrame.display()


# COMMAND ----------

from pyspark.sql.functions import from_json,explode,date_add,when,col,to_timestamp,expr
from pyspark.sql.types import StringType,StructType,StructField,ArrayType
from delta.tables import *


#schema = StructType([StructField("Document", StringType()),StructField("Series", StringType())])

schema = StructType([
      # StructField('Document', StructType([
      #      StructField('Id', StringType(), True),
      #      StructField('Sender', StructType([
      #        StructField('Id', StringType(), True),
      #        StructField('BusinessProcessRole', StringType(), True)
      #        ]), True)]),True),
      StructField('Series', ArrayType(StructType([
                #StructField('Id', StringType(), True),
                StructField('TransactionId', StringType(), True),
                StructField('MeteringPointId', StringType(), True),
                #StructField('MeteringPointType', StringType(), True),
                #StructField('RegistrationDateTime', StringType(), True),
                #StructField('Product', StringType(), True),
                StructField('Period', StructType([
                  StructField('Resolution', StringType(), True),
                  StructField('StartDateTime', StringType(), True),
                  #StructField('EndDateTime', StringType(), True),
                  StructField('Points', ArrayType(StructType([
                     StructField('Quantity', StringType(), True),
                     StructField('Quality', StringType(), True),
                     StructField('Position', StringType(), True),
                  ])), True),
                  ]), True)
                ]),True))
         ])

struct = jsonDataFrame.select(from_json(jsonDataFrame.body,schema).alias('json'))
#struct.display()

flat = struct. \
       select(explode("json.Series")). \
       select("col.MeteringPointId","col.TransactionId", "col.Period"). \
       select(col("MeteringPointId"),col("TransactionId"),to_timestamp(col("Period.StartDateTime")).alias("StartDateTime"),col("Period.Resolution").alias("Resolution"),explode("Period.Points").alias("Period_Point")). \
       select("*","Period_Point.Quantity","Period_Point.Quality","Period_Point.Position"). \
       drop("Period_Point") 
#         QuarterOfHour = 1, Hour = 2, Day = 3, Month = 4,
withResolutionInMinutes = flat.withColumn("ClockResolution",when(col("Resolution") == 1, 15).when(col("Resolution") == 2, 60).when(col("Resolution") == 3, 1440).when(col("Resolution") == 4, 43800)).drop("Resolution")
# resolution should be pulled from enum, how do we handle month resolution ?
timeToAdd = withResolutionInMinutes.withColumn("TimeToAdd",(col("Position") -1 ) * col("ClockResolution")).drop("ClockResolution","Position")
withTime = timeToAdd.withColumn("Time", expr("StartDateTime + make_interval(0, 0, 0, 0, 0, TimeToAdd, 0)")).drop("StartDateTime").drop("TimeToAdd")


isTable = DeltaTable.isDeltaTable(spark, delta_path_processed)
print(isTable)
if not isTable:
  print("d")
  #withTime.write.format("delta").option("path", delta_path_processed).saveAsTable("timeseries")

  withTime.write \
    .partitionBy("MeteringPointId") \
    .format("delta") \
    .mode("append") \
   .save(delta_path_processed)
#  DeltaTable.create(spark).location(delta_path_processed).addColumns(withTime.schema).execute()
#DeltaTable.create(spark) \
#  .tableName("timeseries") \
#  .addColumn("id", "INT") \
#  .addColumn("firstName", "STRING") \
#  .addColumn("middleName", "STRING") \
#  .addColumn("lastName", "STRING", comment = "surname") \
#  .addColumn("gender", "STRING") \
#  .addColumn("birthDate", "TIMESTAMP") \
#  .addColumn("ssn", "STRING") \
#  .addColumn("salary", "INT") \
#  .partitionedBy("gender") \
#  .execute()

  
else:
  print("Found DT")
#  spark \
#    .read.format("delta") \
#    .load(delta_path_processed)
#  withTime.display()
  delta_table = DeltaTable.forPath(spark, delta_path_processed)
  delta_table.alias("target").merge(
      source = withTime.alias("source"),
      condition = "target.MeteringPointId = source.MeteringPointId AND target.Time = source.Time"
    ).whenMatchedUpdate(set =
      {
        "TransactionId": "source.TransactionId",
        "Quantity":  "source.Quantity",
        "Quality":  "source.Quality",
        "Time":  "source.Time",
      }
    ).whenNotMatchedInsert(values =
      {
        "MeteringPointId": "source.MeteringPointId",
        "TransactionId": "source.TransactionId",
        "Time":  "source.Time",
        "Quantity":  "source.Quantity",
        "Quality":  "source.Quality",
      }
    ).execute()

# COMMAND ----------

delta_table = DeltaTable.forPath(spark, delta_path_processed)
delta_table.toDF()

# COMMAND ----------

  newDf = spark \
    .read.format("delta") \
    .load(delta_path)
  newDf.display()

# COMMAND ----------


