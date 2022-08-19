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
from package.schemas import grid_area_updated_event_schema
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


def test__stored_time_matches_persister(grid_area_df_factory, source_path):
    """Test that the anticipated stored time column name matches the column that was created
    by the integration events persister. This test uses the shared contract."""
    raw_integration_events_df = grid_area_df_factory()

    expected_stored_time_name = get_from_file(
        f"{source_path}/contracts/events/grid-area-updated.json"
    )["storedTimeName"]

    assert expected_stored_time_name in raw_integration_events_df.columns


def test__body_prop_names_and_types_matches_integration_event_listener(
    grid_area_df_factory, source_path
):
    """Test that the property names and types of the body data matches
    the event data originating from the integration event listener.
    The test uses a shared contract."""
    raw_integration_events_df = grid_area_df_factory()
    grid_area_updated_expected_schema = get_from_file(
        f"{source_path}/contracts/events/grid-area-updated.json"
    )
    actual_schema_fields = json.loads(grid_area_updated_event_schema.json())["fields"]

    for i in grid_area_updated_expected_schema["bodyFields"]:
        actual_field = next(x for x in actual_schema_fields if i["name"] == x["name"])
    assert i["name"] == actual_field["name"]
    assert i["type"] == actual_field["type"]

    actual_df_fields = json.loads(
        _get_grid_areas_df(raw_integration_events_df, [grid_area_code]).schema.json()
    )["fields"]

    for i in actual_df_fields:
        actual_field = next(
            x
            for x in grid_area_updated_expected_schema["bodyFields"]
            if i["name"] == x["name"]
        )
    assert i["name"] == actual_field["name"]
    assert i["type"] == actual_field["type"]


def test__when_using_same_message_type_as_ingestor__returns_correct_grid_area_data(
    grid_area_df_factory, source_path
):
    # Arrange
    grid_area_updated_schema = get_from_file(
        f"{source_path}/contracts/events/grid-area-updated.json"
    )
    message_type = next(
        x for x in grid_area_updated_schema["bodyFields"] if x["name"] == "MessageType"
    )["value"]
    raw_integration_events_df = grid_area_df_factory(message_type=message_type)

    # Act
    actual_df = _get_grid_areas_df(raw_integration_events_df, [grid_area_code])

    # Assert
    assert actual_df.count() == 1


def test__returns_correct_grid_area_data(grid_area_df_factory):
    # Arrange
    raw_integration_events_df = grid_area_df_factory()

    # Act
    actual_df = _get_grid_areas_df(raw_integration_events_df, [grid_area_code])

    # Assert
    actual = actual_df.first()
    assert actual.GridAreaCode == grid_area_code
    assert actual.GridAreaLinkId == grid_area_link_id


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
        _get_grid_areas_df(raw_integration_events_df, [non_matching_grid_area_code])


def test__when_message_type_is_not_grid_area_updated__throws_because_grid_area_not_found(
    grid_area_df_factory,
):
    # Arrange
    raw_integration_events_df = grid_area_df_factory(
        message_type="some-other-message-type"
    )

    # Act and assert exception
    with pytest.raises(Exception, match=r".* grid areas .*"):
        _get_grid_areas_df(raw_integration_events_df, [grid_area_code])


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
    actual_df = _get_grid_areas_df(raw_integration_events_df, [grid_area_code])

    # Assert
    assert actual_df.count() == 1
    actual = actual_df.first()
    assert actual.GridAreaLinkId == expected_grid_area_link_id
