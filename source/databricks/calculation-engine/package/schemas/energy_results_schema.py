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
)

from package.constants import EnergyResultTableColName

# Note: The order of the columns must match the order of the columns in the Delta table
energy_results_schema = StructType(
    [
        # The grid area in question. In case of exchange it's the to-grid area.
        StructField(EnergyResultTableColName.grid_area, StringType(), False),
        StructField(EnergyResultTableColName.energy_supplier_id, StringType(), True),
        StructField(EnergyResultTableColName.balance_responsible_id, StringType(), True),
        # Energy quantity in kWh for the given observation time.
        # Null when quality is missing.
        # Example: 1234.534
        StructField(EnergyResultTableColName.quantity, DecimalType(18, 3), True),
        StructField(EnergyResultTableColName.quantity_quality, StringType(), False),
        StructField(EnergyResultTableColName.time, TimestampType(), False),
        StructField(EnergyResultTableColName.aggregation_level, StringType(), False),
        StructField(EnergyResultTableColName.time_series_type, StringType(), False),
        StructField(EnergyResultTableColName.batch_id, StringType(), False),
        StructField(EnergyResultTableColName.batch_process_type, StringType(), False),
        StructField(
            EnergyResultTableColName.batch_execution_time_start, TimestampType(), False
        ),
        # The time when the energy was consumed/produced/exchanged
        StructField(EnergyResultTableColName.from_grid_area, StringType(), True),
        StructField(EnergyResultTableColName.calculation_result_id, StringType(), False),
    ]
)
