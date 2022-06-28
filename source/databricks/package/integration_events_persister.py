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

from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import StringType
from pyspark.sql.functions import year, month, dayofmonth
from package.codelists import Colname


def process_eventhub_item(df, epoch_id, integration_events_path):
    """
    Store received integration events ??? partitioned by the time of receival ???.

    ?? Time of receival is currently defined as the time the messages are enqueued
    on the EventHub. ??
    
    start up with clipping stuff out of sebastians AzureEventHub Notebook
    """
    df = (
        df.withColumn(Colname.year, year(df.enqueuedTime))
        .withColumn(Colname.month, month(df.enqueuedTime))
        .withColumn(Colname.day, dayofmonth(df.enqueuedTime))
        .withColumn(Colname.timeseries, df.body.cast(StringType()))
        .select(Colname.timeseries, Colname.year, Colname.month, Colname.day)
    )

    (df
     .write.partitionBy(Colname.year, Colname.month, Colname.day)
     .format("parquet")
     .mode("append")
     .save(integration_events_path))


def integration_events_persister(streamingDf: DataFrame, checkpoint_path: str, integration_events_path: str):
    return (
        streamingDf.writeStream.option("checkpointLocation", checkpoint_path)
        .foreachBatch(
            lambda df, epochId: process_eventhub_item(
                df, epochId, integration_events_path
            )
        )
        .start()
    )
