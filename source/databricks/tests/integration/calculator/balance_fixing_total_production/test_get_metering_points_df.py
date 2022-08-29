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
import pytest
from package.balance_fixing_total_production import (
    _get_metering_point_periods_df,
    metering_point_created_message_type,
    metering_point_connected_message_type,
)
from package.schemas import (
    metering_point_created_event_schema,
    metering_point_connected_event_schema,
    metering_point_generic_event_schema,
)
from package.codelists import (
    ConnectionState,
    MeteringPointType,
    Resolution,
    SettlementMethod,
)
from pyspark.sql.functions import col, struct, to_json
from tests.contract_utils import (
    assert_contract_matches_schema,
    read_contract,
    get_message_type,
)


# Factory defaults
grid_area_code = "805"
grid_area_link_id = "the-grid-area-link-id"
gsrn_number = "the-gsrn-number"
metering_point_id = "the-metering-point-id"

first_of_june = datetime.strptime("31/05/2022 22:00", "%d/%m/%Y %H:%M")
second_of_june = first_of_june + timedelta(days=1)
third_of_june = first_of_june + timedelta(days=2)
the_far_future = datetime.now()  # 200 years into the future is anticipated to be "far"


@pytest.fixture
def grid_area_df(spark):
    row = {
        "GridAreaCode": grid_area_code,
        "GridAreaLinkId": grid_area_link_id,
    }
    return spark.createDataFrame([row])


@pytest.fixture
def metering_point_created_df_factory(spark):
    def factory(
        stored_time=first_of_june,
        message_type=metering_point_created_message_type,
        operation_time=first_of_june,
        grid_area_link_id=grid_area_link_id,
        gsrn_number=gsrn_number,
        effective_date=first_of_june,
        metering_point_type=MeteringPointType.production.value,
        resolution=Resolution.hour.value,
    ):
        row = {
            "storedTime": stored_time,
            "OperationTime": operation_time,
            "MessageType": message_type,
            "GsrnNumber": gsrn_number,
            "GridAreaLinkId": grid_area_link_id,
            "SettlementMethod": SettlementMethod.nonprofiled.value,
            "ConnectionState": ConnectionState.new.value,
            "EffectiveDate": effective_date,
            "MeteringPointType": metering_point_type,
            "MeteringPointId": metering_point_id,
            "Resolution": resolution,
            "CorrelationId": "some-correlation-id",
        }

        return (
            spark.createDataFrame([row])
            .withColumn(
                "body",
                to_json(
                    struct(
                        col("MessageType"),
                        col("OperationTime"),
                        col("GridAreaLinkId"),
                        col("GsrnNumber"),
                        col("GridAreaLinkId"),
                        col("MessageType"),
                        col("SettlementMethod"),
                        col("ConnectionState"),
                        col("EffectiveDate"),
                        col("MeteringPointType"),
                        col("MeteringPointId"),
                        col("Resolution"),
                        col("OperationTime"),
                        col("CorrelationId"),
                    )
                ),
            )
            .select("storedTime", "body")
        )

    return factory


@pytest.fixture
def metering_point_connected_df_factory(spark):
    def factory(
        stored_time=second_of_june,
        message_type=metering_point_connected_message_type,
        operation_time=second_of_june,
        grid_area_link_id=grid_area_link_id,
        gsrn_number=gsrn_number,
        effective_date=second_of_june,
        metering_point_id=metering_point_id,
    ):
        row = [
            {
                "storedTime": stored_time,
                "OperationTime": operation_time,
                "MessageType": message_type,
                "GsrnNumber": gsrn_number,
                "EffectiveDate": effective_date,
                "MeteringPointId": metering_point_id,
                "CorrelationId": "some-other-correlation-id",
            }
        ]

        return (
            spark.createDataFrame(row)
            .withColumn(
                "body",
                to_json(
                    struct(
                        col("MessageType"),
                        col("OperationTime"),
                        col("GsrnNumber"),
                        col("MessageType"),
                        col("EffectiveDate"),
                        col("MeteringPointId"),
                        col("OperationTime"),
                        col("CorrelationId"),
                    )
                ),
            )
            .select("storedTime", "body")
        )

    return factory


