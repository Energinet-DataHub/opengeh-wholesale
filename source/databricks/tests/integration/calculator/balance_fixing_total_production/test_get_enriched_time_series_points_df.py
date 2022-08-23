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
    def factory(
        time: datetime = timestamp_factory("2022-06-08T12:09:15.000Z"),
    ):
        df = [
            {
                "GsrnNumber": "the-gsrn-number",
                "TransactionId": "1",
                "Quantity": Decimal("1.1"),
                "Quality": 3,
                "Resolution": 2,
                "RegistrationDateTime": timestamp_factory("2022-06-10T12:09:15.000Z"),
                "storedTime": timestamp_factory("2022-06-10T12:09:15.000Z"),
                "time": time,
                "year": 2022,
                "month": 6,
                "day": 8,
            }
        ]
        return spark.createDataFrame(df)

    return factory


@pytest.fixture(scope="module")
def metering_point_period_df_factory(spark, timestamp_factory):
    def factory(
        effective_date: datetime = timestamp_factory("2022-06-08T12:09:15.000Z"),
        to_effective_date: datetime = timestamp_factory("2022-06-08T13:09:15.000Z"),
    ):
        df = [
            {
                "GsrnNumber": "the-gsrn-number",
                "GridAreaCode": "805",
                "EffectiveDate": effective_date,
                "toEffectiveDate": to_effective_date,
                "Resolution": 2,
            }
        ]
        return spark.createDataFrame(df)

    return factory


@pytest.mark.parametrize(
    "period_start, period_end, expected_rows",
    [
        # period_start = time and period_end > time should have 1
        ("2022-06-08T12:09:15.000Z", "2022-06-08T12:09:16.000Z", 1),
        # period_start < time and period_end > time should have 1
        ("2022-06-08T12:09:14.000Z", "2022-06-08T12:09:16.000Z", 1),
        # period_start > time and period_end > time should have 0
        ("2022-06-08T12:09:16.000Z", "2022-06-08T12:09:16.000Z", 0),
        # period_start = time and period_end = time should have 0
        ("2022-06-08T12:09:15.000Z", "2022-06-08T12:09:15.000Z", 0),
    ],
)
def test__given_different_period_start_and_period_end__return_dataframe_with_correct_number_of_rows(
    raw_time_series_points_factory,
    metering_point_period_df_factory,
    timestamp_factory,
    period_start,
    period_end,
    expected_rows,
):
    """Test the outcome of _get_enriched_time_series_points_df with different scenarios.
    expected_rows is the number of rows in the output dataframe when given different parameters,
    period_start, period_end"""

    # Arrange
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory("2022-06-08T12:09:15.000Z")
    )
    metering_point_period_df = metering_point_period_df_factory()

    # Act
    df = _get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(period_start),
        timestamp_factory(period_end),
    )

    # Assert
    assert df.count() == expected_rows


@pytest.mark.parametrize(
    "effective_date, to_effective_date, expected_rows",
    [
        # effective_date = time and to_effective_date > time should have 1
        ("2022-06-08T12:09:15.000Z", "2022-06-08T12:09:16.000Z", 1),
        # effective_date < time and to_effective_date > time should have 1
        ("2022-06-08T12:09:14.000Z", "2022-06-08T12:09:16.000Z", 1),
        # effective_date > time and to_effective_date > time should have 0
        ("2022-06-08T12:09:16.000Z", "2022-06-08T12:09:16.000Z", 0),
        # effective_date = time and to_effective_date = time should have 0
        ("2022-06-08T12:09:15.000Z", "2022-06-08T12:09:15.000Z", 0),
    ],
)
def test__given_different_effective_date_and_to_effective_date__return_dataframe_with_correct_number_of_rows(
    raw_time_series_points_factory,
    metering_point_period_df_factory,
    timestamp_factory,
    effective_date,
    to_effective_date,
    expected_rows,
):
    """Test the outcome of _get_enriched_time_series_points_df with different scenarios.
    expected_rows is the number of rows in the output dataframe when given different parameters,
    effective_date and to_effective_date"""

    # Arrange
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory("2022-06-08T12:09:15.000Z")
    )
    metering_point_period_df = metering_point_period_df_factory(
        effective_date=effective_date, to_effective_date=to_effective_date
    )

    # Act
    df = _get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory("2022-06-08T12:09:15.000Z"),
        timestamp_factory("2022-06-08T13:09:15.000Z"),
    )

    # Assert
    assert df.count() == expected_rows


@pytest.mark.parametrize(
    "time, expected_rows",
    [
        ("2022-06-08T12:09:16.000Z", 1),
        ("2022-06-08T12:09:15.000Z", 1),
        ("2022-06-08T12:09:14.000Z", 0),
        ("2022-06-08T13:09:15.000Z", 0),
        ("2022-06-08T13:09:16.000Z", 0),
    ],
)
def test__given_raw_time_series_points_with_different_time__return_dataframe_with_correct_number_of_rows(
    raw_time_series_points_factory,
    metering_point_period_df_factory,
    timestamp_factory,
    time,
    expected_rows,
):
    """Test the outcome of _get_enriched_time_series_points_df with different scenarios.
    expected_rows is the number of rows in the output dataframe when given different time on time series point"""

    # Arrange
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory(time)
    )
    metering_point_period_df = metering_point_period_df_factory(
        effective_date=timestamp_factory("2022-06-08T12:09:15.000Z"),
        to_effective_date=timestamp_factory("2022-06-08T13:09:15.000Z"),
    )

    # Act
    actual = _get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory("2022-06-08T12:09:15.000Z"),
        timestamp_factory("2022-06-08T13:09:15.000Z"),
    )

    # Assert
    assert actual.count() == expected_rows
