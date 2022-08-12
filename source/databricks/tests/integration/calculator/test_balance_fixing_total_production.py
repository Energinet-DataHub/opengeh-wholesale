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
from package.balance_fixing_total_production import (
    _get_grid_areas,
    _get_enriched_time_series_points,
    _get_metering_point_periods,
    _get_result,
)
from pyspark.sql.functions import col


@pytest.fixture(scope="session")
def batch_grid_areas() -> str:
    return ["805", "806"]


@pytest.fixture(scope="session")
def snapshot_datetime() -> str:
    return datetime.now()


@pytest.fixture(scope="session")
def batch_id() -> str:
    return 42


@pytest.fixture(scope="session")
def raw_integration_events_df(spark, delta_lake_path):
    return spark.read.json(
        f"{delta_lake_path}/../calculator/test_files/integration_events.json"
    ).withColumn("body", col("body").cast("binary"))


@pytest.fixture(scope="session")
def raw_time_series_points_df(spark, delta_lake_path):
    return spark.read.json(
        f"{delta_lake_path}/../calculator/test_files/time_series_points.json"
    )


@pytest.fixture(scope="session")
def period_start_datetime() -> str:
    return datetime.strptime("31/05/2022 22:00", "%d/%m/%Y %H:%M")


@pytest.fixture(scope="session")
def period_end_datetime() -> str:
    return datetime.strptime("1/06/2022 22:00", "%d/%m/%Y %H:%M")


def test_balance_fixing_total_production_generates_non_empty_result(
    spark,
    batch_grid_areas,
    batch_id,
    snapshot_datetime,
    raw_time_series_points_df,
    raw_integration_events_df,
    period_start_datetime,
    period_end_datetime,
):
    # Period represents the 1st of June 2022 CEST

    result = calculate_balance_fixing_total_production(
        raw_integration_events_df,
        raw_time_series_points_df,
        batch_id,
        batch_grid_areas,
        snapshot_datetime,
        period_start_datetime,
        period_end_datetime,
    )
    print(result.count())
    assert result.count() > 0, "Could not verify created json file."


def test__get_grid_areas(
    spark,
    delta_lake_path,
    batch_grid_areas,
    snapshot_datetime,
    raw_integration_events_df,
):

    grid_area_df = _get_grid_areas(
        raw_integration_events_df, batch_grid_areas, snapshot_datetime
    )

    assert grid_area_df.count() == 2


def test__get_metering_point_periods(
    batch_grid_areas,
    snapshot_datetime,
    raw_integration_events_df,
    period_start_datetime,
    period_end_datetime,
):
    grid_area_df = _get_grid_areas(
        raw_integration_events_df, batch_grid_areas, snapshot_datetime
    )
    metering_point_periods_df = _get_metering_point_periods(
        raw_integration_events_df,
        grid_area_df,
        snapshot_datetime,
        period_start_datetime,
        period_end_datetime,
    )
    print(metering_point_periods_df.count())
    assert metering_point_periods_df.count() == 4


def test__get_enriched_time_series_points(
    batch_grid_areas,
    snapshot_datetime,
    raw_integration_events_df,
    raw_time_series_points_df,
    period_start_datetime,
    period_end_datetime,
):
    grid_area_df = _get_grid_areas(
        raw_integration_events_df, batch_grid_areas, snapshot_datetime
    )
    metering_point_period_df = _get_metering_point_periods(
        raw_integration_events_df,
        grid_area_df,
        snapshot_datetime,
        period_start_datetime,
        period_end_datetime,
    )

    enriched_time_series_points_df = _get_enriched_time_series_points(
        raw_time_series_points_df,
        metering_point_period_df,
        snapshot_datetime,
        period_start_datetime,
        period_end_datetime,
    )

    assert enriched_time_series_points_df.count() == 240
    assert (
        enriched_time_series_points_df.filter(col("GridAreaCode").isNotNull()).count()
        == 240
    )
