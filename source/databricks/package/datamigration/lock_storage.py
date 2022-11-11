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
import sys
from .data_lake_file_manager import DataLakeFileManager
import configargparse
from package import log

_LOCK_FILE_NAME = "DATALAKE_IS_LOCKED"


def _get_valid_args_or_throw(command_line_args: list[str]):
    p = configargparse.ArgParser(
        description="Locks/unlocks the storage of the specified container",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    p.add("--data-storage-account-name", type=str, required=True)
    p.add("--data-storage-account-key", type=str, required=True)
    p.add("--wholesale-container-name", type=str, required=True)

    args, unknown_args = p.parse_known_args(command_line_args)
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return args


def _lock(args: list[str]) -> None:

    file_manager = DataLakeFileManager(
        args.data_storage_account_name,
        args.data_storage_account_key,
        args.wholesale_container_name,
    )

    file_manager.create_file(_LOCK_FILE_NAME)
    log(f"created lock file: {_LOCK_FILE_NAME}")


def _unlock(args: list[str]) -> None:
    file_manager = DataLakeFileManager(
        args.data_storage_account_name,
        args.data_storage_account_key,
        args.wholesale_container_name,
    )
    file_manager.delete_file(_LOCK_FILE_NAME)
    log(f"deleted lock file: {_LOCK_FILE_NAME}")


def _islocked(args: list[str]) -> bool:
    file_manager = DataLakeFileManager(
        args.data_storage_account_name,
        args.data_storage_account_key,
        args.wholesale_container_name,
    )
    return file_manager.exists_file(_LOCK_FILE_NAME)


def lock():
    args = _get_valid_args_or_throw(sys.argv[1:])
    _lock(args)


def unlock():
    args = _get_valid_args_or_throw(sys.argv[1:])
    _unlock(args)


def islocked() -> bool:
    args = _get_valid_args_or_throw(sys.argv[1:])
    return _islocked(args)
