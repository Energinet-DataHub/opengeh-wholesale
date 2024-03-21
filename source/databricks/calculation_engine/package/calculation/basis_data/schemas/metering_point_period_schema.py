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
    StructField,
    StringType,
    TimestampType,
    StructType,
)

metering_point_period_schema = StructType(
    [
        StructField("calculation_id", StringType(), False),
        StructField("metering_point_id", StringType(), False),
        StructField("metering_point_type", StringType(), False),
        StructField("settlement_method", StringType(), True),
        StructField("grid_area_code", StringType(), False),
        StructField("resolution", StringType(), False),
        StructField("from_grid_area_code", StringType(), True),
        StructField("to_grid_area_code", StringType(), True),
        StructField("parent_metering_point_id", StringType(), True),
        StructField("energy_supplier_id", StringType(), True),
        StructField("balance_responsible_id", StringType(), True),
        StructField("from_date", TimestampType(), False),
        StructField("to_date", TimestampType(), True),
    ]
)
