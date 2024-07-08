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
    StructField,
    StringType,
    TimestampType,
    StructType,
)

from package.calculation.output.storage_column_names import StorageColumnNames

charge_price_points_schema = StructType(
    [
        StructField(StorageColumnNames.calculation_id, StringType(), False),
        StructField(StorageColumnNames.charge_key, StringType(), False),
        StructField(StorageColumnNames.charge_code, StringType(), False),
        StructField(StorageColumnNames.charge_type, StringType(), False),
        StructField(StorageColumnNames.charge_owner_id, StringType(), False),
        StructField(StorageColumnNames.charge_price, DecimalType(18, 6), False),
        StructField(StorageColumnNames.charge_time, TimestampType(), False),
    ]
)
