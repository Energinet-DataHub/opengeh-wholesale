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


# Further the method must remain parameterless because it will be called from the entry point when deployed.
import configargparse
from datamigration import start_databricks_jobs, get_api_client
from package.args_helper import valid_log_level

from package import (
    log,
    debug,
    db_logging,
)


# Further the method must remain parameterless because it will be called from the entry point when deployed.
def start():
    args = _get_valid_args_or_throw()
    log(f"Job arguments: {str(args)}")
    db_logging.loglevel = args.log_level

    api_client = get_api_client(args.databricks_host, args.token)

    jobs_to_start = ["IntegrationEventsPersisterStreamingJob"]
    start_databricks_jobs(api_client, jobs_to_start)


def _get_valid_args_or_throw():
    p = configargparse.ArgParser(
        description="Performs domain calculations for submitted batches",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    # Infrastructure settings
    p.add("--data-storage-account-name", type=str, required=True)
    p.add("--data-storage-account-key", type=str, required=True)
    p.add("--integration-events-path", type=str, required=True)
    p.add("--process-results-path", type=str, required=True)
    p.add("--databricks-host", type=str, required=True)
    p.add("--databricks-token", type=str, required=True)

    p.add(
        "--log-level",
        type=valid_log_level,
        help="debug|information",
    )

    args, unknown_args = p.parse_known_args()
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return args
