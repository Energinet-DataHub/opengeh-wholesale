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
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, from_json, explode
from pyspark.sql.types import (
    DecimalType,
    StructType,
    StructField,
    StringType,
    TimestampType,
    ArrayType,
    BinaryType,
    IntegerType,
)
import json
import datetime
from delta.tables import *

connectionString = "<INSERT>"
conf = {}
conf[
    "eventhubs.connectionString"
] = sc._jvm.org.apache.spark.eventhubs.EventHubsUtils.encrypt(connectionString)

storage_account_name = (
    "timeseriesdatajouless"  # this must be changed to your storage account name
)
storage_account_key = "<INSERT>"
container_name = "masterdata"
spark.conf.set(
    "fs.azure.account.key.{0}.dfs.core.windows.net".format(storage_account_name),
    storage_account_key,
)

delta_path = (
    "abfss://"
    + container_name
    + "@"
    + storage_account_name
    + ".dfs.core.windows.net/delta/metering-point/"
)

schemas = {
    "MeteringPointPeriod": ArrayType(
        StructType(
            [
                StructField("metering_point_id", StringType(), False),
                StructField("valid_from", TimestampType(), False),
                StructField("valid_to", TimestampType(), False),
            ]
        )
    )
}


metering_point_schema = StructType(
    [
        StructField("metering_point_id", StringType(), False),
        StructField("valid_from", TimestampType(), False),
        StructField("valid_to", TimestampType(), False),
    ]
)

charge_schema = StructType(
    [
        StructField("charge_code", StringType(), False),
        StructField("charge", IntegerType(), True),
    ]
)


streamingDF = spark.readStream.format("eventhubs").options(**conf).load()
# test manuel write of different type of event
alt = spark.createDataFrame([{"charge_code": "1234", "charge": 1}], charge_schema)
alt.write.format("delta").mode("append").save(
    "abfss://"
    + container_name
    + "@"
    + storage_account_name
    + ".dfs.core.windows.net/delta/charge/"
)


def foreach_batch_function(df, epoch_id):
    if len(df.head(1)) > 0:
        jsonData = df.select(
            df.body.cast(StringType()).alias("body"),
            (df.properties["SchemaType"]).alias("type"),
        )

        schema = schemas[jsonData.first().type]
        # Parse event body from bytes to schema corresponding to json input and add to new column
        df = jsonData.withColumn("periods", from_json(col("body"), schema))
        df.show(20, False)

        # Get metering point periods from new column as rows
        period_rows = df.select("periods").collect()[0][0]

        # Create dataframe from metering point rows
        period_df = spark.createDataFrame(period_rows, schema=metering_point_schema)

        # Select metering point id from first row
        metering_point_id = period_df.select("metering_point_id").collect()[0][0]

        # Delete entries in delta table based on metering point id from event data
        deltaTable = DeltaTable.forPath(spark, delta_path)
        deltaTable.delete(col("metering_point_id") == metering_point_id)

        # Append new metering point periods
        period_df.write.partitionBy("metering_point_id").format("delta").mode(
            "append"
        ).save(delta_path)


streamingDF.writeStream.foreachBatch(foreach_batch_function).start().awaitTermination()


# COMMAND ----------

from pyspark.sql import DataFrame
from pyspark.sql.functions import col, from_json, to_json
from pyspark.sql.types import (
    DecimalType,
    StructType,
    StructField,
    StringType,
    TimestampType,
    ArrayType,
    BinaryType,
)

my_data = [
    '[{"metering_point_id": "1","valid_from": "2021-01-01T00:00:00Z","valid_to": "9999-01-01T00:00:00Z"}]'
]
rdd = sc.parallelize(my_data)
rdd = rdd.map(lambda x: [x])  # transform the rdd

schema = StructType([StructField("body", StringType(), False)])

binary_schema = ArrayType(
    StructType(
        [
            StructField("metering_point_id", StringType(), False),
            StructField("valid_from", TimestampType(), False),
            StructField("valid_to", TimestampType(), False),
        ]
    )
)

my_df = spark.createDataFrame(rdd, schema=schema)
my_df = my_df.withColumn("binary", col("body").cast(BinaryType()))
my_df = my_df.withColumn(
    "from_binary", from_json(col("binary").cast(StringType()), binary_schema)
)
my_df.display()

# COMMAND ----------

read_df = spark.read.format("delta").load(delta_path)
read_df.display()

# COMMAND ----------
