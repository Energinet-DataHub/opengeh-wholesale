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
from package.args_helper import valid_date, valid_list, valid_log_level
from package.datamigration import islocked
import configargparse
from azure.storage.filedatalake import DataLakeServiceClient


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
    p.add("--log-level", type=valid_log_level, help="debug|information", required=True)

    args, unknown_args = p.parse_known_args(args=command_line_args)
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return args


def _start(command_line_args: list[str]):
    args = _get_valid_args_or_throw(command_line_args)
    log(f"Job arguments: {str(args)}")
    db_logging.loglevel = args.log_level

    # TODO: toggle comments below when islocked functionality works
    # if islocked(args.data_storage_account_name, args.data_storage_account_key):
    #     log("Exiting because storage is locked due to data migrations running.")
    #     sys.exit(3)

    # try:
    #     is_locked = islocked(
    #         args.data_storage_account_name, args.data_storage_account_key, "processes"
    #     )
    #     log(f"processes: islocked={is_locked}")
    # except Exception:
    #     log("Exception occured in 'islocked' (processes).")

    create_file_and_check_existance(
        args.data_storage_account_name, args.data_storage_account_key
    )

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


def create_file_and_check_existance(
    data_storage_account_name, data_storage_account_key
):
    data_storage_account_url = (
        f"https://{data_storage_account_name}.dfs.core.windows.net"
    )

    file_system_name = "processes"

    service_client = DataLakeServiceClient(
        data_storage_account_url, data_storage_account_key
    )

#     file_system = service_client.get_file_system_client(file_system_name)

#     # we get an exception here
#     file_system_found = file_system.exists()
#     log(f"file_system_found={file_system_found}")

    file_system_client = service_client.get_file_system_client("processes")

    file_name = "my_file.txt"
    file_client = file_system_client.create_file(file_name)

    log("File created")

    file_client = file_system_client.get_file_client(file_name)
    file_client.create_file()

    file_exists = file_client.exists()
    log(f"file_exists={file_exists}")


# The start() method should only have its name updated in correspondence with the wheels entry point for it.
# Further the method must remain parameterless because it will be called from the entry point when deployed.
def start():
    _start(sys.argv[1:])
