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

# What has changed with issue #32?
# - Store event data in the body column
#   - to avoid constantly growing parquet schema
#   - and to avoid data loss
# - Use stored time in parquet rather than enqueued time from EventHub as enqueued time might differ from stored time and thus cause errors when e.g. fetching basis data

# General (future?) considerations
# - In geh-timeseries: Why not enrich time series with metering point data (or simply publish the metering point data that was used for validation of the metering points)?
#   Then we would not have to deal with metering point events in wholesale and perhaps more important both domains will have the same perspective about the points
# - There seems to be a problem with the modelling of grid areas vs grid area links in the actor register.
#   How can a grid area have a (single) grid area link id when it's a one-to-many relation?

# Job parameters
from datetime import datetime

batch_id = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
batch_grid_areas = ["805", "806"]
snapshot_datetime = datetime.now()
period_start_datetime = datetime.strptime('31/07/2022 22:00', '%d/%m/%Y %H:%M')
period_end_datetime = datetime.strptime('1/08/2022 22:00', '%d/%m/%Y %H:%M')

# COMMAND ----------

# "Enums"
metering_point_type_production = 2

resolution_quarter = 2
resolution_hour = 1

connection_state_created = 1
connection_state_connected = 2

# COMMAND ----------

from pyspark.sql.functions import lit, when, col, lead, last, coalesce, explode, from_json, row_number, expr
import pyspark.sql.functions as F
from pyspark.sql.types import MapType, StringType, StructType, StructField, TimestampType
from pyspark.sql.window import Window

storage_account_name = "stdatalakesharedresu001" 
storage_account_key = "IOWWRAC8X96GuLcHzYXpHgiEh9QEHR9mkfGwLXstGB1E6Ds0AYX6Ay/dt475NqTnVbtikS70sOX1gKemd3V53Q=="

spark.conf.set("fs.azure.account.key.stdatalakesharedresu001.dfs.core.windows.net",  storage_account_key)

path = "abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/events"

# COMMAND ----------

# Debugging
#display(dbutils.fs.ls(f"{path}/year=2022/month=7/day=22"))

# COMMAND ----------

grid_area_event_schema = StructType(
    [
        StructField("GridAreaCode", StringType(), True),
        StructField("GridAreaLinkId", StringType(), True),
        StructField("EffectiveDate", TimestampType(), True),
        StructField("MessageType", StringType(), True),
    ]
)

grid_area_events_df = (spark.read.option("mergeSchema", "true").parquet(path)
  .withColumn("body", col("body").cast("string"))
  .withColumn("body", from_json(col("body"), grid_area_event_schema))
  .where(col("enqueuedTime") <= snapshot_datetime)
  .where(col("body.MessageType") == "GridAreaUpdatedIntegrationEvent")
  .where(col("body.GridAreaCode").isin(batch_grid_areas))
  .select("enqueuedTime", "body.MessageType", "body.GridAreaLinkId", "body.GridAreaCode")
)

# As we only use (currently) immutable data we can just pick any of the update events randomly.
# This will, however, change when support for merge of grid areas are added.
w2 = Window.partitionBy("GridAreaCode").orderBy(col("enqueuedTime"))
grid_area_events_df = (
    grid_area_events_df
    .withColumn("row",row_number().over(w2))
    .filter(col("row") == 1).drop("row") 
)

if(grid_area_events_df.count() != len(batch_grid_areas)):
    raise Error("Grid areas for processes in batch does not match the known grid areas in wholesale")

display(grid_area_events_df)

# COMMAND ----------

schema = StructType(
    [
        StructField("GsrnNumber", StringType(), True),
        StructField("GridAreaCode", StringType(), True),
        StructField("GridAreaId", StringType(), True),
        StructField("GridAreaLinkId", StringType(), True),
        StructField("SettlementMethod", StringType(), True),
        StructField("ConnectionState", StringType(), True),
        StructField("EffectiveDate", TimestampType(), True),
        StructField("MeteringPointType", StringType(), True),
        StructField("MeteringPointId", StringType(), True),
        StructField("Resolution", StringType(), True),
        StructField("CorrelationId", StringType(), True),
        StructField("MessageType", StringType(), True),
        StructField("OperationTime", TimestampType(), True),
    ]
)

# Debug
display(
  spark.read.option("mergeSchema", "true").parquet("abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/events")
  .withColumn("body", col("body").cast("string"))
  .withColumn("body", from_json(col("body"), schema))
  .where(col("body.MessageType").startswith("Meter"))
)

metering_point_events_df = (spark.read.option("mergeSchema", "true").parquet(path)
  .withColumn("body", col("body").cast("string"))
  .withColumn("body", from_json(col("body"), schema))
  .where(col("storedTime") <= snapshot_datetime)
  .where(col("body.MessageType").startswith("MeteringPoint"))
  .select("storedTime", "body.MessageType", "body.MeteringPointId", "body.GsrnNumber", "body.MeteringPointType", "body.GridAreaLinkId", "body.ConnectionState", "body.EffectiveDate", "body.Resolution"))

# Only include metering points in the selected grid areas
metering_point_events_df = (
    metering_point_events_df
    .join(grid_area_events_df, metering_point_events_df["GridAreaLinkId"] == grid_area_events_df["GridAreaLinkId"], "inner")
    .select(metering_point_events_df["MessageType"], "MeteringPointId", "GsrnNumber", "MeteringPointType", "GridAreaCode", "ConnectionState", "EffectiveDate", "Resolution")
)

