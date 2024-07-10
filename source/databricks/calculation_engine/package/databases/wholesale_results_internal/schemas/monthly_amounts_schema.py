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
    StringType,
    StructField,
    StructType,
    TimestampType,
    BooleanType,
)

from package.databases.table_column_names import TableColumnNames
from package.constants import MonthlyAmountsColumnNames

# Note: The order of the columns must match the order of the columns in the Delta table
monthly_amounts_schema_uc = StructType(
    [
        StructField(TableColumnNames.calculation_id, StringType(), False),
        StructField(TableColumnNames.result_id, StringType(), False),
        StructField(TableColumnNames.grid_area_code, StringType(), False),
        StructField(TableColumnNames.energy_supplier_id, StringType(), False),
        StructField(TableColumnNames.quantity_unit, StringType(), False),
        StructField(TableColumnNames.time, TimestampType(), False),
        StructField(TableColumnNames.amount, DecimalType(18, 6), True),
        StructField(TableColumnNames.is_tax, BooleanType(), False),
        StructField(TableColumnNames.charge_code, StringType(), False),
        StructField(TableColumnNames.charge_type, StringType(), False),
        StructField(TableColumnNames.charge_owner_id, StringType(), False),
    ]
)

hive_monthly_amounts_schema = StructType(
    [
        StructField(MonthlyAmountsColumnNames.calculation_id, StringType(), False),
        StructField(MonthlyAmountsColumnNames.calculation_type, StringType(), False),
        StructField(
            MonthlyAmountsColumnNames.calculation_execution_time_start,
            TimestampType(),
            False,
        ),
        StructField(
            MonthlyAmountsColumnNames.calculation_result_id, StringType(), False
        ),
        StructField(MonthlyAmountsColumnNames.grid_area_code, StringType(), False),
        StructField(MonthlyAmountsColumnNames.energy_supplier_id, StringType(), False),
        StructField(MonthlyAmountsColumnNames.quantity_unit, StringType(), False),
        StructField(MonthlyAmountsColumnNames.time, TimestampType(), False),
        StructField(MonthlyAmountsColumnNames.amount, DecimalType(18, 6), True),
        StructField(MonthlyAmountsColumnNames.is_tax, BooleanType(), False),
        StructField(MonthlyAmountsColumnNames.charge_code, StringType(), False),
        StructField(MonthlyAmountsColumnNames.charge_type, StringType(), False),
        StructField(MonthlyAmountsColumnNames.charge_owner_id, StringType(), False),
    ]
)
