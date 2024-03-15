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

from .migration_script_args import MigrationScriptArgs
from package.infrastructure.paths import OUTPUT_DATABASE_NAME, TEST
from package.infrastructure.paths import (
    OUTPUT_FOLDER,
    INPUT_DATABASE_NAME,
)


def substitutions(migration_args: MigrationScriptArgs) -> dict[str, str]:
    return {
        "{CONTAINER_PATH}": migration_args.storage_container_path,
        "{OUTPUT_DATABASE_NAME}": OUTPUT_DATABASE_NAME,
        "{INPUT_DATABASE_NAME}": INPUT_DATABASE_NAME,
        "{OUTPUT_FOLDER}": OUTPUT_FOLDER,
        "{INPUT_FOLDER}": migration_args.calculation_input_folder,
        "{TEST}": TEST,
    }
