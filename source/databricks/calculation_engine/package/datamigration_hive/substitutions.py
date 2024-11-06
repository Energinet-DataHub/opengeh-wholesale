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

import package.infrastructure.paths as paths
from .migration_script_args import MigrationScriptArgs


def substitutions(migration_args: MigrationScriptArgs) -> dict[str, str]:
    return {
        "{CONTAINER_PATH}": migration_args.storage_container_path,
        "{HIVE_OUTPUT_DATABASE_NAME}": paths.HiveOutputDatabase.DATABASE_NAME,
        "{INPUT_DATABASE_NAME}": paths.InputDatabase.DATABASE_NAME,
        "{OUTPUT_FOLDER}": paths.HiveOutputDatabase.FOLDER_NAME,
        "{INPUT_FOLDER}": migration_args.calculation_input_folder,
        "{BASIS_DATA_FOLDER}": paths.HiveBasisDataDatabase.FOLDER_NAME,
        "{HIVE_BASIS_DATA_DATABASE_NAME}": paths.HiveBasisDataDatabase.DATABASE_NAME,
        "{CALCULATION_RESULTS_DATABASE_NAME}": paths.CalculationResultsPublicDataModel.DATABASE_NAME,
        "{HIVE_SETTLEMENT_REPORT_DATABASE_NAME}": paths.HiveSettlementReportPublicDataModel.DATABASE_NAME,
    }
