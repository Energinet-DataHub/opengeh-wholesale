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

from geh_stream.codelists import Colname
from argparse import Namespace
from pyspark.sql.dataframe import DataFrame
from pyspark import SparkConf
from pyspark.sql.session import SparkSession
import pyspark.sql.functions as F
from pyspark.sql.functions import col
from pyspark.sql.window import Window
from geh_stream.shared.filters import filter_on_date, filter_on_period, filter_on_grid_areas, time_series_points_where_date_condition
from typing import List
from geh_stream.shared.services import StorageAccountService
from geh_stream.shared.period import Period, parse_period


def initialize_spark(args):
    # Set spark config with storage account names/keys and the session timezone so that datetimes are displayed consistently (in UTC)
    spark_conf = (SparkConf(loadDefaults=True)
                  .set("spark.sql.session.timeZone", "UTC")
                  .set("spark.databricks.io.cache.enabled", "True")
                  .set("spark.databricks.delta.formatCheck.enabled", "False")
                  .set(f'fs.azure.account.key.{args.shared_storage_account_name}.dfs.core.windows.net', args.shared_storage_account_key))

    return SparkSession \
        .builder\
        .config(conf=spark_conf)\
        .getOrCreate()


def __load_delta_data(spark: SparkSession, storage_base_path: str, delta_table_path: str, where_condition: str = None) -> DataFrame:
    path = StorageAccountService.get_storage_account_full_path(storage_base_path, delta_table_path)
    df = spark \
        .read \
        .format("delta") \
        .load(path)

    if where_condition is not None:
        df = df.where(where_condition)

    return df


def __load_from_sql_table(spark: SparkSession, args: Namespace, table_name: str) -> DataFrame:
    return (spark
            .read
            .format("com.microsoft.sqlserver.jdbc.spark")
            .option("url", f"jdbc:sqlserver://{args.shared_database_url};databaseName={args.shared_database_aggregations};")
            .option("dbtable", table_name)
            .option("user", args.shared_database_username)
            .option("password", args.shared_database_password)
            .load())


def load_metering_points(beginning_date_time, end_date_time, args: Namespace, spark: SparkSession, grid_areas: List[str]) -> DataFrame:
    df = (__load_from_sql_table(spark, args, "MeteringPoint")
          .withColumnRenamed("MeteringPointId", Colname.metering_point_id)
          .withColumnRenamed("MeteringPointType", Colname.metering_point_type)
          .withColumnRenamed("SettlementMethod", Colname.settlement_method)
          .withColumnRenamed("GridArea", Colname.grid_area)
          .withColumnRenamed("ConnectionState", Colname.connection_state)
          .withColumnRenamed("Resolution", Colname.resolution)
          .withColumnRenamed("InGridArea", Colname.in_grid_area)
          .withColumnRenamed("OutGridArea", Colname.out_grid_area)
          .withColumnRenamed("MeteringMethod", Colname.metering_method)
          .withColumnRenamed("ParentMeteringPointId", Colname.parent_metering_point_id)
          .withColumnRenamed("Unit", Colname.unit)
          .withColumnRenamed("Product", Colname.product)
          .withColumnRenamed("FromDate", Colname.from_date)
          .withColumnRenamed("ToDate", Colname.to_date))
    df = filter_on_period(df, parse_period(beginning_date_time, end_date_time))
    df = filter_on_grid_areas(df, Colname.grid_area, grid_areas)
    return df


def load_grid_loss_sys_corr(args: Namespace, spark: SparkSession, grid_areas: List[str]) -> DataFrame:
    df = (__load_from_sql_table(spark, args, "GridLossSysCorr")
          .withColumnRenamed("MeteringPointId", Colname.metering_point_id)
          .withColumnRenamed("GridArea", Colname.grid_area)
          .withColumnRenamed("EnergySupplierId", Colname.energy_supplier_id)
          .withColumnRenamed("IsGridLoss", Colname.is_grid_loss)
          .withColumnRenamed("IsSystemCorrection", Colname.is_system_correction)
          .withColumnRenamed("FromDate", Colname.from_date)
          .withColumnRenamed("ToDate", Colname.to_date))
    df = filter_on_period(df, parse_period(args.beginning_date_time, args.end_date_time))
    df = filter_on_grid_areas(df, Colname.grid_area, grid_areas)
    return df


