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
from package import calculator
from pyspark.sql.types import IntegerType
from package.schemas import eventhub_integration_events_schema


def test_calculator_creates_file(
    spark, delta_lake_path, write_str_to_file, find_first_file, json_lines_reader
):
    batchId = 1234
    process_results_path = f"{delta_lake_path}/results"
    integration_events_path = f"{delta_lake_path}/integration_events"

    if os.path.exists(integration_events_path):
        shutil.rmtree(integration_events_path)

    mp_created = "{'GsrnNumber':'575387199703339827','GridAreaLinkId':'f5a0cdeb-79dd-4a18-a20a-1210fb84daf0','SettlementMethod':'2','ConnectionState':'1','EffectiveDate':'2021-09-25T22:00:00.000Z','MeteringPointType':'1','Resolution':'1','CorrelationId':'00-106dd5f611c1f2478f54d47e244318b8-044879afbff6c946-00','MessageType':'MeteringPointCreated','OperationTime':'2022-06-29T11:26:31.000Z'}"
    grid_area_updated = "{'GridAreaCode':'805','GridAreaLinkId':'f5a0cdeb-79dd-4a18-a20a-1210fb84daf0','CorrelationId':'00-206dd5f611c1f2478f54d47e244318b8-044879afbff6c946-00','MessageType':'GridAreaUpdatedIntegrationEvent','OperationTime':'2022-06-29T11:26:31.000Z'}"

    write_str_to_file(
        integration_events_path,
        "events.json",
        f"{mp_created}\n{grid_area_updated}",
    )

    raw_integration_events_df = spark.read.format("json").load(integration_events_path)

    calculator(spark, raw_integration_events_df, process_results_path, batchId)

    jsonFile = find_first_file(
        f"{delta_lake_path}/results/batch_id={batchId}/grid_area=805", "part-*.json"
    )

    result = json_lines_reader(jsonFile)
    assert len(result) > 0, "Could not verify created json file."
