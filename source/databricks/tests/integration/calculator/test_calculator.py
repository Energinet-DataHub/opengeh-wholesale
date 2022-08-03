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


def test_calculator_creates_file(
    spark, delta_lake_path, find_first_file, json_lines_reader
):
    batchId = 1234
    process_results_path = f"{delta_lake_path}/results"
    integration_events_path = f"{delta_lake_path}/../calculator/test_files"

    raw_integration_events_df = spark.read.format("json").load(integration_events_path)

    calculator(spark, raw_integration_events_df, process_results_path, batchId)

    jsonFile = find_first_file(
        f"{delta_lake_path}/results/batch_id={batchId}/grid_area=805", "part-*.json"
    )

    result = json_lines_reader(jsonFile)
    assert len(result) > 0, "Could not verify created json file."
