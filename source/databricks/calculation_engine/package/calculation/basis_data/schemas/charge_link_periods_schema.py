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
    IntegerType,
    StructField,
    StringType,
    TimestampType,
    StructType,
)

hive_charge_link_periods_schema = StructType(
    [
        StructField("calculation_id", StringType(), False),
        StructField("charge_key", StringType(), False),
        StructField("charge_code", StringType(), False),
        StructField("charge_type", StringType(), False),
        StructField("charge_owner_id", StringType(), False),
        StructField("metering_point_id", StringType(), False),
        StructField("quantity", IntegerType(), False),
        StructField("from_date", TimestampType(), False),
        StructField("to_date", TimestampType(), True),
    ]
)
