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
    DecimalType,
    StringType,
    StructField,
    StructType,
    TimestampType,
)

from package.databases.table_column_names import TableColumnNames

# Note: The order of the columns must match the order of the columns in the Delta table
hive_energy_results_schema = StructType(
    [
        # The grid area in question. In case of exchange it's the to-grid area.
        StructField(TableColumnNames.grid_area_code, StringType(), False),
        StructField(TableColumnNames.energy_supplier_id, StringType(), True),
        StructField(TableColumnNames.balance_responsible_id, StringType(), True),
        # Energy quantity in kWh for the given observation time.
        # Null when quality is missing.
        # Example: 1234.534
        StructField(TableColumnNames.quantity, DecimalType(18, 3), False),
        StructField(
            TableColumnNames.quantity_qualities,
            ArrayType(StringType()),
            False,
        ),
        StructField(TableColumnNames.time, TimestampType(), False),
        StructField(TableColumnNames.aggregation_level, StringType(), False),
        StructField(TableColumnNames.time_series_type, StringType(), False),
        StructField(TableColumnNames.calculation_id, StringType(), False),
        StructField(TableColumnNames.calculation_type, StringType(), False),
        StructField(
            TableColumnNames.calculation_execution_time_start,
            TimestampType(),
            False,
        ),
        # The time when the energy was consumed/produced/exchanged
        StructField(TableColumnNames.neighbor_grid_area_code, StringType(), True),
        StructField(TableColumnNames.calculation_result_id, StringType(), False),
        StructField(TableColumnNames.metering_point_id, StringType(), True),
        StructField(TableColumnNames.resolution, StringType(), True),
    ]
)
