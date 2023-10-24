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

from azure.identity import ClientSecretCredential
from importlib.resources import contents

import package.infrastructure.environment_variables as env_vars
from package.infrastructure.paths import WHOLESALE_CONTAINER_NAME
from package.infrastructure.storage_account_access.data_lake_file_manager import (
    DataLakeFileManager,
)
from .committed_migrations import download_committed_migrations
import package.datamigration.constants as c


def _get_all_migrations() -> list[str]:
    migration_files = list(
        contents(f"{c.WHEEL_NAME}.{c.MIGRATION_SCRIPTS_FOLDER_PATH}")
    )
    migration_files.sort()
    return [
        file.removesuffix(".sql") for file in migration_files if file.endswith(".sql")
    ]


def _print_count(
    storage_account_name: str, storage_account_credential: ClientSecretCredential
) -> None:
    file_manager = DataLakeFileManager(
        storage_account_name,
        storage_account_credential,
        WHOLESALE_CONTAINER_NAME,
    )

    uncommitted_migrations = get_uncommitted_migrations(file_manager)

    uncommitted_migrations_count = len(uncommitted_migrations)

    # This format is fixed as it is being used by external tools
    print(f"uncommitted_migrations_count={uncommitted_migrations_count}")


def get_uncommitted_migrations(file_manager: DataLakeFileManager) -> list[str]:
    """Get list of migrations that have not yet been committed"""

    all_migrations = _get_all_migrations()
    committed_migrations = download_committed_migrations(file_manager)
    uncommitted_migrations = [
        m for m in all_migrations if m not in committed_migrations
    ]

    return uncommitted_migrations


# This method must remain parameterless because it will be called from the entry point when deployed.
def print_count() -> None:
    storage_account_name = env_vars.get_storage_account_name()
    credential = env_vars.get_storage_account_credential()
    _print_count(storage_account_name, credential)


# This method must remain parameterless because it will be called from the entry point when deployed.
def print_all_migrations_in_package() -> None:
    all_migrations = _get_all_migrations()
    for m in all_migrations:
        print(m)
