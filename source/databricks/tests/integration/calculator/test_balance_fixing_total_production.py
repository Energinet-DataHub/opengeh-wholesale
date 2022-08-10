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


from datetime import datetime
import os
import shutil
import pytest
from package import calculate_balance_fixing_total_production
from pyspark.sql.functions import col


def test_balance_fixing_total_production_generates_non_empty_result(
    spark, delta_lake_path, find_first_file, json_lines_reader
):
    batch_id = 42
    batch_grid_areas = ["805", "806"]
    snapshot_datetime = datetime.now()

    # Period represents the 1st of June 2022 CEST
    period_start_datetime = datetime.strptime("31/05/2022 22:00", "%d/%m/%Y %H:%M")
    period_end_datetime = datetime.strptime("1/06/2022 22:00", "%d/%m/%Y %H:%M")

    # Get dataframe for integration events (metering points and grid areas)
    integration_events_path = (
        f"{delta_lake_path}/../calculator/test_files/integration_events.json"
    )
    raw_integration_events_df = spark.read.json(integration_events_path).withColumn(
        "body", col("body").cast("binary")
    )

    # Get dataframe for time series points
    time_series_points_path = (
        f"{delta_lake_path}/../calculator/test_files/time_series_points.json"
    )
    raw_time_series_points_df = spark.read.json(time_series_points_path)

    result = calculate_balance_fixing_total_production(
        raw_integration_events_df,
        raw_time_series_points_df,
        batch_id,
        batch_grid_areas,
        snapshot_datetime,
        period_start_datetime,
        period_end_datetime,
    )

    assert result.count() > 0, "Could not verify created json file."
