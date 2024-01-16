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

from package.constants import EnergyResultColumnNames

# Note: The order of the columns must match the order of the columns in the Delta table
energy_results_schema = StructType(
    [
        # The grid area in question. In case of exchange it's the to-grid area.
        StructField(EnergyResultColumnNames.grid_area, StringType(), False),
        StructField(EnergyResultColumnNames.energy_supplier_id, StringType(), True),
        StructField(EnergyResultColumnNames.balance_responsible_id, StringType(), True),
        # Energy quantity in kWh for the given observation time.
        # Null when quality is missing.
        # Example: 1234.534
        StructField(EnergyResultColumnNames.quantity, DecimalType(18, 3), True),
        StructField(
            EnergyResultColumnNames.quantity_qualities,
            ArrayType(StringType(), False),
            False,
        ),
        StructField(EnergyResultColumnNames.time, TimestampType(), False),
        StructField(EnergyResultColumnNames.aggregation_level, StringType(), False),
        StructField(EnergyResultColumnNames.time_series_type, StringType(), False),
        StructField(EnergyResultColumnNames.calculation_id, StringType(), False),
        StructField(EnergyResultColumnNames.calculation_type, StringType(), False),
        StructField(
            EnergyResultColumnNames.calculation_execution_time_start,
            TimestampType(),
            False,
        ),
        # The time when the energy was consumed/produced/exchanged
        StructField(EnergyResultColumnNames.from_grid_area, StringType(), True),
        StructField(EnergyResultColumnNames.calculation_result_id, StringType(), False),
        StructField(EnergyResultColumnNames.metering_point_id, StringType(), True),
    ]
)
