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
sys.path.append(r'/workspaces/opengeh-wholesale/source/databricks')
sys.path.append(r'/opt/conda/lib/python3.8/site-packages')

import configargparse

from package import integration_events_persister, initialize_spark


p = configargparse.ArgParser(description='Timeseries events stream ingestor', formatter_class=configargparse.ArgumentDefaultsHelpFormatter)
p.add('--data-storage-account-name', type=str, required=True)
p.add('--data-storage-account-key', type=str, required=True)
p.add('--event-hub-connectionstring', type=str, required=True)
p.add('--integration-events-path', type=str, required=True)
p.add('--integration-events-checkpoint-path', type=str, required=True)

args, unknown_args = p.parse_known_args()

spark = initialize_spark(args)

integration_events_path = f"{args.integration_events_path}"
integration_events_checkpoint_path = f"{args.integration_events_checkpoint_path}"

input_configuration = {}
input_configuration["eventhubs.connectionString"] = spark.sparkContext._gateway.jvm.org.apache.spark.eventhubs.EventHubsUtils.encrypt(f"{args.event_hub_connectionstring}")
streamingDF = spark.readStream.format("eventhubs").options(**input_configuration).load()

# start the timeseries persister job
integration_events_persister(streamingDF, integration_events_checkpoint_path, integration_events_path)