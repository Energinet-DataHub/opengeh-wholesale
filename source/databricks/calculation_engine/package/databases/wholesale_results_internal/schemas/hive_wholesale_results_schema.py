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
    ArrayType,
    BooleanType,
    DecimalType,
    StringType,
    StructField,
    StructType,
    TimestampType,
)

from package.databases.table_column_names import TableColumnNames

# TODO AJW: This schema is not used in the codebase. It should be removed.
# Note: The order of the columns must match the order of the columns in the Delta table
hive_wholesale_results_schema = StructType(
    [
        StructField(TableColumnNames.calculation_id, StringType(), False),
        StructField(TableColumnNames.calculation_type, StringType(), False),
        StructField(
            TableColumnNames.calculation_execution_time_start,
            TimestampType(),
            False,
        ),
        StructField(TableColumnNames.calculation_result_id, StringType(), False),
        StructField(TableColumnNames.grid_area_code, StringType(), False),
        # Wholesale results are per energy supplier therefore energy_supplier_id cannot be null.
        StructField(TableColumnNames.energy_supplier_id, StringType(), False),
        StructField(TableColumnNames.quantity, DecimalType(18, 3), True),
        StructField(TableColumnNames.quantity_unit, StringType(), False),
        StructField(
            TableColumnNames.quantity_qualities,
            ArrayType(StringType()),
            True,
        ),
        StructField(TableColumnNames.time, TimestampType(), False),
        StructField(TableColumnNames.resolution, StringType(), False),
        StructField(TableColumnNames.metering_point_type, StringType(), True),
        StructField(TableColumnNames.settlement_method, StringType(), True),
        StructField(TableColumnNames.price, DecimalType(18, 6), True),
        StructField(TableColumnNames.amount, DecimalType(18, 6), True),
        StructField(TableColumnNames.is_tax, BooleanType(), True),
        StructField(TableColumnNames.charge_code, StringType(), True),
        StructField(TableColumnNames.charge_type, StringType(), True),
        StructField(TableColumnNames.charge_owner_id, StringType(), True),
        StructField(TableColumnNames.amount_type, StringType(), False),
    ]
)
