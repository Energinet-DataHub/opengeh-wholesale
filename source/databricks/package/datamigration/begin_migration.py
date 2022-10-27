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
import configargparse
from databricks_cli.sdk.api_client import ApiClient

from datamigration import stop_databricks_jobs
from package import (
    log,
    debug,
    db_logging,
)


def start():
    log("Migrations job starting...")
    args = _get_valid_args_or_throw()
    log(f"Job arguments: {str(args)}")
    db_logging.loglevel = args.log_level

    api_client = ApiClient(
        host=args.databricks_host,  # "https://adb-5870161604877074.14.azuredatabricks.net"
        token=args.databricks_token,  # "the token",
    )
    jobs_to_stop = [
        "CalculatorJob",
        "IntegrationEventsPersisterStreamingJob",
        "persister_streaming_job",
        "publisher_streaming_job",
    ]
    stop_databricks_jobs(api_client, jobs_to_stop)

    # start migration


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
    p.add("--databricks_host", type=str, required=True)
    p.add("--databricks_toen", type=str, required=True)

    p.add(
        "--log-level",
        type=_valid_log_level,
        help="debug|information",
    )

    args, unknown_args = p.parse_known_args()
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return args


def _valid_log_level(s):
    if s in ["information", "debug"]:
        return str(s)
    else:
        msg = "loglevel is not valid"
        raise configargparse.ArgumentTypeError(msg)
