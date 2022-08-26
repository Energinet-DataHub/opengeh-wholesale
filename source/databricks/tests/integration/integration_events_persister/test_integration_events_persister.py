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

import os
import shutil
import pytest
from package import integration_events_persister
from tests.integration.utils import streaming_job_asserter
from package.schemas import published_time_series_points_schema
from tests.contract_utils import assert_contract_matches_schema


@pytest.fixture(scope="session")
def integration_events_persister_tester(spark, databricks_path, data_lake_path):
    event_hub_streaming_simulation_path = (
        f"{data_lake_path}/../integration_events_persister/test_files"
    )

    integration_events_path = f"{data_lake_path}/integration_events"
    integration_events_checkpoint_path = (
        f"{data_lake_path}/integration_events_checkpoint"
    )

    # Remove test folders in order to avoid side effects from previous/other test runs
    if os.path.exists(integration_events_path):
        shutil.rmtree(integration_events_path)
    if os.path.exists(integration_events_checkpoint_path):
        shutil.rmtree(integration_events_checkpoint_path)

    streamingDF = (
        spark.readStream.option("startingOffsets", "earliest")
        .format("json")
        .load(event_hub_streaming_simulation_path)
    )

    return integration_events_persister(
        streamingDF,
        integration_events_path,
        integration_events_checkpoint_path,
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
