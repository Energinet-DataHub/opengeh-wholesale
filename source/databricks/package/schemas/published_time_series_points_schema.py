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
from pyspark.sql.types import (
    DecimalType,
    StructType,
    StructField,
    StringType,
    TimestampType,
    IntegerType,
    LongType,
)

published_time_series_points_schema = StructType(
    [
        StructField("GsrnNumber", StringType(), True),
        StructField("TransactionId", StringType(), True),
        StructField("Quantity", DecimalType(18, 3), True),
        StructField("Quality", LongType(), True),
        StructField("Resolution", LongType(), True),
        StructField("RegistrationDateTime", TimestampType(), True),
        StructField("storedTime", TimestampType(), False),
        StructField("time", TimestampType(), True),
        StructField("year", IntegerType(), True),
        StructField("month", IntegerType(), True),
        StructField("day", IntegerType(), True),
    ]
)
