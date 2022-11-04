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
from .uncommitted_migrations_count import create_file, delete_file
import configargparse
from package import log


def lock():
    args = _get_valid_args_or_throw()
    if args.only_validate_args:
        exit(0)

    create_file(
        args.data_storage_account_name,
        args.data_storage_account_key,
        args.wholesale_container_name,
        "DATALAKE_IS_LOCKED"
    )
    log("created lock file: DATALAKE_IS_LOCKED")


def unlock():
    args = _get_valid_args_or_throw()
    if args.only_validate_args:
        exit(0)

    create_file(
        args.data_storage_account_name,
        args.data_storage_account_key,
        args.wholesale_container_name,
        "DATALAKE_IS_LOCKED"
    )
    log("deleted lock file: DATALAKE_IS_LOCKED")


def _get_valid_args_or_throw():
    p = configargparse.ArgParser(
        description="Returns number of uncommitted data migrations",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    p.add("--data-storage-account-name", type=str, required=True)
    p.add("--data-storage-account-key", type=str, required=True)
    p.add("--wholesale-container-name", type=str, required=True)
    p.add(
        "--only-validate-args",
        type=bool,
        required=False,
        default=False,
        help="Instruct the script to exit after validating input arguments.",
    )

    args, unknown_args = p.parse_known_args()
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return args
