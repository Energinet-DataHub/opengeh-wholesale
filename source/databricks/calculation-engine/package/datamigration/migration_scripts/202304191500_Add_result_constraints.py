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

from package.datamigration.migration_script_args import MigrationScriptArgs

OUTPUT_FOLDER = "calculation-output"
DATABASE_NAME = "wholesale_output"  # Also known as schema
RESULT_TABLE_NAME = "result"

CONSTRAINTS = [
    (
        "batch_process_type_chk",
        "batch_process_type IN ('BalanceFixing', 'Aggregation')",
    ),
    (
        "time_series_type_chk",
        """time_series_type IN (
            'production',
            'non_profiled_consumption',
            'net_exchange_per_neighboring_ga',
            'net_exchange_per_ga',
            'flex_consumption',
            'grid_loss',
            'negative_grid_loss',
            'positive_grid_loss')""",
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
    existing_constraints = args.spark.sql(
        f"SHOW CONSTRAINTS ON {DATABASE_NAME}.{RESULT_TABLE_NAME}"
    ).collect()

    for constraint in CONSTRAINTS:
        constraint_name = constraint[0]
        constraint_condition = constraint[1]

        # Check if the constraint already exists
        existing_constraint = next(
            (c for c in existing_constraints if c["name"] == constraint_name),
            None
        )

        if existing_constraint:
            # Drop the existing constraint
            args.spark.sql(
                f"ALTER TABLE {DATABASE_NAME}.{RESULT_TABLE_NAME} DROP CONSTRAINT {constraint_name}"
            )
            print(f"Dropped constraint '{constraint_name}'")

        # Add the constraint
        args.spark.sql(
            f"ALTER TABLE {DATABASE_NAME}.{RESULT_TABLE_NAME} ADD CONSTRAINT {constraint_name} CHECK ({constraint_condition})"
        )
        print(f"Added constraint '{constraint_name}'")
