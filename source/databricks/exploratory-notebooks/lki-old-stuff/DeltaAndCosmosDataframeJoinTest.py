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
from pyspark import SparkConf
from pyspark.sql import SparkSession

def initialize_spark(args):
    # Set spark config with storage account names/keys and the session timezone so that datetimes are displayed consistently (in UTC)
    spark_conf = SparkConf(loadDefaults=True) \
        .set('fs.azure.account.key.{0}.dfs.core.windows.net'.format(args.data_storage_account_name), args.data_storage_account_key) \
        .set("spark.sql.session.timeZone", "UTC") \
        .set("spark.databricks.io.cache.enabled", "True")

    return SparkSession \
        .builder\
        .config(conf=spark_conf)\
        .getOrCreate()

# COMMAND ----------

p = configargparse.ArgParser(description='Green Energy Hub Tempory aggregation triggger', formatter_class=configargparse.ArgumentDefaultsHelpFormatter)
p.add('--data-storage-account-name', type=str, required=True,
      help='Azure Storage account name holding time series data')
p.add('--data-storage-account-key', type=str, required=True,
      help='Azure Storage key for storage', env_var='GEH_INPUT_STORAGE_KEY')
p.add('--data-storage-container-name', type=str, required=False, default='data',
      help='Azure Storage container name for input storage')
p.add('--time-series-path', type=str, required=False, default="delta/time_series_test_data/",
      help='Path to time series data storage location (deltalake) relative to root container')
p.add('--beginning-date-time', type=str, required=True,
      help='The timezone aware date-time representing the beginning of the time period of aggregation (ex: 2020-01-03T00:00:00Z %Y-%m-%dT%H:%M:%S%z)')
p.add('--end-date-time', type=str, required=True,
      help='The timezone aware date-time representing the end of the time period of aggregation (ex: 2020-01-03T00:00:00Z %Y-%m-%dT%H:%M:%S%z)')
p.add('--telemetry-instrumentation-key', type=str, required=True,
      help='Instrumentation key used for telemetry')
p.add('--grid-area', type=str, required=False,
      help='Run aggregation for specific grid areas format is { "areas": ["123","234"]}. If none is specifed. All grid areas are calculated')
p.add('--calcultion-type', type=str, required=True,
      help='D03 (Aggregation) or D04 (Balance fixing) '),
p.add('--result-url', type=str, required=True, help="The target url to post result json"),
p.add('--result-id', type=str, required=True, help="Postback id that will be added to header"),
p.add('--persist-source-dataframe', type=bool, required=False, default=False)
p.add('--persist-source-dataframe-location', type=str, required=False, default="delta/basis-data/")
p.add('--snapshot-url', type=str, required=True, help="The target url to post result json")
p.add('--cosmos-account-endpoint', type=str, required=True, help="Cosmos account endpoint")
p.add('--cosmos-account-key', type=str, required=True, help="Cosmos account key")
p.add('--cosmos-database', type=str, required=True, help="Cosmos database name")
p.add('--cosmos-container-metering-points', type=str, required=True, help="Cosmos container for metering points input data")
p.add('--cosmos-container-market-roles', type=str, required=True, help="Cosmos container for market roles input data")
p.add('--cosmos-container-grid-loss-sys-corr', type=str, required=True, help="Cosmos container for grid loss and system correction")
p.add('--cosmos-container-es-brp-relations', type=str, required=True, help="Cosmos container for relations between energy supplier and balance responsible")
p.add('--resolution', type=str, required=True, help="Time window resolution eg. 1 hour, 15 minutes etc.")
args, unknown_args = p.parse_known_args()

areas = []

if args.grid_area:
    areasParsed = json.loads(args.grid_area)
    areas = areasParsed["areas"]
if unknown_args:
    print("Unknown args: {0}".format(args))

spark = initialize_spark(args)

# COMMAND ----------

from pyspark.sql.types import StringType, TimestampType, StructType, StructField, IntegerType,DecimalType
from pyspark.sql.functions import col, date_format
from pyspark.sql import SparkSession

# COMMAND ----------

