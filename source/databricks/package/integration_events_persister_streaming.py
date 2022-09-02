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

from package import integration_events_persister, initialize_spark, log, debug
import configargparse


def start():
    p = configargparse.ArgParser(
        description="Integration events stream ingestor",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )
    p.add("--data-storage-account-name", type=str, required=True)
    p.add("--data-storage-account-key", type=str, required=True)
    p.add("--event-hub-connectionstring", type=str, required=True)
    p.add("--integration-events-path", type=str, required=True)
    p.add("--integration-events-checkpoint-path", type=str, required=True)

    args, unknown_args = p.parse_known_args()
    log(f"Job arguments: {str(args)}")

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
