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

from package.constants import ResultTableColName

results_schema = StructType(
    [
        StructField(ResultTableColName.batch_id, StringType(), False),
        StructField(
            ResultTableColName.batch_execution_time_start, TimestampType(), False
        ),
        StructField(ResultTableColName.batch_process_type, StringType(), False),
        StructField(ResultTableColName.time_series_type, StringType(), False),
        # The grid area in question. In case of exchange it's the in-grid area.
        StructField(ResultTableColName.grid_area, StringType(), False),
        StructField(ResultTableColName.out_grid_area, StringType(), True),
        StructField(ResultTableColName.balance_responsible_id, StringType(), True),
        StructField(ResultTableColName.energy_supplier_id, StringType(), True),
        # The time when the energy was consumed/produced/exchanged
        StructField(ResultTableColName.time, TimestampType(), False),
        # Energy quantity in kWh for the given observation time.
        # Null when quality is missing.
        # Example: 1234.534
        StructField(ResultTableColName.quantity, DecimalType(18, 3), True),
        StructField(ResultTableColName.quantity_quality, StringType(), False),
        StructField(ResultTableColName.aggregation_level, StringType(), False),
    ]
)
