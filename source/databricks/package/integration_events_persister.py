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

sys.path.append(r"/workspaces/opengeh-wholesale/source/databricks")

from pyspark.sql import DataFrame
from pyspark.sql.functions import year, month, dayofmonth, col, from_json, current_timestamp


# integration_events_persister
def integration_events_persister(
    streamingDf: DataFrame,
    integration_events_path: str,
    integration_events_checkpoint_path: str,
):

    events = (
        streamingDf
        .withColumn("storedTime", current_timestamp())
        .withColumn("day", dayofmonth(col("storedTime")))
        .withColumn("month", month(col("storedTime")))
        .withColumn("year", year(col("storedTime")))
    )

    (
        events.writeStream.partitionBy("year", "month", "day")
        .format("parquet")
        .option("checkpointLocation", integration_events_checkpoint_path)
        .start(integration_events_path)
    )
