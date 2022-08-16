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
from package.balance_fixing_total_production import _get_grid_areas_df
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, struct, to_json


# Factory defaults
grid_area_code = "805"
grid_area_link_id = "the-grid-area-link-id"
message_type = (
    "GridAreaUpdatedIntegrationEvent"  # The exact name of the event of interest
)

# Beginning of the danish date (CEST)
first_of_june = datetime.strptime("31/05/2022 22:00", "%d/%m/%Y %H:%M")
second_of_june = datetime.strptime("1/06/2022 22:00", "%d/%m/%Y %H:%M")


# TODO: How do we ensure that the generated dataframe has the exact schema expected by the sut?
@pytest.fixture
def grid_area_df_factory(spark):
    def factory(
        stored_time=first_of_june,
        body_grid_area_code=grid_area_code,
        body_grid_area_link_id=grid_area_link_id,
        body_message_type=message_type,
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
                to_json(
                    struct(
                        col("GridAreaCode"),
                        col("GridAreaLinkId"),
                        col("MessageType"),
                    )
                ).cast("binary"),
            )
            .select("storedTime", "body")
        )

    return factory


def test_returns_correct_grid_area_data(grid_area_df_factory):
    # Arrange
    raw_integration_events_df = grid_area_df_factory()

    # Act
    grid_area_df = _get_grid_areas_df(
        raw_integration_events_df, [grid_area_code], snapshot_datetime=second_of_june
    )

    # Assert
    assert grid_area_df.count() == 1
    actual = grid_area_df.first()
    assert actual.GridAreaCode == grid_area_code
    assert actual.GridAreaLinkId == grid_area_link_id


def test_when_stored_time_equals_snapshot_datetime_returns_grid_area(
    grid_area_df_factory,
):
    # Arrange
    raw_integration_events_df = grid_area_df_factory(stored_time=first_of_june)

    # Act
    grid_area_df = _get_grid_areas_df(
        raw_integration_events_df, [grid_area_code], snapshot_datetime=first_of_june
    )

    # Assert
    assert grid_area_df.count() == 1


# TODO: How do we know that this test is green for the right reason
def test_when_stored_after_snapshot_time_throws_because_grid_area_not_found(
    grid_area_df_factory,
):
    # Arrange
    raw_integration_events_df = grid_area_df_factory(stored_time=second_of_june)

    # Act and assert exception
    with pytest.raises(Exception, match=r".* grid areas .*"):
        _get_grid_areas_df(
            raw_integration_events_df, [grid_area_code], snapshot_datetime=first_of_june
        )


# TODO: How do we know that this test is green for the right reason
def test_when_grid_area_code_does_not_match_throws_because_grid_area_not_found(
    grid_area_df_factory,
):
    # Arrange
    non_matching_grid_area_code = "999"
    raw_integration_events_df = grid_area_df_factory(
        stored_time=first_of_june, body_grid_area_code=grid_area_code
    )

    # Act and assert exception
    with pytest.raises(Exception, match=r".* grid areas .*"):
        _get_grid_areas_df(
            raw_integration_events_df,
            [non_matching_grid_area_code],
            snapshot_datetime=second_of_june,
        )


# TODO: How do we know that this test is green for the right reason
def test_when_message_type_is_not_grid_area_updated_throws_because_grid_area_not_found(
    grid_area_df_factory,
):
    # Arrange
    raw_integration_events_df = grid_area_df_factory(
        body_message_type="some-other-message-type"
    )

    # Act and assert exception
    with pytest.raises(Exception, match=r".* grid areas .*"):
        _get_grid_areas_df(
            raw_integration_events_df,
            [grid_area_code],
            snapshot_datetime=second_of_june,
        )
