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
    _get_cached_integration_events,
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


@pytest.fixture
def integration_events_df_factory(spark):
    def factory(stored_time=first_of_june):
        row = {
            "storedTime": stored_time,
            "OperationTime": first_of_june,
            "MessageType": metering_point_created_message_type,
            "GsrnNumber": gsrn_number,
            "GridAreaLinkId": grid_area_link_id,
            "SettlementMethod": SettlementMethod.nonprofiled.value,
            "ConnectionState": ConnectionState.new.value,
            "EffectiveDate": first_of_june,
            "MeteringPointType": MeteringPointType.production.value,
            "MeteringPointId": metering_point_id,
            "Resolution": Resolution.hour.value,
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


def test__raw_events_with_stored_time_of_after_snapshot_time_is_not_included_in_cacheched_integration_events(
    integration_events_df_factory, source_path
):
    events_df_first_of_june = integration_events_df_factory(stored_time=first_of_june)

    events_df_second_of_june = integration_events_df_factory(stored_time=second_of_june)

    events_df_third_of_june = integration_events_df_factory(stored_time=third_of_june)
    events_df = events_df_first_of_june.union(events_df_second_of_june).union(
        events_df_third_of_june
    )

    assert _get_cached_integration_events(events_df, first_of_june).count() == 1
    assert _get_cached_integration_events(events_df, second_of_june).count() == 2
    assert _get_cached_integration_events(events_df, third_of_june).count() == 3
