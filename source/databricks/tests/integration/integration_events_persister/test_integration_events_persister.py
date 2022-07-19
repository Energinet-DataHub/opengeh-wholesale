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
import os
import shutil

sys.path.append(r"/workspaces/opengeh-wholesale/source/databricks")


import pytest
from package import integration_events_persister
from tests.integration.utils import streaming_job_asserter
from package.schemas import eventhub_integration_events_schema as schema


@pytest.fixture(scope="session")
def integration_events_persister_tester(spark, databricks_path, delta_lake_path):
    event_hub_streaming_simulation_path = (
        f"{delta_lake_path}/event_hub_streaming_simulation"
    )

    integration_events_path = f"{delta_lake_path}/integration_events"
    integration_events_checkpoint_path = f"{delta_lake_path}/integration_events_checkpoint"
    market_participant_events_path = f"{delta_lake_path}/market_participant_events"
    market_participant_events_checkpoint_path = f"{delta_lake_path}/market_participant_events_checkpoint"

    # Remove test folders in order to avoid side effects from previous/other test runs
    if os.path.exists(event_hub_streaming_simulation_path):
        shutil.rmtree(event_hub_streaming_simulation_path)
    if os.path.exists(integration_events_path):
        shutil.rmtree(integration_events_path)
    if os.path.exists(integration_events_checkpoint_path):
        shutil.rmtree(integration_events_checkpoint_path)
    if os.path.exists(market_participant_events_path):
        shutil.rmtree(market_participant_events_path)
    if os.path.exists(market_participant_events_checkpoint_path):
        shutil.rmtree(market_participant_events_checkpoint_path)

    os.makedirs(event_hub_streaming_simulation_path)
    f = open(f"{event_hub_streaming_simulation_path}/test.json", "w")
    f.write(
        "{\"body\":\"{'GsrnNumber':'575387199703339827','GridAreaLinkId':'f5a0cdeb-79dd-4a18-a20a-1210fb84daf0','SettlementMethod':'2','ConnectionState':'1','EffectiveDate':'2021-09-25T22:00:00.000Z','MeteringPointType':'1','Resolution':'1','CorrelationId':'00-106dd5f611c1f2478f54d47e244318b8-044879afbff6c946-00','MessageType':'MeteringPointCreated','OperationTime':'2022-06-29T11:26:31.000Z'}\",\"partition\":\"0\",\"offset\":\"8589936200\",\"sequenceNumber\":25,\"enqueuedTime\":\"2022-06-29T11:26:41.003Z\",\"properties\":{\"Diagnostic-Id\":\"00-3fac0b8fc6488252d3f9847f72178ec2-52587d983735b766-00\"},\"systemProperties\":{}}"
    )
    f.close()

    streamingDF = (
        spark.readStream.option("startingOffsets", "earliest")
        .format("json")
        .load(event_hub_streaming_simulation_path)
    )

    return integration_events_persister(
        streamingDF,
        integration_events_path,
        integration_events_checkpoint_path,
        market_participant_events_path,
        market_participant_events_checkpoint_path
    )


@pytest.mark.asyncio
async def test_process_json(parquet_reader, integration_events_persister_tester):
    def verification_function():
        data = parquet_reader("/integration_events")
        return data.count() > 0

    succeeded = streaming_job_asserter(
        integration_events_persister_tester, verification_function
    )
    assert succeeded, "No data was stored in Delta table"
