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

import sys

from pyspark.sql import DataFrame
from pyspark.sql.functions import (
    year,
    month,
    dayofmonth,
    col,
    from_json,
    current_timestamp,
)
from package import log, debug


def _persist(
    events_df, batch_id, integration_events_checkpoint_path, integration_events_path
):
    (
        events_df.partitionBy("year", "month", "day")
        # .format("parquet")
        .option("checkpointLocation", integration_events_checkpoint_path)
        # .start(integration_events_path)
        .parquet(integration_events_path)
    )

    log("Events received", events_df)


# integration_events_persister
def integration_events_persister(
    streamingDf: DataFrame,
    integration_events_path: str,
    integration_events_checkpoint_path: str,
):
    events = (
        streamingDf.withColumn("storedTime", current_timestamp())
        .withColumn("year", year(col("storedTime")))
        .withColumn("month", month(col("storedTime")))
        .withColumn("day", dayofmonth(col("storedTime")))
    )

    events.writeStream.foreachBatch(
        lambda events_df, batch_id: _persist(
            events_df,
            batch_id,
            integration_events_checkpoint_path,
            integration_events_path,
        )
    )
