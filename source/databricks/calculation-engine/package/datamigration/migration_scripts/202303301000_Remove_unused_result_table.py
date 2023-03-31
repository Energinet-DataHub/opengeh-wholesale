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
from delta import DeltaTable


def apply(args: MigrationScriptArgs) -> None:
    result_table_path = f"abfss://wholesale@{args.storage_account_name}.dfs.core.windows.net/calculation-output/result_table"
    if DeltaTable.isDeltaTable(args.spark, result_table_path):
        # drop the Delta table
        delta_table = DeltaTable.forPath(args.spark, result_table_path)

        # Delete the Delta table
        delta_table.delete()

        print(f"Delta table '{result_table_path}' deleted successfully.")
    else:
        print(f"Delta table '{result_table_path}' does not exist.")
