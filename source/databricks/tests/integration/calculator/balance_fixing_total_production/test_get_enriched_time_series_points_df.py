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
from package.balance_fixing_total_production import (
    _get_enriched_time_series_points_df,
)
from tests.contract_utils import (
    assert_contract_matches_schema,
    read_contract,
    get_message_type,
)

from package.codelists import Resolution, TimeSeriesQuality, MeteringpointResolution
from pyspark.sql.functions import col


@pytest.fixture(scope="module")
def raw_time_series_points_factory(spark, timestamp_factory):
    def factory(
        time: datetime = timestamp_factory("2022-06-08T12:09:15.000Z"),
        resolution: Resolution = Resolution.quarter.value,
    ):
        df = [
            {
                "GsrnNumber": "the-gsrn-number",
                "TransactionId": "1",
                "Quantity": Decimal("1.1"),
                "Quality": 3,
                "Resolution": resolution,
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
        effective_date: datetime = timestamp_factory("2022-06-08T12:00:00.000Z"),
        to_effective_date: datetime = timestamp_factory("2022-06-08T13:00:00.000Z"),
        resolution: MeteringpointResolution = MeteringpointResolution.quarterly.value,
    ):
        df = [
            {
                "GsrnNumber": "the-gsrn-number",
                "GridAreaCode": "805",
                "MeteringPointType": "the_metering_point_type",
                "EffectiveDate": effective_date,
                "toEffectiveDate": to_effective_date,
                "Resolution": resolution,
            }
        ]
        return spark.createDataFrame(df)

    return factory


point_1_quantity = Decimal("1.1")
point_2_quantity = Decimal("2.2")


@pytest.fixture(scope="module")
def raw_time_series_points_with_same_gsrn_and_time_factory(spark, timestamp_factory):
    def factory(
        registration_date_time_1: datetime = timestamp_factory(
            "2022-06-10T12:00:00.000Z"
        ),
        registration_date_time_2: datetime = timestamp_factory(
            "2022-06-10T12:15:00.000Z"
        ),
        stored_time_1: datetime = timestamp_factory("2022-06-10T12:09:15.000Z"),
        stored_time_2: datetime = timestamp_factory("2022-06-10T12:09:15.000Z"),
    ):
        df = [
            {
                "GsrnNumber": "the-gsrn-number",
                "TransactionId": "1",
                "Quantity": point_1_quantity,
                "Quality": 3,
                "Resolution": 2,
                "RegistrationDateTime": registration_date_time_1,
                "storedTime": stored_time_1,
                "time": timestamp_factory("2022-06-10T12:15:00.000Z"),
                "year": 2022,
                "month": 6,
                "day": 8,
            },
            {
                "GsrnNumber": "the-gsrn-number",
                "TransactionId": "1",
                "Quantity": point_2_quantity,
                "Quality": 3,
                "Resolution": 2,
                "RegistrationDateTime": registration_date_time_2,
                "storedTime": stored_time_2,
                "time": timestamp_factory("2022-06-10T12:15:00.000Z"),
                "year": 2022,
                "month": 6,
                "day": 8,
            },
        ]
        return spark.createDataFrame(df)

    return factory


time_1 = "2022-06-10T12:15:00.000Z"
time_2 = "2022-06-10T13:15:00.000Z"


@pytest.mark.parametrize(
    "period_start, period_end, expected_rows",
    [
        ("2022-06-08T22:00:00.000Z", "2022-06-09T22:00:00.000Z", 96),
        ("2022-06-08T22:00:00.000Z", "2022-06-10T22:00:00.000Z", 192),
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
        time=timestamp_factory("2022-06-08T22:15:00.000Z")
    )
    metering_point_period_df = metering_point_period_df_factory(
        effective_date=timestamp_factory("2022-06-08T12:00:00.000Z"),
        to_effective_date=timestamp_factory("2023-06-10T13:00:00.000Z"),
    )

    # Act
    actual = _get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(period_start),
        timestamp_factory(period_end),
    )

    # Assert
    assert actual.count() == expected_rows


@pytest.mark.parametrize(
    "effective_date, to_effective_date, expected_rows",
    [
        # effective_date = time and to_effective_date > time should have 1
        ("2022-06-15T22:00:00.000Z", "2022-06-16T22:00:00.000Z", 96),
        # effective_date < time and to_effective_date > time should have 1
        ("2022-06-14T22:00:00.000Z", "2022-06-16T22:00:00.000Z", 192),
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
        time=timestamp_factory("2022-06-08T12:15:00.000Z")
    )
    metering_point_period_df = metering_point_period_df_factory(
        effective_date=effective_date, to_effective_date=to_effective_date
    )

    # Act
    actual = _get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(effective_date),
        timestamp_factory(to_effective_date),
    )

    # Assert
    assert actual.count() == expected_rows


@pytest.mark.parametrize(
    "registration_date_time_1, registration_date_time_2, expected_quantity",
    [
        (time_1, time_2, point_2_quantity),
        (time_2, time_1, point_1_quantity),
    ],
)
def test__given_two_points_with_same_gsrn_and_time__only_uses_the_one_with_the_latest_registation_time(
    raw_time_series_points_with_same_gsrn_and_time_factory,
    metering_point_period_df_factory,
    timestamp_factory,
    registration_date_time_1,
    registration_date_time_2,
    expected_quantity,
):
    # Arrange
    raw_time_series_points = raw_time_series_points_with_same_gsrn_and_time_factory(
        registration_date_time_1=registration_date_time_1,
        registration_date_time_2=registration_date_time_2,
    )
    metering_point_period_df = metering_point_period_df_factory()

    # Act
    actual = _get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory("2022-06-10T12:00:00.000Z"),
        timestamp_factory("2022-06-10T13:00:00.000Z"),
    )

    # Assert
    assert actual.count() == 4
    assert (
        actual.filter(col("Quantity").isNotNull()).first().Quantity == expected_quantity
    )


