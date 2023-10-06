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
from pyspark.sql.functions import col
from pyspark.sql.types import (
    DecimalType,
    StructType,
    StructField,
    StringType,
    TimestampType,
)
from dataclasses import dataclass
from enum import IntEnum

from pure_protobuf.dataclasses_ import field, message
from pure_protobuf.types import int32

connectionString = "<INSERT>"
conf = {}
conf[
    "eventhubs.connectionString"
] = sc._jvm.org.apache.spark.eventhubs.EventHubsUtils.encrypt(connectionString)

streamingDF = spark.readStream.format("eventhubs").options(**conf).load()


@message
@dataclass
class ChargeLinkCreated:
    charge_link_id: str = field(1, default="")
    metering_point_id: str = field(2, default="")
    charge_code: str = field(3, default="")
    charge_type: ChargeTypeContract = field(4, default=0)
    charge_owner: str = field(5, default="")


class ChargeTypeContract(IntEnum):
    CT_UNKNOWN = 0
    CT_SUBSCRIPTION = 1
    CT_FEE = 2
    CT_TARIFF = 3


def specific_message_bytes_to_row(pb_bytes):
    obj = ChargeLinkCreated.loads(pb_bytes)
    # df = spark.createDataFrame(obj)
    # df.printSchema()
    return str(obj)


specific_message_bytes_to_row_udf = udf(specific_message_bytes_to_row, StringType())


def parse(raw_data: DataFrame) -> DataFrame:
    parsed_data = raw_data.withColumn(
        "event", specific_message_bytes_to_row_udf(col("body"))
    )
    # parsed_data.display()
    evt = parsed_data.select("event")

    # evt.show()
    # print("Parsed stream schema:")
    # raw_data.printSchema()

    return parsed_data


def foreach_batch_function(df, epoch_id):
    raw_stream = df.cache()
    # raw_stream.printSchema()
    evt = parse(raw_stream)
    evt.display()
    # return evt


streamingDF.writeStream.foreachBatch(foreach_batch_function).start().awaitTermination()


# COMMAND ----------
