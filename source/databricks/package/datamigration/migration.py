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

import importlib
import sys

import configargparse
from package import infrastructure, initialize_spark, log
from package.args_helper import valid_log_level

from .committed_migrations import upload_committed_migration
from .data_lake_file_manager import DataLakeFileManager
from .migration_script_args import MigrationScriptArgs
from .uncommitted_migrations import get_uncommitted_migrations
from typing import Any
from configargparse import argparse


def _get_valid_args_or_throw(command_line_args: list[str]) -> argparse.Namespace:
    p = configargparse.ArgParser(
        description="Apply uncommitted migations",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    p.add("--data-storage-account-name", type=str, required=True)
    p.add("--data-storage-account-key", type=str, required=True)
    p.add(
        "--log-level",
        type=valid_log_level,
        help="debug|information",
    )

    args, unknown_args = p.parse_known_args(command_line_args)
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return args


def _apply_migration(migration_name: str, migration_args: MigrationScriptArgs) -> None:
    migration = importlib.import_module(
        "package.datamigration.migration_scripts." + migration_name
    )
    migration.apply(
        migration_args,
    )


def _migrate_data_lake(storage_account_name: str, storage_account_key: str) -> None:

    spark = initialize_spark(
        storage_account_name,
        storage_account_key,
    )
    file_manager = DataLakeFileManager(
        storage_account_name,
        storage_account_key,
        infrastructure.WHOLESALE_CONTAINER_NAME,
    )

    uncommitted_migrations = get_uncommitted_migrations(file_manager)

    storage_account_url = infrastructure.get_storage_account_url(
        storage_account_name,
    )

    migration_args = MigrationScriptArgs(
        storage_account_url, storage_account_key, spark
    )

    for name in uncommitted_migrations:
        print(name)
        _apply_migration(name, migration_args)
        upload_committed_migration(file_manager, name)


# This method must remain parameterless because it will be called from the entry point when deployed.
def migrate_data_lake() -> None:
    args = _get_valid_args_or_throw(sys.argv[1:])
    _migrate_data_lake(args.data_storage_account_name, args.data_storage_account_key)
