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
from types import SimpleNamespace
from package import calculate_balance_fixing_total_production
from package.balance_fixing_total_production import (
    _get_metering_point_periods_df,
    metering_point_created_message_type,
    metering_point_connected_message_type,
)
from package.schemas import (
    metering_point_created_event_schema,
    metering_point_connected_event_schema,
)
from package.codelists import (
    ConnectionState,
    MeteringPointType,
    Resolution,
    SettlementMethod,
)
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, struct, to_json, from_json
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
        resolution=Resolution.hour,
    ):
        row = {
            "storedTime": stored_time,
            "OperationTime": operation_time,
            "MessageType": message_type,
            "GsrnNumber": gsrn_number,
            "GridAreaLinkId": grid_area_link_id,
            "SettlementMethod": SettlementMethod.non_profiled,
            "ConnectionState": ConnectionState.new,
            "EffectiveDate": first_of_june,
            "MeteringPointType": MeteringPointType.production,
            "MeteringPointId": metering_point_id,
            "Resolution": resolution,
            "CorrelationId": "some-correlation-id",
        }

        return (
            spark.createDataFrame([row])
            .withColumn(
                "body",
                # TODO BJARKE: Can we do `to_json(struct(col("body.*")))`?
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
                ).cast("binary"),
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
                ).cast("binary"),
            )
            .select("storedTime", "body")
        )

    return factory


# TODO BJARKE: Test that the generic schema is the union of the created and connected events


def test__stored_time_of_metering_point_created_matches_persister(
    metering_point_created_df_factory, source_path
):
    raw_integration_events_df = metering_point_created_df_factory()

    expected_stored_time_name = read_contract(
        f"{source_path}/contracts/events/metering-point-created.json"
    )["storedTimeName"]

    assert expected_stored_time_name in raw_integration_events_df.columns


def test__stored_time_of_metering_point_connected_matches_persister(
    metering_point_connected_df_factory, source_path
):
    raw_integration_events_df = metering_point_connected_df_factory()

    expected_stored_time_name = read_contract(
        f"{source_path}/contracts/events/metering-point-connected.json"
    )["storedTimeName"]

    assert expected_stored_time_name in raw_integration_events_df.columns


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


def test__when_connected__returns_correct_period_data(
    metering_point_created_df_factory, metering_point_connected_df_factory, grid_area_df
):
    # Arrange
    integration_events_df = metering_point_created_df_factory().union(
        metering_point_connected_df_factory()
    )

    # Act
    actual_df = _get_metering_point_periods_df(
        integration_events_df, grid_area_df, second_of_june, third_of_june
    )

    # Assert
    actual = actual_df.first()
    assert actual.GsrnNumber == gsrn_number
    assert actual.GridAreaCode == grid_area_code
    assert actual.EffectiveDate == second_of_june
    assert actual.toEffectiveDate > the_far_future
    assert actual.Resolution == Resolution.hour
