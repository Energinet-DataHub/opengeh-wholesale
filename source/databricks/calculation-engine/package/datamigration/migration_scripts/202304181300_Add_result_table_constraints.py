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

from delta import DeltaTable
import os
from pyspark.sql.types import (
    DecimalType,
    StringType,
    StructField,
    StructType,
    TimestampType,
)

from package.datamigration.migration_script_args import MigrationScriptArgs


OUTPUT_FOLDER = "calculation-output"
DATABASE_NAME = "wholesale_output"  # Also known as schema
RESULT_TABLE_NAME = "result"

RESULTS_SCHEMA = StructType(
    [
        StructField("batch_id", StringType(), False),
        StructField("batch_execution_time_start", TimestampType(), False),
        StructField("batch_process_type", StringType(), False),
        StructField("time_series_type", StringType(), False),
        # The grid area in question. In case of exchange it's the in-grid area.
        StructField("grid_area", StringType(), False),
        StructField("out_grid_area", StringType(), True),
        StructField("balance_responsible_id", StringType(), True),
        StructField("energy_supplier_id", StringType(), True),
        # The time when the energy was consumed/produced/exchanged
        StructField("time", TimestampType(), False),
        # Energy quantity in kWh for the given observation time.
        # Null when quality is missing.
        # Example: 1234.534
        StructField("quantity", DecimalType(18, 3), True),
        StructField("quantity_quality", StringType(), False),
        StructField("aggregation_level", StringType(), False),
    ]
)

CONSTRAINTS = [
    (
        "batch_process_type_chk",
        "batch_process_type in ('BalanceFixing', 'Aggregation')",
    ),
    (
        "time_series_type_chk",
        "time_series_type IN ('production', 'non_profiled_consumption', 'net_exchange_per_neighboring_ga', 'net_exchange_per_ga')",
    ),
    ("grid_area_chk", "LENGTH(grid_area) = 3"),
    ("out_grid_area_chk", "out_grid_area IS NULL OR LENGTH(out_grid_area) = 3"),
    (
        "quantity_quality_chk",
        "quantity_quality IN ('missing', 'estimated', 'measured', 'calculated', 'incomplete')",
    ),
    (
        "aggregation_level_chk",
        "aggregation_level IN ('total_ga', 'es_brp_ga', 'es_ga', 'brp_ga')",
    ),
]


def apply(args: MigrationScriptArgs) -> None:
    db_location = f"{args.storage_container_path}/{OUTPUT_FOLDER}"
    table_location = (
        f"{args.storage_container_path}/{OUTPUT_FOLDER}/{RESULT_TABLE_NAME}"
    )

    # Functionality to create the delta table was moved from production code.
    # That's the reason why this guard is required.
    if os.path.exists(table_location) and DeltaTable.isDeltaTable(
        args.spark, table_location
    ):
        return

    args.spark.sql(
        f"CREATE DATABASE {DATABASE_NAME} \
        COMMENT 'Contains result data from wholesale domain.' \
        LOCATION '{db_location}'"
    )

    (
        DeltaTable.create(args.spark)
        .tableName(f"{DATABASE_NAME}.{RESULT_TABLE_NAME}")
        .location(table_location)
        .addColumns(RESULTS_SCHEMA)
        .execute()
    )

    for constraint in CONSTRAINTS:
        args.spark.sql(
            f"ALTER TABLE {DATABASE_NAME}.{RESULT_TABLE_NAME} ADD CONSTRAINT {constraint[0]} CHECK ({constraint[1]})"
        )
