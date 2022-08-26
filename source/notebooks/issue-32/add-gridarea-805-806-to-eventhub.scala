// Databricks notebook source
//addd com.microsoft.azure:azure-eventhubs-spark_2.12:2.3.22 to cluster
import org.apache.spark.sql.functions._
import spark.implicits._
import org.apache.spark.sql.types.{MapType, StringType, StructType, TimestampType}
import org.apache.spark.sql.expressions.Window
import org.apache.spark.eventhubs.ConnectionStringBuilder

import org.apache.spark.eventhubs.{ ConnectionStringBuilder, EventHubsConf, EventPosition }

var storageKey = sys.env("STORAGE_ACCOUNT_KEY")

spark.conf.set("fs.azure.account.key.stdatalakesharedresu001.dfs.core.windows.net",  storageKey)

// COMMAND ----------

var sasKey = sys.env("SAS_KEY")

val connectionString = ConnectionStringBuilder()
  .setNamespaceName("evhns-wholesale-wholsal-u-001")
  .setEventHubName("masterdataevents")
  .setSasKeyName("manage")
  .setSasKey(sasKey)
  .build

val ehConf = EventHubsConf(connectionString).setStartingPosition(EventPosition.fromStartOfStream)

println(ehConf)


// COMMAND ----------

val schema = new StructType()
    .add("GsrnNumber", StringType,true)
    .add("GridAreaLinkId", StringType,true)
    .add("SettlementMethod", StringType,true)
    .add("ConnectionState", StringType,true)
    .add("EffectiveDate", TimestampType,true)
    .add("MeteringPointType", StringType,true)
    .add("Resolution", StringType,true)
    .add("CorrelationId", StringType,true)
    .add("MessageType", StringType,true)
    .add("OperationTime", TimestampType,true)

val dfStream = spark
  .readStream
  .format("eventhubs")
  //.option("startingOffsets", "earliest")
  .options(ehConf.toMap)
  .load()
  
val raw = dfStream
  .withColumn("body", from_json($"body" cast "string", schema))
  .select(
     col("*"),
     col("body.*")
   )
  .drop("body")
  .withColumn("year", year(col("enqueuedTime")))
  .withColumn("month", month(col("enqueuedTime")))
  .withColumn("day", dayofmonth(col("enqueuedTime")))
  
display(raw)

// COMMAND ----------

val df = spark.read.parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/gridareaEvents-805-806")
.withColumn("body", col("body")
.cast("string"))
.select("body")
//.toJSON
//.selectExpr("value as body")

display(df)

// COMMAND ----------


df
.write.format("eventhubs").options(ehConf.toMap).
save()

// COMMAND ----------

dbutils.fs.rm("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/events", True)
dbutils.fs.rm("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/events-checkpoint/", True)