def load_aggregation_data(cosmos_container_name, schema, args, spark):
    config = {
        "spark.cosmos.accountEndpoint": args.cosmos_account_endpoint,
        "spark.cosmos.accountKey": args.cosmos_account_key,
        "spark.cosmos.database": args.cosmos_database,
        "spark.cosmos.container": cosmos_container_name,
        "spark.cosmos.read.inferSchema.forceNullableProperties": False
    }
    return spark.read.schema(schema).format("cosmos.oltp").options(**config).load()

# COMMAND ----------

metering_point_schema = StructType([
      StructField("metering_point_id", StringType(), False),
      StructField("metering_point_type", StringType(), False),
      StructField("settlement_method", StringType()),
      StructField("grid_area", StringType(), False),
      StructField("connection_state", StringType(), False),
      StructField("resolution", StringType(), False),
      StructField("in_grid_area", StringType()),
      StructField("out_grid_area", StringType()),
      StructField("metering_method", StringType(), False),
      StructField("net_settlement_group", StringType()),
      StructField("parent_metering_point_id", StringType()),
      StructField("unit", StringType(), False),
      StructField("product", StringType()),
      StructField("from_date", TimestampType(), False),
      StructField("to_date", TimestampType(), False)
])

def load_metering_points(args, spark):
    return load_aggregation_data(args.cosmos_container_metering_points, metering_point_schema, args, spark)

# COMMAND ----------

from pyspark.sql import DataFrame
from datetime import datetime


def filter_time_period(df: DataFrame, from_time: datetime, to_time: datetime):
    return df \
        .filter(col('time') >= from_time) \
        .filter(col('time') <= to_time)

# COMMAND ----------

import dateutil.parser

def load_time_series(args, areas, spark):
    beginning_date_time = dateutil.parser.parse(args.beginning_date_time)
    end_date_time = dateutil.parser.parse(args.end_date_time)

    TIME_SERIES_STORAGE_PATH = "abfss://{0}@{1}.dfs.core.windows.net/{2}".format(
        args.data_storage_container_name, args.data_storage_account_name, args.time_series_path
    )

    # Create input and output storage paths
    TIME_SERIES_STORAGE_PATH = "abfss://{0}@{1}.dfs.core.windows.net/{2}".format(
        args.data_storage_container_name, args.data_storage_account_name, args.time_series_path
    )

    print("Time series storage url:", TIME_SERIES_STORAGE_PATH)

    beginning_condition = f"Year >= {beginning_date_time.year} AND Month >= {beginning_date_time.month} AND Day >= {beginning_date_time.day}"
    end_condition = f"Year <= {end_date_time.year} AND Month <= {end_date_time.month} AND Day <= {end_date_time.day}"

    # Read in time series data (delta doesn't support user specified schema)
    timeseries_df = spark \
        .read \
        .format("delta") \
        .load(TIME_SERIES_STORAGE_PATH) \
        .where(f"{beginning_condition} AND {end_condition}")
    # Filter out time series data that is not in the specified time period
    valid_time_period_df = filter_time_period(timeseries_df, beginning_date_time, end_date_time)
    # Filter out time series data that do not belong to the specified grid areas
    if areas:
        valid_time_period_df = valid_time_period_df \
            .filter(col("grid_area").isin(areas))

    return valid_time_period_df

# COMMAND ----------

def get_time_series_dataframe(args, areas, spark):
    time_series_df = load_time_series(args, areas, spark)
    metering_point_df = load_metering_points(args, spark)

    print("time_series_df = " + str(time_series_df.count()))

    metering_point_join_conditions = \
        [
            time_series_df.metering_point_id == metering_point_df.metering_point_id,
            time_series_df.time >= metering_point_df.from_date,
            time_series_df.time < metering_point_df.to_date
        ]

    time_series_with_metering_point = time_series_df \
        .join(metering_point_df, metering_point_join_conditions) \
        .drop(metering_point_df.metering_point_id) \
        .drop(metering_point_df.from_date) \
        .drop(metering_point_df.to_date)

    return time_series_with_metering_point

# COMMAND ----------

the_df = get_time_series_dataframe(args, areas, spark)
the_df.display()
