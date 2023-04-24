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
        # The grid area in question. In case of exchange it's the in-grid area.
        StructField("grid_area", StringType(), False),
        StructField("energy_supplier_id", StringType(), True),
        StructField("balance_responsible_id", StringType(), True),
        # Energy quantity in kWh for the given observation time.
        # Null when quality is missing.
        # Example: 1234.534
        StructField("quantity", DecimalType(18, 3), True),
        StructField("quantity_quality", StringType(), False),
        # The time when the energy was consumed/produced/exchanged
        StructField("time", TimestampType(), False),
        StructField("aggregation_level", StringType(), False),
        StructField("time_series_type", StringType(), False),
        StructField("batch_id", StringType(), False),
        StructField("batch_process_type", StringType(), False),
        StructField("batch_execution_time_start", TimestampType(), False),
        StructField("out_grid_area", StringType(), True),
    ]
)


def apply(args: MigrationScriptArgs) -> None:
    db_location = f"{args.storage_container_path}/{OUTPUT_FOLDER}"
    table_location = (
        f"{args.storage_container_path}/{OUTPUT_FOLDER}/{RESULT_TABLE_NAME}"
    )

    # Functionality to create the delta table was moved from production code.
    # That's the reason why this guard is required.
    if DeltaTable.isDeltaTable(args.spark, table_location):
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
