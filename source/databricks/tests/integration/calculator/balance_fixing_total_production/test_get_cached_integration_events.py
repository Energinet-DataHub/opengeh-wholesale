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

"""
This test need to be removed when the static datasource has been implemented
from datetime import datetime, timedelta
import pytest
from package.balance_fixing_total_production import (
    _get_cached_integration_events,
)


# Factory defaults
first_of_june = datetime.strptime("31/05/2022 22:00", "%d/%m/%Y %H:%M")
second_of_june = first_of_june + timedelta(days=1)
third_of_june = first_of_june + timedelta(days=2)


@pytest.fixture
def integration_events_df_factory(spark):
    def factory(stored_time=first_of_june):
        row = {"storedTime": stored_time, "body": ""}

        return spark.createDataFrame([row])

    return factory


def test__raw_events_with_stored_time_of_after_snapshot_time_is_not_included_in_cacheched_integration_events(
    integration_events_df_factory,
):
    events_df_first_of_june = integration_events_df_factory(stored_time=first_of_june)
    events_df_second_of_june = integration_events_df_factory(stored_time=second_of_june)
    events_df_third_of_june = integration_events_df_factory(stored_time=third_of_june)

    # events from first second and third of June
    events_df = events_df_first_of_june.union(events_df_second_of_june).union(
        events_df_third_of_june
    )

    # assert that only events from first of june and before is returned when snapshot time is first of june
    assert _get_cached_integration_events(events_df, first_of_june).count() == 1
    # assert that only events from second of june and before is returned when snapshot time is second of june
    assert _get_cached_integration_events(events_df, second_of_june).count() == 2
    # assert that only events from third of june and before is returned when snapshot time is third of june
    assert _get_cached_integration_events(events_df, third_of_june).count() == 3
"""
