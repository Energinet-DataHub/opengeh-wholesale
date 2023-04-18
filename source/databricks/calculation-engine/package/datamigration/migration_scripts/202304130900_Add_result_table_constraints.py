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
from package.datamigration.script_tools import DeltaTableMigrator


RESULT_TABLE_NAME = "result"
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
    migrator = DeltaTableMigrator(
        args.spark,
        RESULT_TABLE_NAME,
    )
    statements = [
        f"ALTER TABLE {{table_name}} ADD CONSTRAINT {constraint[0]}_chk CHECK ({constraint[1]})"
        for constraint in CONSTRAINTS
    ]
    migrator.apply_sql(statements)
