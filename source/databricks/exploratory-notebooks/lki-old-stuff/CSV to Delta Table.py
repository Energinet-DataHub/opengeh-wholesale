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
# MAGIC %md # Initial setup

# COMMAND ----------

from pyspark.sql.types import (
    DecimalType,
    StructType,
    StructField,
    StringType,
    TimestampType,
    BooleanType,
)
from pyspark.sql.functions import year, month, dayofmonth, col

storage_account_name = (
    "datasharedresendku"  # this must be changed to your storage account name
)
storage_account_key = "INSERT KEY HERE"

shared_storage_account_name = (
    "stdatalakesharedresu002"  # this must be changed to your storage account name
)
shared_storage_account_key = "INSERT KEY HERE"

container_name = "data-lake"
shared_container_name = "data"

spark.conf.set(
    f"fs.azure.account.key.{storage_account_name}.dfs.core.windows.net",
    storage_account_key,
)
spark.conf.set(
    f"fs.azure.account.key.{shared_storage_account_name}.dfs.core.windows.net",
    shared_storage_account_key,
)

spark.conf.set("spark.databricks.delta.formatCheck.enabled", "False")

data_lake_path = (
    f"abfss://{container_name}@{storage_account_name}.dfs.core.windows.net/"
)
shared_data_lake_path = f"abfss://{shared_container_name}@{shared_storage_account_name}.dfs.core.windows.net/"

directory_path = "test-data/csv-files"
storage_path = f"abfss://{container_name}@{storage_account_name}.dfs.core.windows.net/{directory_path}/"

# COMMAND ----------


class Colname:
    added_grid_loss = "added_grid_loss"
    added_system_correction = "added_system_correction"
    aggregated_quality = "aggregated_quality"
    balance_responsible_id = "balance_responsible_id"
    charge_code = "charge_code"
    charge_key = "charge_key"
    charge_owner = "charge_owner"
    charge_price = "charge_price"
    charge_tax = "charge_tax"
    charge_type = "charge_type"
    connection_state = "connection_state"
    currency = "currency"
    date = "date"
    effective_date = "effective_date"
    end = "end"
    end_datetime = "end_datetime"
    energy_supplier_id = "energy_supplier_id"
    from_date = "from_date"
    grid_area = "grid_area"
    grid_loss = "grid_loss"
    in_grid_area = "in_grid_area"
    is_grid_loss = "is_grid_loss"
    is_system_correction = "is_system_correction"
    job_id = "job_id"
    metering_method = "metering_method"
    metering_point_id = "metering_point_id"
    metering_point_type = "metering_point_type"
    net_settlement_group = "net_settlement_group"
    out_grid_area = "out_grid_area"
    parent_metering_point_id = "parent_metering_point_id"
    price_per_day = "price_per_day"
    product = "product"
    quality = "quality"
    quantity = "quantity"
    resolution = "resolution"
    result_id = "result_id"
    result_name = "result_name"
    result_path = "result_path"
    settlement_method = "settlement_method"
    snapshot_id = "snapshot_id"
    start = "start"
    start_datetime = "start_datetime"
    charge_count = "charge_count"
    sum_quantity = "sum_quantity"
    time = "time"
    time_window = "time_window"
    time_window_end = "time_window.end"
    time_window_start = "time_window.start"
    to_date = "to_date"
    total_daily_charge_price = "total_daily_charge_price"
    total_amount = "total_amount"
    total_quantity = "total_quantity"
    unit = "unit"
    gsrn_number = "gsrn_number"


# COMMAND ----------

# MAGIC %md #TimeSeries

# COMMAND ----------

ts_csv_path = f"{storage_path}/TimeSeries(Auto).csv"
ts_delta_path = f"{shared_data_lake_path}/timeseries"

ts_schema = StructType(
    [
        StructField(Colname.metering_point_id, StringType(), False),
        StructField(Colname.quantity, DecimalType(18, 3), False),
        StructField(Colname.quality, StringType(), False),
        StructField(Colname.time, TimestampType(), False),
    ]
)

ts_df = (
    spark.read.format("csv").option("header", True).schema(ts_schema).load(ts_csv_path)
)

ts_df = (
    ts_df.withColumn("year", year(col(Colname.time)))
    .withColumn("month", month(col(Colname.time)))
    .withColumn("day", dayofmonth(col(Colname.time)))
)

ts_df.write.format("delta").mode("overwrite").partitionBy("year", "month", "day").save(
    ts_delta_path
)

ts_df.display()

# COMMAND ----------

# MAGIC %md # MeteringPoint

# COMMAND ----------

mp_csv_path = f"{storage_path}/MeteringPoints(Master).csv"
mp_delta_path = f"{data_lake_path}/master-data/metering-points"

mp_schema = StructType(
    [
        StructField(Colname.metering_point_id, StringType(), False),
        StructField(Colname.metering_point_type, StringType(), False),
        StructField(Colname.settlement_method, StringType()),
        StructField(Colname.grid_area, StringType(), False),
        StructField(Colname.connection_state, StringType(), False),
        StructField(Colname.resolution, StringType(), False),
        StructField(Colname.in_grid_area, StringType()),
        StructField(Colname.out_grid_area, StringType()),
        StructField(Colname.metering_method, StringType(), False),
        StructField(Colname.parent_metering_point_id, StringType()),
        StructField(Colname.unit, StringType(), False),
        StructField(Colname.product, StringType()),
        StructField(Colname.from_date, TimestampType(), False),
        StructField(Colname.to_date, TimestampType(), False),
    ]
)

mp_df = (
    spark.read.format("csv").option("header", True).schema(mp_schema).load(mp_csv_path)
)

