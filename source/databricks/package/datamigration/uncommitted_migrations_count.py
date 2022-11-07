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
import configargparse
from os.path import isfile, join
from .data_lake_file_manager import DataLakeFileManager

MIGRATION_STATE_FILE_NAME = "migration_state.csv"


def _get_valid_args_or_throw(command_line_args: list[str]):
    p = configargparse.ArgParser(
        description="Returns number of uncommitted data migrations",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    p.add("--data-storage-account-name", type=str, required=True)
    p.add("--data-storage-account-key", type=str, required=True)
    p.add("--wholesale-container-name", type=str, required=True)

    known_args, unknown_args = p.parse_known_args(args=command_line_args)
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return known_args


def _download_committed_migrations(file_manager: DataLakeFileManager) -> list[str]:
    """Download file with migration state from datalake and return a list of aldready committed migrations"""

    csv_reader = file_manager.download_csv(
        MIGRATION_STATE_FILE_NAME,
    )
    committed_migrations = [row[0] for row in csv_reader]

    return committed_migrations


def _get_all_migrations() -> list[str]:
    return []


def _get_uncommitted_migrations_count(file_manager: DataLakeFileManager) -> int:
    """Get the number of migrations that have not yet been committed"""

    committed_migrations = _download_committed_migrations(file_manager)

    all_migrations = _get_all_migrations()

    uncommitted_migrations = [
        m for m in all_migrations if m not in committed_migrations
    ]

    return len(uncommitted_migrations)


def _start(command_line_args: list[str]):
    args = _get_valid_args_or_throw(command_line_args)

    file_manager = DataLakeFileManager(
        args.data_storage_account_name,
        args.data_storage_account_key,
        args.wholesale_container_name,
    )
    uncommitted_migrations_count = _get_uncommitted_migrations_count(file_manager)

    # This format is fixed as it is being used by external tools
    print(f"uncommitted_migrations_count={uncommitted_migrations_count}")


# This method must remain parameterless because it will be called from the entry point when deployed.
def start():
    _start(sys.argv[1:])
