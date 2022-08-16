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


from datetime import datetime, timedelta
import os
import shutil
import pytest
import json
from package import calculate_balance_fixing_total_production
from package.balance_fixing_total_production import _get_grid_areas_df
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, struct, to_json


# Factory defaults
grid_area_code = "805"
grid_area_link_id = "the-grid-area-link-id"
message_type = "GridAreaUpdated"  # The exact name of the event of interest

# Beginning of the danish date (CEST)
first_of_june = datetime.strptime("31/05/2022 22:00", "%d/%m/%Y %H:%M")
second_of_june = first_of_june + timedelta(days=1)
third_of_june = first_of_june + timedelta(days=2)


def get_from_file(path):
    jsonFile = open(path)
    return json.load(jsonFile)


# TODO: How do we ensure that the generated dataframe has the exact schema expected by the sut?
@pytest.fixture
def grid_area_df_factory(spark):
    def factory(
        stored_time=first_of_june,
        grid_area_code=grid_area_code,
        grid_area_link_id=grid_area_link_id,
        message_type=message_type,
        operation_time=first_of_june,
    ):
        row = [
            {
                "storedTime": stored_time,
                "OperationTime": operation_time,
                "GridAreaCode": grid_area_code,
                "GridAreaLinkId": grid_area_link_id,
                "MessageType": message_type,
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
                        col("OperationTime"),
                    )
                ).cast("binary"),
            )
            .select("storedTime", "body")
        )

    return factory


def test__when_using_same_message_type_as_ingestor__returns_correct_grid_area_data(
    grid_area_df_factory, source_path
):
    # Arrange
    message_types = get_from_file(
        f"{source_path}/contracts/events/grid-area-updated.json"
    )
    message_type = message_types["MessageType"]["value"]
    raw_integration_events_df = grid_area_df_factory(message_type=message_type)

    # Act
    actual_df = _get_grid_areas_df(
        raw_integration_events_df, [grid_area_code], snapshot_datetime=second_of_june
    )

    # Assert
    assert actual_df.count() == 1


def test__returns_correct_grid_area_data(grid_area_df_factory):
    # Arrange
    raw_integration_events_df = grid_area_df_factory()

    # Act
    actual_df = _get_grid_areas_df(
        raw_integration_events_df, [grid_area_code], snapshot_datetime=second_of_june
    )

    # Assert
    actual = actual_df.first()
    assert actual.GridAreaCode == grid_area_code
    assert actual.GridAreaLinkId == grid_area_link_id


def test__when_stored_time_equals_snapshot_datetime__returns_grid_area(
    grid_area_df_factory,
):
    # Arrange
    raw_integration_events_df = grid_area_df_factory(stored_time=first_of_june)

    # Act
    actual_df = _get_grid_areas_df(
        raw_integration_events_df, [grid_area_code], snapshot_datetime=first_of_june
    )

    # Assert
    assert actual_df.count() == 1


def test__when_stored_after_snapshot_time__throws_because_grid_area_not_found(
    grid_area_df_factory,
):
    # Arrange
    raw_integration_events_df = grid_area_df_factory(stored_time=second_of_june)

    # Act and assert exception
    with pytest.raises(Exception, match=r".* grid areas .*"):
        _get_grid_areas_df(
            raw_integration_events_df, [grid_area_code], snapshot_datetime=first_of_june
        )


def test__when_grid_area_code_does_not_match__throws_because_grid_area_not_found(
    grid_area_df_factory,
):
    # Arrange
    non_matching_grid_area_code = "999"
    raw_integration_events_df = grid_area_df_factory(
        stored_time=first_of_june, grid_area_code=grid_area_code
    )

    # Act and assert exception
    with pytest.raises(Exception, match=r".* grid areas .*"):
        _get_grid_areas_df(
            raw_integration_events_df,
            [non_matching_grid_area_code],
            snapshot_datetime=second_of_june,
        )


def test__when_message_type_is_not_grid_area_updated__throws_because_grid_area_not_found(
    grid_area_df_factory,
):
    # Arrange
    raw_integration_events_df = grid_area_df_factory(
        message_type="some-other-message-type"
    )

    # Act and assert exception
    with pytest.raises(Exception, match=r".* grid areas .*"):
        _get_grid_areas_df(
            raw_integration_events_df,
            [grid_area_code],
            snapshot_datetime=second_of_june,
        )


def test__returns_newest_grid_area_state(
    grid_area_df_factory,
):
    # Arrange
    expected_grid_area_link_id = "foo"
    unexpected_grid_area_link_id = "bar"

    # Make the row of interest the middle to increase the likeliness of the succes
    # to not depend on the row being selected by incident by being e.g. the first or last row.
    raw_integration_events_df = (
        grid_area_df_factory(
            operation_time=first_of_june,
            grid_area_link_id=unexpected_grid_area_link_id,
        )
        .union(
            grid_area_df_factory(
                operation_time=second_of_june,
                grid_area_link_id=expected_grid_area_link_id,
            )
        )
        .union(
            grid_area_df_factory(
                operation_time=third_of_june,
                grid_area_link_id=expected_grid_area_link_id,
            )
        )
    )

    # Act
    actual_df = _get_grid_areas_df(
        raw_integration_events_df,
        [grid_area_code],
        snapshot_datetime=second_of_june,
    )

    # Assert
    assert actual_df.count() == 1
    actual = actual_df.first()
    assert actual.GridAreaLinkId == expected_grid_area_link_id
