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
import sys
from .job_handler import stop_databricks_jobs, start_databricks_jobs, get_api_client
from package.args_helper import valid_log_level
from package import (
    log,
    debug,
    db_logging,
)


def _get_valid_args_or_throw(command_line_args: list[str]):
    p = configargparse.ArgParser(
        description="Performs domain calculations for submitted batches",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    # Infrastructure settings
    p.add("--databricks-host", type=str, required=True)
    p.add("--databricks-token", type=str, required=True)

    p.add(
        "--log-level",
        type=valid_log_level,
        help="debug|information",
    )

    args, unknown_args = p.parse_known_args(args=command_line_args)
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return args


def _stop_db_jobs(command_line_args: list[str]):
    log("Stoping_db_jobs...")
    args = _get_valid_args_or_throw(command_line_args)

    log(f"Job arguments: {str(args)}")
    db_logging.loglevel = args.log_level

    api_client = get_api_client(args.databricks_host, args.databricks_token)

    jobs_to_stop = ["CalculatorJob", "IntegrationEventsPersisterStreamingJob"]
    stop_databricks_jobs(api_client, jobs_to_stop)


def _start_db_jobs(command_line_args: list[str]):
    args = _get_valid_args_or_throw(command_line_args)
    log(f"Job arguments: {str(args)}")

    db_logging.loglevel = args.log_level

    api_client = get_api_client(args.databricks_host, args.databricks_token)

    jobs_to_start = ["IntegrationEventsPersisterStreamingJob"]
    start_databricks_jobs(api_client, jobs_to_start)


def start_db_jobs():
    _start_db_jobs(sys.argv[1:])


def stop_db_jobs():
    _stop_db_jobs(sys.argv[1:])