display(metering_point_events_df)              

# COMMAND ----------

window = Window.partitionBy("MeteringPointId").orderBy("EffectiveDate")

metering_point_periods_df  = (metering_point_events_df
  .withColumn("toEffectiveDate", lead("EffectiveDate", 1, "2099-01-01T23:00:00.000+0000").over(window))
  .withColumn("GridAreaLinkId", coalesce(col("GridAreaLinkId"), last("GridAreaLinkId", True).over(window)))
  .withColumn("ConnectionState", when(col("MessageType") == "MeteringPointCreated", lit(connection_state_created))
                                .when(col("MessageType") == "MeteringPointConnected", lit(connection_state_connected)))
  .withColumn("MeteringPointType", coalesce(col("MeteringPointType"), last("MeteringPointType", True).over(window)))
  .withColumn("Resolution", coalesce(col("Resolution"), last("Resolution", True).over(window)))
 )

display(metering_point_periods_df)

# COMMAND ----------

timeseries_df = (spark.read.parquet("abfss://timeseries-data@stdatalakesharedresu001.dfs.core.windows.net/time-series-points/")
                 .where(col("storedTime") <= snapshot_datetime)
                 .where(col("time") >= period_start_datetime)
                 .where(col("time") < period_end_datetime)
                 # Quantity of time series points should have 3 digits. Calculations, however, must use 6 digit precision to reduce rounding errors
                 .withColumn("quantity", col("quantity").cast("decimal(18,6)"))
                )

display(timeseries_df)
                 
# Only use latest registered points
window = Window.partitionBy("metering_point_id", "time").orderBy(col("registration_date_time").desc())
timeseries_df = (timeseries_df
                 .withColumn("row_number", row_number().over(window))
                 .where(col("row_number") == 1)
                 .drop("row_number")
                )

timeseries_df = timeseries_df.select("metering_point_id", "time", "quantity", "quality")

display(timeseries_df)


# COMMAND ----------

# TODO: Use range join optimization: This query has a join condition that can benefit from range join optimization. To improve performance, consider adding a range join hint.
#       https://docs.microsoft.com/azure/databricks/delta/join-performance/range-join
timeseriesWithMeteringPoint = timeseries_df.join(metering_point_periods_df, (metering_point_periods_df["metering_point_id"] == timeseries_df["metering_point_id"]) &
               (timeseries_df["time"] >= metering_point_periods_df["EffectiveDate"]) &
               (timeseries_df["time"] < metering_point_periods_df["toEffectiveDate"]),
                                                 "left")

display(timeseriesWithMeteringPoint)

# COMMAND ----------

# TODO: Spørg Khatozen/Mads: Points are missing in the result if they are missing for all metering points at a certain time (behøver vi vist ikke at lave nu ifølge SME)

# TODO: Use range join optimization: This query has a join condition that can benefit from range join optimization. To improve performance, consider adding a range join hint.
#       https://docs.microsoft.com/azure/databricks/delta/join-performance/range-join

# Total production in batch grid areas with quarterly resolution as json file per grid area
result_df = (timeseriesWithMeteringPoint
      .where(col("MeteringPointType") == metering_point_type_production)
      .where(col("GridAreaCode").isin(batch_grid_areas))
      # TODO: Does this work correctly when daylight saving changes?
      .withColumn("quarter_times", when(col("resolution") == resolution_hour, array(col("time"), col("time") + expr('INTERVAL 15 minutes'), col("time") + expr('INTERVAL 30 minutes'), col("time") + expr('INTERVAL 45 minutes')))
                               .when(col("resolution") == resolution_quarter, array(col("time"))))
      #.select(timeseriesWithMeteringPoint["*"], explode("quantities").alias("quarter_quantity"))
      .select(timeseriesWithMeteringPoint["*"], explode("quarter_times").alias("quarter_time"))
      .withColumn("quarter_quantity", when(col("resolution") == resolution_hour, col("quantity") / 4)
                               .when(col("resolution") == resolution_quarter, col("quantity")))
      .groupBy("GridAreaCode", "quarter_time").sum("quarter_quantity")
     )

display(result_df)

# COMMAND ----------

window = Window.partitionBy("grid-area").orderBy(col("quarter_time"))

# TODO: Use range join optimization: This query has a join condition that can benefit from range join optimization. To improve performance, consider adding a range join hint.
#       https://docs.microsoft.com/azure/databricks/delta/join-performance/range-join

output_df = (result_df
 .withColumnRenamed("GridAreaCode", "grid-area")
 .withColumn("position", row_number().over(window))
 #.drop("quarter_time")
 .withColumnRenamed("sum(quarter_quantity)", "quantity")
 # RSM-014 requires 3 digits
 # TODO: consider if this should be handled in the sender instead
 #.withColumn("quantity", col("quantity").cast("decimal(18,3)"))
)

(output_df
 .repartition("grid-area")
 .write
 .partitionBy("grid-area")
 .json(f"abfss://processes@stdatalakesharedresu001.dfs.core.windows.net/results/batch-id={batch_id}")
)


# COMMAND ----------

display(dbutils.fs.head(f"abfss://integration-events@stdatalakesharedresu001.dfs.core.windows.net/processes/results/batch-id={batch_id}/grid-area=805/*.json"))
