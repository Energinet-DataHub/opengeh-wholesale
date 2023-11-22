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

from package import calculation
from package.calculator_job_args import get_calculator_args
from package.container import create_and_configure_container
from package.infrastructure import (
    db_logging,
    log,
)
from package.infrastructure.storage_account_access import islocked


def start() -> None:
    """
    The start() method should only have its name updated in correspondence with the
    wheels entry point for it. Further the method must remain parameterless because
    it will be called from the entry point when deployed.
    """
    # Enable dependency injection
    create_and_configure_container()

    args = get_calculator_args()

    db_logging.loglevel = "information"

    if islocked(args.data_storage_account_name, args.data_storage_account_credentials):
        log("Exiting because storage is locked due to data migrations running.")
        sys.exit(3)

    calculation.execute(args)
