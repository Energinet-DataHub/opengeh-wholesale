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
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, struct


@pytest.fixture(scope="session")
def batch_grid_areas() -> list:
    return ["805", "806"]


@pytest.fixture(scope="session")
def snapshot_datetime() -> datetime:
    return datetime.now()


@pytest.fixture(scope="session")
def raw_integration_events_df(spark, delta_lake_path) -> DataFrame:
    return spark.read.json(
        f"{delta_lake_path}/../calculator/test_files/integration_events.json"
    ).withColumn("body", col("body").cast("binary"))


@pytest.fixture(scope="session")
def raw_time_series_points_df(spark, delta_lake_path) -> DataFrame:
    return spark.read.json(
        f"{delta_lake_path}/../calculator/test_files/time_series_points.json"
    )


@pytest.fixture(scope="session")
def period_start_datetime() -> datetime:
    return datetime.strptime("31/05/2022 22:00", "%d/%m/%Y %H:%M")


@pytest.fixture(scope="session")
def period_end_datetime() -> datetime:
    return datetime.strptime("1/06/2022 22:00", "%d/%m/%Y %H:%M")


# TODO BJARKE: How do we ensure that the generated dataframe has the exact structure expected by the sut?
@pytest.fixture
def grid_area_df_factory(spark):
    def factory(
        stored_time=datetime.now(),
        body_grid_area_code="805",
        body_grid_area_link_id="the-grid-area-link-id",
        body_message_type="GridAreaUpdatedIntegrationEvent",
    ):
        row = [
            {
                "storedTime": stored_time,
                "GridAreaCode": body_grid_area_code,
                "GridAreaLinkId": body_grid_area_link_id,
                "MessageType": body_message_type,
            }
        ]

        return (
            spark.createDataFrame(row)
            .withColumn(
                "body",
                struct(
                    col("GridAreaCode"),
                    col("GridAreaLinkId"),
                    col("MessageType").cast("binary"),
                ),
            )
            .select("storedTime", "body")
        )

    return factory


"""
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
"""


def test_when_matching_selected_code_returns_grid_area(grid_area_df_factory):
    # Arrange
    snapshot_datetime = datetime.now()  # Must be later than the grid area event
    matching_grid_area_code = "805"

    raw_integration_events_df = grid_area_df_factory(
        body_grid_area_code=matching_grid_area_code
    )

    # Act
    grid_area_df = _get_grid_areas(
        raw_integration_events_df, [matching_grid_area_code], snapshot_datetime
    )

    # Assert
    assert grid_area_df.count() == 1
    actual = grid_area_df.first()
    assert actual.GridAreaCode == matching_grid_area_code


def test_when_not_matching_selected_code_returns_no_grid_area(grid_area_df_factory):
    # Arrange
    snapshot_datetime = datetime.now()  # Must be later than the grid area event
    non_matching_grid_area_code = "999"
    raw_integration_events_df = grid_area_df_factory(
        body_grid_area_code=non_matching_grid_area_code
    )

    # Act
    grid_area_df = _get_grid_areas(
        raw_integration_events_df, [non_matching_grid_area_code], snapshot_datetime
    )

    # Assert
    assert grid_area_df.count() == 0