mp_df.write.format("delta").mode("overwrite").partitionBy(
    Colname.metering_point_id
).save(mp_delta_path)

mp_df.display()

# COMMAND ----------

# MAGIC %md # MarketRole

# COMMAND ----------

mr_csv_path = f"{storage_path}/MarketRoles(Master).csv"
mr_delta_path = f"{data_lake_path}/master-data/market-roles"

mr_schema = StructType(
    [
        StructField(Colname.energy_supplier_id, StringType(), False),
        StructField(Colname.metering_point_id, StringType(), False),
        StructField(Colname.from_date, TimestampType(), False),
        StructField(Colname.to_date, TimestampType(), False),
    ]
)

mr_df = (
    spark.read.format("csv").option("header", True).schema(mr_schema).load(mr_csv_path)
)

mr_df.write.format("delta").mode("overwrite").partitionBy(
    Colname.energy_supplier_id
).save(mr_delta_path)

mr_df.display()

# COMMAND ----------

# MAGIC %md # Charges

# COMMAND ----------

ch_csv_path = f"{storage_path}/Charges(Master).csv"
ch_delta_path = f"{data_lake_path}/master-data/charges"

ch_schema = StructType(
    [
        StructField(Colname.charge_key, StringType(), False),
        StructField(Colname.charge_code, StringType(), False),
        StructField(Colname.charge_type, StringType(), False),
        StructField(Colname.charge_owner, StringType(), False),
        StructField(Colname.resolution, StringType(), False),
        StructField(Colname.charge_tax, StringType(), False),
        StructField(Colname.currency, StringType(), False),
        StructField(Colname.from_date, TimestampType(), False),
        StructField(Colname.to_date, TimestampType(), False),
    ]
)

ch_df = (
    spark.read.format("csv").option("header", True).schema(ch_schema).load(ch_csv_path)
)

ch_df.write.format("delta").mode("overwrite").partitionBy(Colname.charge_key).save(
    ch_delta_path
)

ch_df.display()

# COMMAND ----------

# MAGIC %md # ChargeLinks

# COMMAND ----------

chlk_csv_path = f"{storage_path}/ChargeLinks(Auto).csv"
chlk_delta_path = f"{data_lake_path}/master-data/charge-links"

chlk_schema = StructType(
    [
        StructField(Colname.charge_key, StringType(), False),
        StructField(Colname.metering_point_id, StringType(), False),
        StructField(Colname.from_date, TimestampType(), False),
        StructField(Colname.to_date, TimestampType(), False),
    ]
)

chlk_df = (
    spark.read.format("csv")
    .option("header", True)
    .schema(chlk_schema)
    .load(chlk_csv_path)
)

chlk_df.write.format("delta").mode("overwrite").partitionBy(Colname.charge_key).save(
    chlk_delta_path
)

chlk_df.display()

# COMMAND ----------

# MAGIC %md # ChargePrices

# COMMAND ----------

chpr_csv_path = f"{storage_path}/ChargePrices(Auto).csv"
chpr_delta_path = f"{data_lake_path}/master-data/charge-prices"

chpr_schema = StructType(
    [
        StructField(Colname.charge_key, StringType(), False),
        StructField(Colname.charge_price, DecimalType(18, 8), False),
        StructField(Colname.time, TimestampType(), False),
    ]
)

chpr_df = (
    spark.read.format("csv")
    .option("header", True)
    .schema(chpr_schema)
    .load(chpr_csv_path)
)

chpr_df.write.format("delta").mode("overwrite").partitionBy(Colname.charge_key).save(
    chpr_delta_path
)

chpr_df.display()

# COMMAND ----------

# MAGIC %md # ES/BRP Relations

# COMMAND ----------

esbrp_csv_path = f"{storage_path}/ES & BRP Relations(Master).csv"
esbrp_delta_path = f"{data_lake_path}/master-data/es-brp-relations"

esbrp_schema = StructType(
    [
        StructField(Colname.energy_supplier_id, StringType(), False),
        StructField(Colname.balance_responsible_id, StringType(), False),
        StructField(Colname.grid_area, StringType(), False),
        StructField(Colname.metering_point_type, StringType(), False),
        StructField(Colname.from_date, TimestampType(), False),
        StructField(Colname.to_date, TimestampType(), False),
    ]
)

esbrp_df = (
    spark.read.format("csv")
    .option("header", True)
    .schema(esbrp_schema)
    .load(esbrp_csv_path)
)

esbrp_df.write.format("delta").mode("overwrite").save(esbrp_delta_path)

esbrp_df.display()

# COMMAND ----------

# MAGIC %md # GridLoss/SystemCorrection

# COMMAND ----------

grsc_csv_path = f"{storage_path}/GL&SKMP(Master).csv"
grsc_delta_path = f"{data_lake_path}/master-data/grid-loss-system-correction"

grsc_schema = StructType(
    [
        StructField(Colname.metering_point_id, StringType(), False),
        StructField(Colname.grid_area, StringType(), False),
        StructField(Colname.energy_supplier_id, StringType(), False),
        StructField(Colname.is_grid_loss, BooleanType(), False),
        StructField(Colname.is_system_correction, BooleanType(), False),
        StructField(Colname.from_date, TimestampType(), False),
        StructField(Colname.to_date, TimestampType(), False),
    ]
)

grsc_df = (
    spark.read.format("csv")
    .option("header", True)
    .schema(grsc_schema)
    .load(grsc_csv_path)
)

grsc_df.write.format("delta").mode("overwrite").partitionBy(
    Colname.metering_point_id
).save(grsc_delta_path)

grsc_df.display()

# COMMAND ----------