def test__schema_for_created__is_subsets_of_generic_schema():
    for created_field in metering_point_created_event_schema:
        generic_field = next(
            f
            for f in metering_point_generic_event_schema
            if f.name == created_field.name
        )
        assert created_field.dataType == generic_field.dataType


def test__schema_for_connected__is_subsets_of_generic_schema():
    for connected_field in metering_point_connected_event_schema:
        generic_field = next(
            f
            for f in metering_point_generic_event_schema
            if f.name == connected_field.name
        )
        assert connected_field.dataType == generic_field.dataType


def test__stored_time_of_metering_point_created_matches_persister(
    metering_point_created_df_factory, source_path
):
    cached_integration_events_df = metering_point_created_df_factory()

    expected_stored_time_name = read_contract(
        f"{source_path}/contracts/events/metering-point-created.json"
    )["storedTimeName"]

    assert expected_stored_time_name in cached_integration_events_df.columns


def test__stored_time_of_metering_point_connected_matches_persister(
    metering_point_connected_df_factory, source_path
):
    cached_integration_events_df = metering_point_connected_df_factory()

    expected_stored_time_name = read_contract(
        f"{source_path}/contracts/events/metering-point-connected.json"
    )["storedTimeName"]

    assert expected_stored_time_name in cached_integration_events_df.columns


def test__metering_point_created_schema_matches_contract(source_path):
    assert_contract_matches_schema(
        f"{source_path}/contracts/events/metering-point-created.json",
        metering_point_created_event_schema,
    )


def test__metering_point_connected_schema_matches_contract(source_path):
    assert_contract_matches_schema(
        f"{source_path}/contracts/events/metering-point-connected.json",
        metering_point_connected_event_schema,
    )


def test__metering_point_created_message_type__matches_contract(
    metering_point_created_df_factory,
    grid_area_df,
    source_path,
):
    contract_message_type = get_message_type(
        f"{source_path}/contracts/events/metering-point-created.json"
    )

    assert metering_point_created_message_type == contract_message_type


def test__metering_point_connected_message_type__matches_contract(
    metering_point_connected_df_factory,
    grid_area_df,
    source_path,
):
    contract_message_type = get_message_type(
        f"{source_path}/contracts/events/metering-point-connected.json"
    )

    assert metering_point_connected_message_type == contract_message_type


@pytest.mark.parametrize(
    "created_message_type,connected_message_type,expected",
    [
        ("MeteringPointCreated", "MeteringPointConnected", True),
        ("BadCreatedMessageType", "MeteringPointConnected", False),
        ("MeteringPointCreated", "BadConnectedMessageType", False),
        ("BadCreatedMessageType", "BadConnectedMessageType", False),
    ],
)
def test__when_correct_message_types__returns_row(
    metering_point_created_df_factory,
    metering_point_connected_df_factory,
    grid_area_df,
    created_message_type,
    connected_message_type,
    expected,
):
    # Arrange
    created_events_df = metering_point_created_df_factory(
        message_type=created_message_type
    )
    connected_events_df = metering_point_connected_df_factory(
        message_type=connected_message_type
    )
    integration_events_df = created_events_df.union(connected_events_df)

    # Act
    actual_df = _get_metering_point_periods_df(
        integration_events_df, grid_area_df, second_of_june, third_of_june
    )

    # Assert
    assert (actual_df.count() == 1) == expected


@pytest.mark.parametrize(
    "metering_point_type,expected_is_included",
    [
        (MeteringPointType.production.value, True),
        (MeteringPointType.production.value - 1, False),
        (MeteringPointType.production.value + 1, False),
    ],
)
def test__when_metering_point_type_is_production__metering_point_is_included(
    metering_point_created_df_factory,
    metering_point_connected_df_factory,
    grid_area_df,
    metering_point_type,
    expected_is_included,
):
    # Arrange
    created_events_df = metering_point_created_df_factory(
        metering_point_type=metering_point_type
    )
    connected_events_df = metering_point_connected_df_factory()
    integration_events_df = created_events_df.union(connected_events_df)

    # Act
    actual_df = _get_metering_point_periods_df(
        integration_events_df, grid_area_df, second_of_june, third_of_june
    )

    # Assert
    actual_is_included = actual_df.count() == 1
    assert actual_is_included == expected_is_included


