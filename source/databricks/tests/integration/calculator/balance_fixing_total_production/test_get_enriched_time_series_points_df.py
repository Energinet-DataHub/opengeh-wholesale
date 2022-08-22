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


import pytest
from decimal import Decimal
from datetime import datetime
from package.balance_fixing_total_production import _get_enriched_time_series_points_df
from tests.contract_utils import (
    assert_contract_matches_schema,
    read_contract,
    get_message_type,
)


@pytest.fixture(scope="module")
def raw_time_series_points_factory(spark, timestamp_factory):
    def factory():
        df = [
            {
                "GsrnNumber": "1",
                "TransactionId": "1",
                "Quantity": Decimal(1.1),
                "Quality": 3,
                "Resolution": 2,
                "RegistrationDateTime": timestamp_factory("2022-06-10T12:09:15.000Z"),
                "storedTime": timestamp_factory("2022-06-10T12:09:15.000Z"),
                "time": timestamp_factory("2022-06-08T12:09:15.000Z"),
                "year": 2022,
                "month": 6,
                "day": 8,
            },
            {
                "GsrnNumber": "1",
                "TransactionId": "1",
                "Quantity": Decimal(1.1),
                "Quality": 3,
                "Resolution": 2,
                "RegistrationDateTime": timestamp_factory("2022-06-10T12:09:15.000Z"),
                "storedTime": timestamp_factory("2022-06-08T12:09:15.000Z"),
                "time": timestamp_factory("2022-06-08T13:09:15.000Z"),
                "year": 2022,
                "month": 6,
                "day": 8,
            },
        ]
        return spark.createDataFrame(df)

    return factory


@pytest.fixture(scope="module")
def metering_point_period_df_factory(spark, timestamp_factory):
    def factory(
        effective_date: datetime = timestamp_factory("2022-06-08T12:09:15.000Z"),
        to_effective_date: datetime = timestamp_factory("2022-06-08T13:09:16.000Z"),
    ):
        df = [
            {
                "MessageType": "1",
                "GsrnNumber": "1",
                "GridAreaCode": "1",
                "EffectiveDate": effective_date,
                "toEffectiveDate": to_effective_date,
                "Resolution": 2,
            }
        ]
        return spark.createDataFrame(df)

    return factory


@pytest.mark.parametrize(
    "period_start, period_end, effective_date, to_effective_date, expected_rows",
    [
        (
            "2022-06-08T12:09:15.000Z",
            "2022-06-08T13:09:16.000Z",
            "2022-06-08T12:09:15.000Z",
            "2022-06-08T13:09:16.000Z",
            2,
        ),
        (
            "2022-06-08T12:09:15.000Z",
            "2022-06-08T13:09:15.000Z",
            "2022-06-08T12:09:15.000Z",
            "2022-06-08T13:09:15.000Z",
            1,
        ),
        (
            "2022-06-08T12:09:15.000Z",
            "2022-06-08T13:09:16.000Z",
            "2022-06-08T12:09:15.000Z",
            "2022-06-08T13:09:15.000Z",
            1,
        ),
        (
            "2022-06-08T12:09:15.000Z",
            "2022-06-08T13:09:15.000Z",
            "2022-06-08T12:09:15.000Z",
            "2022-06-08T13:09:16.000Z",
            1,
        ),
        (
            "2022-06-08T12:09:16.000Z",
            "2022-06-08T13:09:16.000Z",
            "2022-06-08T12:09:16.000Z",
            "2022-06-08T13:09:16.000Z",
            1,
        ),
    ],
)
def test__when(
    raw_time_series_points_factory,
    metering_point_period_df_factory,
    timestamp_factory,
    period_start,
    period_end,
    effective_date,
    to_effective_date,
    expected_rows,
):
    raw_time_series_points = raw_time_series_points_factory()
    metering_point_period_df = metering_point_period_df_factory(
        effective_date=effective_date, to_effective_date=to_effective_date
    )
    df = _get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(period_start),
        timestamp_factory(period_end),
    )

    assert expected_rows == df.count()
