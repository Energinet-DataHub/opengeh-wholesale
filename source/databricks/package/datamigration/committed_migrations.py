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

from .data_lake_file_manager import DataLakeFileManager

COMMITTED_MIGRATIONS_FILE_NAME = "migration_state.csv"


def download_committed_migrations(file_manager: DataLakeFileManager) -> list[str]:
    """Download file with migration state from datalake and return a list of already committed migrations"""

    _create_migration_file_if_not_existing(file_manager)

    csv_reader = file_manager.download_csv(COMMITTED_MIGRATIONS_FILE_NAME)
    committed_migrations = [row[0] for row in csv_reader]
    return committed_migrations


def upload_committed_migration(file_manager: DataLakeFileManager):
    """Upload file with migration state from datalake and return a list of already committed migrations"""

    _create_migration_file_if_not_existing(file_manager)

    print("upload_committed_migration is not implemented yet")


def _create_migration_file_if_not_existing(file_manager: DataLakeFileManager):

    if not file_manager.file_exists(COMMITTED_MIGRATIONS_FILE_NAME):
        file_manager.create_file(COMMITTED_MIGRATIONS_FILE_NAME)