def test__missing_point_has_quantity_null_for_quarterly_resolution(
    raw_time_series_points_factory, metering_point_period_df_factory, timestamp_factory
):
    # Arrange
    start_time = "2022-06-08T12:00:00.000Z"
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory(start_time),
        resolution=Resolution.quarter.value,
    )

    metering_point_period_df = metering_point_period_df_factory()

    # Act
    actual = _get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(start_time),
        timestamp_factory("2022-06-08T14:00:00.000Z"),
    )

    # Assert
    # We remove the point we created before inspecting the remaining
    actual = actual.filter(col("time") != timestamp_factory(start_time))

    assert actual.where(col("Quantity").isNull()).count() == actual.count()


def test__missing_point_has_quantity_null_for_hourly_resolution(
    raw_time_series_points_factory, metering_point_period_df_factory, timestamp_factory
):
    # Arrange
    start_time = "2022-06-08T12:00:00.000Z"
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory(start_time),
        resolution=Resolution.hour.value,
    )

    metering_point_period_df = metering_point_period_df_factory()

    # Act
    actual = _get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(start_time),
        timestamp_factory("2022-06-08T14:00:00.000Z"),
    )

    # Assert
    # We remove the point we created before inspecting the remaining
    actual = actual.filter(col("time") != timestamp_factory(start_time))

    assert actual.count() > 1
    assert actual.where(col("Quantity").isNull()).count() == actual.count()


def test__missing_point_has_quality_incomplete_for_quarterly_resolution(
    raw_time_series_points_factory, metering_point_period_df_factory, timestamp_factory
):
    # Arrange
    start_time = "2022-06-08T12:00:00.000Z"
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory(start_time),
        resolution=Resolution.quarter.value,
    )

    metering_point_period_df = metering_point_period_df_factory()

    # Act
    actual = _get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(start_time),
        timestamp_factory("2022-06-08T14:00:00.000Z"),
    )

    # Assert
    # We remove the point we created before inspecting the remaining
    actual = actual.filter(col("time") != timestamp_factory(start_time))

    assert (
        actual.where(col("quality") == TimeSeriesQuality.incomplete.value).count()
        == actual.count()
    )


def test__missing_point_has_quality_incomplete_for_hourly_resolution(
    raw_time_series_points_factory, metering_point_period_df_factory, timestamp_factory
):
    # Arrange
    start_time = "2022-06-08T12:00:00.000Z"
    end_time = "2022-06-08T14:00:00.000Z"
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory(start_time),
        resolution=Resolution.hour.value,
    )

    metering_point_period_df = metering_point_period_df_factory(
        effective_date=timestamp_factory(start_time),
        to_effective_date=timestamp_factory(end_time),
    )

    # Act
    actual = _get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(start_time),
        timestamp_factory(end_time),
    )

    # Assert
    # We remove the point we created before inspecting the remaining
    actual = actual.filter(col("time") != timestamp_factory(start_time))
    actual.show()
    assert (
        #    actual.where(col("quality") == TimeSeriesQuality.invalid.value).count()
        actual.where(col("quality").isNull()).count()
        == actual.count()
    )


def test__df_is_not_empty_when_no_time_series_points():
    raise Exception("TODO")


@pytest.mark.parametrize(
    "period_start, period_end, resolution, expected_number_of_rows",
    [
        # DST has 24 hours
        (
            "2022-06-08T22:00:00.000Z",
            "2022-06-09T22:00:00.000Z",
            MeteringpointResolution.quarterly.value,
            96,
        ),
        # DST has 24 hours
        (
            "2022-06-08T22:00:00.000Z",
            "2022-06-09T22:00:00.000Z",
            MeteringpointResolution.hour.value,
            24,
        ),
        # going from DST to standard time there are 25 hours (100 quarters)
        # where the 30 oktober is day with 25 hours.and
        # Therefore there should be 100 rows for quarter resolution and 25 for  hour resolution
        (
            "2022-10-29T22:00:00.000Z",
            "2022-10-30T23:00:00.000Z",
            MeteringpointResolution.quarterly.value,
            100,
        ),
        (
            "2022-10-29T22:00:00.000Z",
            "2022-10-30T23:00:00.000Z",
            MeteringpointResolution.hour.value,
            25,
        ),
        # going from vinter to summertime there are 23 hours (92 quarters)
        (
            "2022-03-26T23:00:00.000Z",
            "2022-03-27T22:00:00.000Z",
            MeteringpointResolution.hour.value,
            23,
        ),
        (
            "2022-03-26T23:00:00.000Z",
            "2022-03-27T22:00:00.000Z",
            MeteringpointResolution.quarterly.value,
            92,
        ),
    ],
)
def test__df_has_expected_row_count_according_to_dst(
    raw_time_series_points_factory,
    metering_point_period_df_factory,
    timestamp_factory,
    period_start,
    period_end,
    resolution,
    expected_number_of_rows,
):
    # Arrange
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory(period_start), resolution=resolution
    ).filter(col("GsrnNumber") != "the-gsrn-number")

    metering_point_period_df = metering_point_period_df_factory(
        effective_date=timestamp_factory(period_start),
        to_effective_date=timestamp_factory(period_end),
        resolution=resolution,
    )

    # Act
    actual = _get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(period_start),
        timestamp_factory(period_end),
    )
    assert actual.count() == expected_number_of_rows
