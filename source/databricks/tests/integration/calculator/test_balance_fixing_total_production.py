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
from package import calculate_balance_fixing_total_production
from pyspark.sql.types import IntegerType


def test_balance_fixing_total_production(
    spark, delta_lake_path, find_first_file, json_lines_reader
):
    from datetime import datetime

    batch_id = datetime.now().strftime("%Y-%m-%d_%H_%M_%S")
    grid_areas = ["805", "806"]
    snapshot_datetime = datetime.now()
    period_start_datetime = datetime.strptime('31/05/2022 22:00', '%d/%m/%Y %H:%M')
    period_end_datetime = datetime.strptime('1/06/2022 22:00', '%d/%m/%Y %H:%M')
    integration_events_path = f"{delta_lake_path}/../calculator/test_files/integration_events.csv"
    time_series_points_path = f"{delta_lake_path}/../calculator/test_files/time_series_points.csv"
    raw_integration_events_df = spark.read.format("csv").load(integration_events_path)
    raw_time_series_points = spark.read.format("csv").load(time_series_points_path)
    
    result = calculate_balance_fixing_total_production(
        raw_integration_events_df,
        raw_time_series_points,
        batch_id,
        grid_areas,
        snapshot_datetime,
        period_start_datetime,
        period_end_datetime
    )


    assert len(result.count()) > 0, "Could not verify created json file."
