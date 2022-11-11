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
from package import integration_events_persister, initialize_spark, log, db_logging
from package.datamigration import islocked
import configargparse


def _get_valid_args_or_throw(command_line_args: list[str]):
    p = configargparse.ArgParser(
        description="Integration events stream ingestor",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )
    p.add("--data-storage-account-name", type=str, required=True)
    p.add("--data-storage-account-key", type=str, required=True)
    p.add("--event-hub-connectionstring", type=str, required=True)
    p.add("--integration-events-path", type=str, required=True)
    p.add("--integration-events-checkpoint-path", type=str, required=True)

    args, unknown_args = p.parse_known_args(args=command_line_args)
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return args


def _start(command_line_args: list[str]):
    args = _get_valid_args_or_throw(command_line_args)
    log(f"Job arguments: {str(args)}")
    db_logging.loglevel = args.log_level

    if islocked():
        log("Exiting because storage is locked due to data migrations running.")
        sys.exit(3)

    spark = initialize_spark(
        args.data_storage_account_name, args.data_storage_account_key
    )

    input_configuration = {}
    input_configuration[
        "eventhubs.connectionString"
    ] = spark.sparkContext._gateway.jvm.org.apache.spark.eventhubs.EventHubsUtils.encrypt(
        f"{args.event_hub_connectionstring}"
    )

    streamingDF = (
        spark.readStream.format("eventhubs").options(**input_configuration).load()
    )

    integration_events_persister(
        streamingDF,
        args.integration_events_path,
        args.integration_events_checkpoint_path,
    )


# The start() method should only have its name updated in correspondence with the wheels entry point for it.
# Further the method must remain parameterless because it will be called from the entry point when deployed.
def start():
    _start(sys.argv[1:])