def load_market_roles(args: Namespace, spark: SparkSession) -> DataFrame:
    columns = [Colname.energy_supplier_id, Colname.metering_point_id, Colname.from_date, Colname.to_date]
    hardcoded_energy_suppliers = [("42", "some-mp-id", "2010-01-01T00:00:00Z", "2021-01-01T00:00:00Z")]
    df = spark.sparkContext.parallelize(hardcoded_energy_suppliers).toDF(columns)
    df = df.withColumn(Colname.from_date, F.to_timestamp(Colname.from_date))
    df = df.withColumn(Colname.to_date, F.to_timestamp(Colname.to_date))
    df = filter_on_period(df, parse_period(args.beginning_date_time, args.end_date_time))
    return df


def load_charges(args: Namespace, spark: SparkSession) -> DataFrame:
    df = (__load_from_sql_table(spark, args, "Charge")
          .withColumnRenamed("ChargeKey", Colname.charge_key)
          .withColumnRenamed("ChargeId", Colname.charge_id)
          .withColumnRenamed("ChargeOwner", Colname.charge_owner)
          .withColumnRenamed("ChargeType", Colname.charge_type)
          .withColumnRenamed("Resolution", Colname.resolution)
          .withColumnRenamed("ChargeTax", Colname.charge_tax)
          .withColumnRenamed("Currency", Colname.currency)
          .withColumnRenamed("FromDate", Colname.from_date)
          .withColumnRenamed("ToDate", Colname.to_date))
    return filter_on_period(df, parse_period(args.beginning_date_time, args.end_date_time))


def load_charge_links(args: Namespace, spark: SparkSession) -> DataFrame:
    df = (__load_from_sql_table(spark, args, "ChargeLink")
          .withColumnRenamed("ChargeKey", Colname.charge_key)
          .withColumnRenamed("MeteringPointId", Colname.metering_point_id)
          .withColumnRenamed("FromDate", Colname.from_date)
          .withColumnRenamed("ToDate", Colname.to_date))
    return filter_on_period(df, parse_period(args.beginning_date_time, args.end_date_time))


def load_charge_prices(args: Namespace, spark: SparkSession) -> DataFrame:
    df = (__load_from_sql_table(spark, args, "ChargePrice")
          .withColumnRenamed("ChargeKey", Colname.charge_key)
          .withColumnRenamed("ChargePrice", Colname.charge_price)
          .withColumnRenamed("Time", Colname.time))
    df = filter_on_date(df, parse_period(args.beginning_date_time, args.end_date_time))
    return df


def load_es_brp_relations(args: Namespace, spark: SparkSession, grid_areas: List[str]) -> DataFrame:
    columns = [Colname.energy_supplier_id, "grid_area", Colname.from_date, Colname.to_date]
    hardcoded_energy_suppliers = [("brp-id-43", "123", "2010-01-01T00:00:00Z", "2021-01-01T00:00:00Z")]
    df = spark.sparkContext.parallelize(hardcoded_energy_suppliers).toDF(columns)
    df = df.withColumn(Colname.from_date, F.to_timestamp(Colname.from_date))
    df = df.withColumn(Colname.to_date, F.to_timestamp(Colname.to_date))
    df = filter_on_period(df, parse_period(args.beginning_date_time, args.end_date_time))
    df = filter_on_grid_areas(df, Colname.grid_area, grid_areas)
    return df


def load_time_series_points(args: Namespace, spark: SparkSession, metering_point_df: DataFrame) -> DataFrame:
    df = __load_delta_data(
        spark,
        args.shared_storage_time_series_base_path,
        args.time_series_points_delta_table_name,
        time_series_points_where_date_condition(parse_period(args.beginning_date_time, args.end_date_time)))

    df = filter_on_date(df, parse_period(args.beginning_date_time, args.end_date_time))

    df = select_latest_point_data(df)

    df = filter_time_series_by_metering_points(df, metering_point_df.select(col(Colname.metering_point_id)))

    return df


def select_latest_point_data(df: DataFrame) -> DataFrame:
    df = (df.withColumn(
              "row_number",
              F.row_number()
              .over(Window
                    .partitionBy(Colname.metering_point_id, Colname.time)
                    .orderBy(F.col(Colname.registration_date_time).desc()))))
    return df.filter(F.col("row_number") == 1).drop("row_number")


def filter_time_series_by_metering_points(df: DataFrame, metering_point_df: DataFrame) -> DataFrame:
    # Solely include time series for which we have metering point master data
    return df.join(metering_point_df, df.metering_point_id == metering_point_df.metering_point_id, "leftsemi")
