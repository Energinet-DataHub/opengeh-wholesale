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

from package.infrastructure.storage_account_access.data_lake_file_manager import (
    DataLakeFileManager,
)
from .constants import COMMITTED_MIGRATIONS_FILE_NAME


def download_committed_migrations(file_manager: DataLakeFileManager) -> list[str]:
    """Download file with migration state from datalake and return a list of already committed migrations"""

    if not file_manager.exists_file(COMMITTED_MIGRATIONS_FILE_NAME):
        return []

    csv_reader = file_manager.download_csv(COMMITTED_MIGRATIONS_FILE_NAME)
    committed_migrations = [row[0] for row in csv_reader]
    return committed_migrations


def upload_committed_migration(
    file_manager: DataLakeFileManager, migration_name: str
) -> None:
    """Upload file with migration state from datalake and return a list of already committed migrations"""

    if not file_manager.exists_file(COMMITTED_MIGRATIONS_FILE_NAME):
        file_manager.create_file(COMMITTED_MIGRATIONS_FILE_NAME)
    else:
        _validate_number_of_columns_in_csv(file_manager)

    csv_row = f"{migration_name}\r\n"
    file_manager.append_data(COMMITTED_MIGRATIONS_FILE_NAME, csv_row)


def _validate_number_of_columns_in_csv(file_manager: DataLakeFileManager) -> None:
    if file_manager.get_file_size(COMMITTED_MIGRATIONS_FILE_NAME) == 0:
        return  # we must return because we can't iterate on the reader (it would raise an exception)

    csv_reader = file_manager.download_csv(COMMITTED_MIGRATIONS_FILE_NAME)

    expected_number_of_columns = 1
    actual_number_of_columns = len(next(csv_reader))
    if actual_number_of_columns != expected_number_of_columns:
        raise Exception(
            f"Unexpected number of columns in {COMMITTED_MIGRATIONS_FILE_NAME}. Expected: {expected_number_of_columns}. Actual: {actual_number_of_columns}"
        )
