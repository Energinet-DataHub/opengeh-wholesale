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
from package.balance_fixing import (
    _get_enriched_time_series_points_df,
)

from package.codelists import (
    MeteringPointResolution,
    TimeSeriesQuality,
)
from package.constants import Colname
from pyspark.sql.functions import col


@pytest.fixture(scope="module")
def raw_time_series_points_factory(spark, timestamp_factory):
    def factory(
        time: datetime = timestamp_factory("2022-06-08T12:09:15.000Z"),
    ):
        df = [
            {
                "MeteringPointId": "the-meteringpoint-id",
                "Quantity": Decimal("1.1"),
                "Quality": TimeSeriesQuality.calculated.value,
                Colname.observation_time: time,
            }
        ]
        return spark.createDataFrame(df)

    return factory


@pytest.fixture(scope="module")
def metering_point_period_df_factory(spark, timestamp_factory):
    def factory(
        resolution,
        effective_date: datetime = timestamp_factory("2022-01-01T22:00:00.000Z"),
        to_effective_date: datetime = timestamp_factory("2022-12-22T22:00:00.000Z"),
    ):
        df = [
            {
                "MeteringPointId": "the-meteringpoint-id",
                "GridAreaCode": "805",
                "Type": "the_metering_point_type",
                "EffectiveDate": effective_date,
                "toEffectiveDate": to_effective_date,
                "Resolution": resolution,
            }
        ]
        return spark.createDataFrame(df)

    return factory


point_1_quantity = Decimal("1.100")
point_2_quantity = Decimal("2.200")


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
        resolution=MeteringPointResolution.quarter.value,
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
        effective_date=effective_date,
        to_effective_date=to_effective_date,
        resolution=MeteringPointResolution.quarter.value,
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


def test__missing_point_has_quantity_null_for_quarterly_resolution(
    raw_time_series_points_factory, metering_point_period_df_factory, timestamp_factory
):
    # Arrange
    start_time = "2022-06-08T22:00:00.000Z"
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory(start_time),
    )

    metering_point_period_df = metering_point_period_df_factory(
        resolution=MeteringPointResolution.quarter.value
    )
    # Act
    actual = _get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(start_time),
        timestamp_factory("2022-06-09T22:00:00.000Z"),
    )

    # Assert
    # We remove the point we created before inspecting the remaining
    actual = actual.filter(
        col(Colname.observation_time) != timestamp_factory(start_time)
    )
    assert actual.where(col("Quantity").isNull()).count() == 95


def test__missing_point_has_quantity_null_for_hourly_resolution(
    raw_time_series_points_factory, metering_point_period_df_factory, timestamp_factory
):
    # Arrange
    start_time = "2022-06-08T22:00:00.000Z"
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory(start_time),
    )

    metering_point_period_df = metering_point_period_df_factory(
        resolution=MeteringPointResolution.hour.value
    )

    # Act
    actual = _get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(start_time),
        timestamp_factory("2022-06-09T22:00:00.000Z"),
    )

    # Assert
    # We remove the point we created before inspecting the remaining
    actual = actual.filter(
        col(Colname.observation_time) != timestamp_factory(start_time)
    )
    assert actual.where(col("Quantity").isNull()).count() == 23


def test__missing_point_has_quality_incomplete_for_quarterly_resolution(
    raw_time_series_points_factory, metering_point_period_df_factory, timestamp_factory
):
    # Arrange
    start_time = "2022-06-08T12:00:00.000Z"
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory(start_time),
    )

    metering_point_period_df = metering_point_period_df_factory(
        resolution=MeteringPointResolution.quarter.value
    )

    # Act
    actual = _get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(start_time),
        timestamp_factory("2022-06-08T22:00:00.000Z"),
    )

    # Assert
    # We remove the point we created before inspecting the remaining
    actual = actual.filter(
        col(Colname.observation_time) != timestamp_factory(start_time)
    )
    assert actual.count() > 1
    assert actual.where(col("quality").isNull()).count() == actual.count()


def test__missing_point_has_quality_incomplete_for_hourly_resolution(
    raw_time_series_points_factory, metering_point_period_df_factory, timestamp_factory
):
    # Arrange
    start_time = "2022-06-08T22:00:00.000Z"
    end_time = "2022-06-09T22:00:00.000Z"
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory(start_time)
    )

    metering_point_period_df = metering_point_period_df_factory(
        effective_date=timestamp_factory(start_time),
        to_effective_date=timestamp_factory(end_time),
        resolution=MeteringPointResolution.hour.value,
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
    actual = actual.filter(
        col(Colname.observation_time) != timestamp_factory(start_time)
    )
    assert actual.count() > 1
    assert actual.where(col("quality").isNull()).count() == actual.count()


def test__df_is_not_empty_when_no_time_series_points(
    raw_time_series_points_factory, metering_point_period_df_factory, timestamp_factory
):
    # Arrange
    start_time = "2022-06-08T22:00:00.000Z"
    end_time = "2022-06-09T22:00:00.000Z"

    empty_raw_time_series_points = raw_time_series_points_factory().filter(
        col("MeteringPointId") == ""
    )
    metering_point_period_df = metering_point_period_df_factory(
        resolution=MeteringPointResolution.quarter.value,
        effective_date=timestamp_factory(start_time),
        to_effective_date=timestamp_factory(end_time),
    )

    # Act
    actual = _get_enriched_time_series_points_df(
        empty_raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(start_time),
        timestamp_factory(end_time),
    )

    # Assert
    assert actual.count() == 96


@pytest.mark.parametrize(
    "period_start, period_end, resolution, expected_number_of_rows",
    [
        # DST has 24 hours
        (
            "2022-06-08T22:00:00.000Z",
            "2022-06-09T22:00:00.000Z",
            MeteringPointResolution.quarter.value,
            96,
        ),
        # DST has 24 hours
        (
            "2022-06-08T22:00:00.000Z",
            "2022-06-09T22:00:00.000Z",
            MeteringPointResolution.hour.value,
            24,
        ),
        # going from DST to standard time there are 25 hours (100 quarters)
        # where the 30 oktober is day with 25 hours.and
        # Therefore there should be 100 rows for quarter resolution and 25 for  hour resolution
        (
            "2022-10-29T22:00:00.000Z",
            "2022-10-30T23:00:00.000Z",
            MeteringPointResolution.quarter.value,
            100,
        ),
        (
            "2022-10-29T22:00:00.000Z",
            "2022-10-30T23:00:00.000Z",
            MeteringPointResolution.hour.value,
            25,
        ),
        # going from vinter to summertime there are 23 hours (92 quarters)
        (
            "2022-03-26T23:00:00.000Z",
            "2022-03-27T22:00:00.000Z",
            MeteringPointResolution.hour.value,
            23,
        ),
        (
            "2022-03-26T23:00:00.000Z",
            "2022-03-27T22:00:00.000Z",
            MeteringPointResolution.quarter.value,
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
        time=timestamp_factory(period_start)
    ).filter(col("MeteringPointId") != "the-meteringpoint-id")

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
