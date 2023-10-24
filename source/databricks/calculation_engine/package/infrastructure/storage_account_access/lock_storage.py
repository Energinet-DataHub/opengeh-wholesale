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
from package.infrastructure.storage_account_access import (
    DataLakeFileManager,
)
import package.infrastructure.environment_variables as env_vars
from package.infrastructure import log
from package.infrastructure.paths import WHOLESALE_CONTAINER_NAME

_LOCK_FILE_NAME = "DATALAKE_IS_LOCKED"


def _lock(
    storage_account_name: str, storage_account_credential: ClientSecretCredential
) -> None:
    file_manager = DataLakeFileManager(
        storage_account_name, storage_account_credential, WHOLESALE_CONTAINER_NAME
    )
    file_manager.create_file(_LOCK_FILE_NAME)
    log(f"created lock file: {_LOCK_FILE_NAME}")


def _unlock(
    storage_account_name: str, storage_account_credential: ClientSecretCredential
) -> None:
    file_manager = DataLakeFileManager(
        storage_account_name, storage_account_credential, WHOLESALE_CONTAINER_NAME
    )
    file_manager.delete_file(_LOCK_FILE_NAME)
    log(f"deleted lock file: {_LOCK_FILE_NAME}")


def islocked(
    storage_account_name: str, storage_account_credential: ClientSecretCredential
) -> bool:
    file_manager = DataLakeFileManager(
        storage_account_name, storage_account_credential, WHOLESALE_CONTAINER_NAME
    )
    return file_manager.exists_file(_LOCK_FILE_NAME)


# This method must remain parameterless because it will be called from the entry point when deployed.
def lock() -> None:
    storage_account_name = env_vars.get_storage_account_name()
    credential = env_vars.get_storage_account_credential()
    _lock(storage_account_name, credential)


# This method must remain parameterless because it will be called from the entry point when deployed.
def unlock() -> None:
    storage_account_name = env_vars.get_storage_account_name()
    credential = env_vars.get_storage_account_credential()
    _unlock(storage_account_name, credential)