def test__gsrn_code_value(
    metering_point_created_df_factory, metering_point_connected_df_factory, grid_area_df
):
    # Arrange
    created_events_df = metering_point_created_df_factory()
    connected_events_df = metering_point_connected_df_factory()
    integration_events_df = created_events_df.union(connected_events_df)

    # Act
    actual_df = _get_metering_point_periods_df(
        integration_events_df, grid_area_df, second_of_june, third_of_june
    )

    # Assert
    assert actual_df.first().GsrnNumber == gsrn_number


def test__grid_area_code(
    metering_point_created_df_factory, metering_point_connected_df_factory, grid_area_df
):
    # Arrange
    created_events_df = metering_point_created_df_factory()
    connected_events_df = metering_point_connected_df_factory()
    integration_events_df = created_events_df.union(connected_events_df)

    # Act
    actual_df = _get_metering_point_periods_df(
        integration_events_df, grid_area_df, second_of_june, third_of_june
    )

    # Assert
    assert actual_df.first().GridAreaCode == grid_area_code


def test__effective_date__matches_effective_date_of_connected_event(
    metering_point_created_df_factory, metering_point_connected_df_factory, grid_area_df
):
    # Arrange
    effective_date_of_connected_event = second_of_june + timedelta(seconds=1)

    created_events_df = metering_point_created_df_factory()
    connected_events_df = metering_point_connected_df_factory(
        effective_date=effective_date_of_connected_event
    )
    integration_events_df = created_events_df.union(connected_events_df)

    # Act
    actual_df = _get_metering_point_periods_df(
        integration_events_df, grid_area_df, second_of_june, third_of_june
    )

    # Assert
    assert actual_df.first().EffectiveDate == effective_date_of_connected_event


def test__to_effective_date__is_in_the_far_future(
    metering_point_created_df_factory, metering_point_connected_df_factory, grid_area_df
):
    """toEffectiveDate can only be "far future" as only connected metering points
    are included and no further events are currently supported."""

    # Arrange
    created_events_df = metering_point_created_df_factory()
    connected_events_df = metering_point_connected_df_factory()
    integration_events_df = created_events_df.union(connected_events_df)

    # Act
    actual_df = _get_metering_point_periods_df(
        integration_events_df, grid_area_df, second_of_june, third_of_june
    )

    # Assert
    assert actual_df.first().toEffectiveDate > the_far_future


@pytest.mark.parametrize(
    "effective_date,period_end_date,expected_is_included",
    [
        (third_of_june, third_of_june, True),
        (third_of_june + timedelta(seconds=1), third_of_june, False),
        (third_of_june - timedelta(seconds=1), third_of_june, True),
    ],
)
def test__when_effective_date_less_than_or_equal_to_period_end__row_is_included(
    metering_point_created_df_factory,
    metering_point_connected_df_factory,
    grid_area_df,
    effective_date,
    period_end_date,
    expected_is_included,
):
    # Arrange
    created_events_df = metering_point_created_df_factory()
    connected_events_df = metering_point_connected_df_factory(
        effective_date=effective_date
    )
    integration_events_df = created_events_df.union(connected_events_df)

    # Act
    actual_df = _get_metering_point_periods_df(
        integration_events_df, grid_area_df, second_of_june, period_end_date
    )

    # Assert
    actual_is_included = actual_df.count() == 1
    assert actual_is_included == expected_is_included


@pytest.mark.parametrize(
    "created_effective_date,connected_effective_date,expected",
    [
        (first_of_june, second_of_june, True),
        (second_of_june, first_of_june, False),
    ],
)
def test__metering_points_are_periodized_by_effective_date(
    metering_point_created_df_factory,
    metering_point_connected_df_factory,
    grid_area_df,
    created_effective_date,
    connected_effective_date,
    expected,
):
    # Arrange
    created_events_df = metering_point_created_df_factory(
        effective_date=created_effective_date
    )
    connected_events_df = metering_point_connected_df_factory(
        effective_date=connected_effective_date
    )
    integration_events_df = connected_events_df.union(created_events_df)

    # Act
    actual_df = _get_metering_point_periods_df(
        integration_events_df, grid_area_df, second_of_june, third_of_june
    )

    # Assert
    assert (actual_df.count() == 1) == expected
