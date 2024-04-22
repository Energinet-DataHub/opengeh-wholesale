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

COMMITTED_MIGRATIONS_FILE_NAME = "migration_state.csv"
WHEEL_NAME = "package"
MIGRATION_SCRIPTS_FOLDER_PATH = "package.datamigration.migration_scripts"
CURRENT_STATE_SCHEMAS_FOLDER_PATH = (
    "package.datamigration.current_state_scripts.schemas"
)
CURRENT_STATE_TABLES_FOLDER_PATH = "package.datamigration.current_state_scripts.tables"
CURRENT_STATE_VIEWS_FOLDER_PATH = "package.datamigration.current_state_scripts.views"
