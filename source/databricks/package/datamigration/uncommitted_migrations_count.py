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

import csv
import sys
import configargparse
from os import path
from .committed_migrations import download_committed_migrations

MIGRATION_SCRIPTS_FOLDER_NAME = "migration_scripts"
ALL_MIGRATIONS_FILE_NAME = "migrations.csv"


def _get_migration_scripts_path():
    path.join(path.dirname(__file__), MIGRATION_SCRIPTS_FOLDER_NAME)


def _get_all_migrations_file_name():
    return path.join(_get_migration_scripts_path, ALL_MIGRATIONS_FILE_NAME)


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


def _get_all_migrations() -> list[str]:
    """Get names of all the migrations. i.e., those that are required for the datalake to be up-to-date"""

    all_migrations_file_name = _get_all_migrations_file_name
    with open(all_migrations_file_name, newline="") as csvfile:
        reader = csv.reader(csvfile, dialect="excel")
        all_migration_names = [row[0] for row in reader]

    return all_migration_names


def _print_count(command_line_args: list[str]):
    args = _get_valid_args_or_throw(command_line_args)

    uncommitted_migrations = get_uncommitted_migrations(
        args.data_storage_account_name,
        args.data_storage_account_key,
        args.wholesale_container_name,
    )

    uncommitted_migrations_count = len(uncommitted_migrations)

    # This format is fixed as it is being used by external tools
    print(f"uncommitted_migrations_count={uncommitted_migrations_count}")


def get_uncommitted_migrations(
    storage_account_name: str, storage_account_key: str, container_name: str
) -> list[str]:
    """Get list of migrations that have not yet been committed"""

    committed_migrations = download_committed_migrations(
        storage_account_name, storage_account_key, container_name
    )

    all_migrations = _get_all_migrations()

    uncommitted_migrations = [
        m for m in all_migrations if m not in committed_migrations
    ]

    return uncommitted_migrations


# This method must remain parameterless because it will be called from the entry point when deployed.
def print_count():
    print_count(sys.argv[1:])
