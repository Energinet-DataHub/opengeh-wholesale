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
from pyspark.sql.types import StringType, StructType, StructField, TimestampType

eventhub_integration_events_schema = StructType(
    [
        StructField("GsrnNumber", StringType(), True),
        StructField("GridAreaCode", StringType(), True),
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
