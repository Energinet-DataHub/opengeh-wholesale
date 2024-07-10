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
    BooleanType,
    StructField,
    StringType,
    TimestampType,
    StructType,
)

from package.databases.table_column_names import TableColumnNames

charge_price_information_periods_schema_uc = StructType(
    [
        StructField(TableColumnNames.calculation_id, StringType(), False),
        StructField(TableColumnNames.charge_key, StringType(), False),
        StructField(TableColumnNames.charge_code, StringType(), False),
        StructField(TableColumnNames.charge_type, StringType(), False),
        StructField(TableColumnNames.charge_owner_id, StringType(), False),
        StructField(TableColumnNames.resolution, StringType(), False),
        StructField(TableColumnNames.is_tax, BooleanType(), False),
        StructField(TableColumnNames.from_date, TimestampType(), False),
        StructField(TableColumnNames.to_date, TimestampType(), False),
    ]
)

# ToDo JMG: Remove when we are on Unity Catalog
hive_charge_price_information_periods_schema = StructType(
    [
        StructField("calculation_id", StringType(), False),
        StructField("charge_key", StringType(), False),
        StructField("charge_code", StringType(), False),
        StructField("charge_type", StringType(), False),
        StructField("charge_owner_id", StringType(), False),
        StructField("resolution", StringType(), False),
        StructField("is_tax", BooleanType(), False),
        StructField("from_date", TimestampType(), False),
        StructField("to_date", TimestampType(), True),
    ]
)
