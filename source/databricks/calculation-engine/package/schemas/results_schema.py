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


class ResultSchemaField:
    "IMPORTANT: Any semantic change to these field names most likely requires a corresponding data migration of the results Delta table."
    batch_id = "batch_id"
    batch_execution_time_start = "batch_execution_time_start"
    batch_process_type = "batch_process_type"
    time_series_type = "time_series_type"
    grid_area = "grid_area"
    out_grid_area = "out_grid_area"
    balance_responsible_id = "balance_responsible_id"
    energy_supplier_id = "energy_supplier_id"
    time = "time"
    quantity = "quantity"
    quantity_quality = "quantity_quality"
    aggregation_level = "aggregation_level"


results_schema = StructType(
    [
        StructField(ResultSchemaField.batch_id, StringType(), False),
        StructField(
            ResultSchemaField.batch_execution_time_start, TimestampType(), False
        ),
        StructField(ResultSchemaField.batch_process_type, StringType(), False),
        StructField(ResultSchemaField.time_series_type, StringType(), False),
        # The grid area in question. In case of exchange it's the in-grid area.
        StructField(ResultSchemaField.grid_area, StringType(), False),
        StructField(ResultSchemaField.out_grid_area, StringType(), True),
        StructField(ResultSchemaField.balance_responsible_id, StringType(), True),
        StructField(ResultSchemaField.energy_supplier_id, StringType(), True),
        # The time when the energy was consumed/produced/exchanged
        StructField(ResultSchemaField.time, TimestampType(), False),
        # Energy quantity in kWh for the given observation time.
        # Null when quality is missing.
        # Example: 1234.534
        StructField(ResultSchemaField.quantity, DecimalType(18, 3), True),
        # "missing" | "estimated" | "measured" | "calculated" | "incomplete"
        # Example: measured
        StructField(ResultSchemaField.quantity_quality, StringType(), False),
        StructField(ResultSchemaField.aggregation_level, StringType(), False),
    ]
)
"""Schema for calculation results created by the calculator job.
IMPORTANT: Any semantic change to this schema most likely requires a corresponding data migration of the results Delta table."""

constraints = [
    (
        f"{ResultSchemaField.batch_process_type}_chk",
        f"{ResultSchemaField.batch_process_type} in ('BalanceFixing', 'Aggregation')",
    ),
    (
        f"{ResultSchemaField.time_series_type}_chk",
        f"""{ResultSchemaField.time_series_type}
                IN ('production', 'non_profiled_consumption', 'net_exchange_per_neighboring_ga', 'net_exchange_per_ga')""",
    ),
    (
        f"{ResultSchemaField.grid_area}_chk",
        f"LENGTH({ResultSchemaField.grid_area}) = 3",
    ),
    (
        f"{ResultSchemaField.out_grid_area}_chk",
        f"{ResultSchemaField.out_grid_area} IS NULL OR LENGTH({ResultSchemaField.out_grid_area}) = 3",
    ),
    (
        f"{ResultSchemaField.quantity_quality}_chk",
        f"{ResultSchemaField.quantity_quality} IN ('missing', 'estimated', 'measured', 'calculated', 'incomplete')",
    ),
    (
        f"{ResultSchemaField.aggregation_level}_chk",
        f"{ResultSchemaField.aggregation_level} IN ('total_ga', 'es_brp_ga', 'es_ga', 'brp_ga')",
    ),
]
