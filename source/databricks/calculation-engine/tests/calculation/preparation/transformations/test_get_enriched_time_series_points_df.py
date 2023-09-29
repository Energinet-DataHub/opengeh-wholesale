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
from package.calculation.preparation.transformations import (
    get_enriched_time_series_points_df,
)
from package.constants import Colname

from package.codelists import (
    MeteringPointResolution,
    TimeSeriesQuality,
)
from pyspark.sql.functions import col


@pytest.fixture(scope="module")
def raw_time_series_points_factory(spark, timestamp_factory):
    def factory(
        time: datetime = timestamp_factory("2022-06-08T12:15:00.000Z"),
    ):
        df = [
            {
                "metering_point_id": "the-meteringpoint-id",
                "quantity": Decimal("1.1"),
                "quality": TimeSeriesQuality.CALCULATED.value,
                Colname.observation_time: time,
            }
        ]
        return spark.createDataFrame(df)

    return factory


@pytest.fixture(scope="module")
def metering_point_period_df_factory(spark, timestamp_factory):
    def factory(
        resolution,
        from_date: datetime = timestamp_factory("2022-01-01T22:00:00.000Z"),
        to_date: datetime = timestamp_factory("2022-12-22T22:00:00.000Z"),
    ):
        df = [
            {
                Colname.metering_point_id: "the-meteringpoint-id",
                Colname.grid_area: "805",
                Colname.from_date: from_date,
                Colname.to_date: to_date,
                Colname.metering_point_type: "the_metering_point_type",
                Colname.settlement_method: "D01",
                Colname.from_grid_area: "",
                Colname.to_grid_area: "",
                Colname.resolution: resolution,
                Colname.energy_supplier_id: "someId",
                Colname.balance_responsible_id: "someId",
            }
        ]
        return spark.createDataFrame(df)

    return factory


point_1_quantity = Decimal("1.100")
point_2_quantity = Decimal("2.200")


time_1 = "2022-06-10T12:15:00.000Z"
time_2 = "2022-06-10T13:15:00.000Z"


@pytest.mark.parametrize(
    "from_date, to_date, expected_rows, resolution",
    [
        ("2022-01-01T22:00:00.000Z", "2022-01-02T22:00:00.000Z", 96, "PT15M"),
        ("2022-01-01T22:00:00.000Z", "2022-01-03T22:00:00.000Z", 192, "PT15M"),
        ("2022-01-01T22:00:00.000Z", "2022-01-02T22:00:00.000Z", 24, "PT1H"),
        ("2022-01-01T22:00:00.000Z", "2022-01-03T22:00:00.000Z", 48, "PT1H"),
    ],
)
def test__given_different_from_date_and_to_date__return_dataframe_with_correct_number_of_rows(
    raw_time_series_points_factory,
    metering_point_period_df_factory,
    timestamp_factory,
    from_date,
    to_date,
    expected_rows,
    resolution,
):
    """Test the outcome of _get_enriched_time_series_points_df with different scenarios.
    expected_rows is the number of rows in the output dataframe when given different parameters,
    FromDate and ToDate on the meteringpoint"""

    # Arrange
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory("2022-06-08T12:15:00.000Z")
    )
    metering_point_period_df = metering_point_period_df_factory(
        from_date=from_date,
        to_date=to_date,
        resolution=resolution,
    )

    # Act
    actual = get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(from_date),
        timestamp_factory(to_date),
    )

    # Assert
    assert actual.count() == expected_rows


def test__missing_point_has_quantity_null_for_quarterly_resolution(
    raw_time_series_points_factory, metering_point_period_df_factory, timestamp_factory
):
    # Arrange
    start_time = "2022-06-08T22:00:00.000Z"
    end_time = "2022-06-09T22:00:00.000Z"
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory(start_time),
    )

    metering_point_period_df = metering_point_period_df_factory(
        resolution=MeteringPointResolution.QUARTER.value,
        from_date=start_time,
        to_date=end_time,
    )
    # Act
    actual = get_enriched_time_series_points_df(
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
    assert actual.where(col("quantity").isNull()).count() == 95


def test__missing_point_has_quantity_null_for_hourly_resolution(
    raw_time_series_points_factory, metering_point_period_df_factory, timestamp_factory
):
    # Arrange
    start_time = "2022-06-08T22:00:00.000Z"
    end_time = "2022-06-09T22:00:00.000Z"
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory(start_time),
    )

    metering_point_period_df = metering_point_period_df_factory(
        resolution=MeteringPointResolution.HOUR.value,
        from_date=start_time,
        to_date=end_time,
    )

    # Act
    actual = get_enriched_time_series_points_df(
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
    assert actual.where(col("quantity").isNull()).count() == 23


def test__missing_point_has_quality_incomplete_for_quarterly_resolution(
    raw_time_series_points_factory, metering_point_period_df_factory, timestamp_factory
):
    # Arrange
    start_time = "2022-06-08T12:00:00.000Z"
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory(start_time),
    )

    metering_point_period_df = metering_point_period_df_factory(
        resolution=MeteringPointResolution.QUARTER.value
    )

    # Act
    actual = get_enriched_time_series_points_df(
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
        from_date=timestamp_factory(start_time),
        to_date=timestamp_factory(end_time),
        resolution=MeteringPointResolution.HOUR.value,
    )

    # Act
    actual = get_enriched_time_series_points_df(
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
        col(Colname.metering_point_id) == ""
    )
    metering_point_period_df = metering_point_period_df_factory(
        resolution=MeteringPointResolution.QUARTER.value,
        from_date=timestamp_factory(start_time),
        to_date=timestamp_factory(end_time),
    )

    # Act
    actual = get_enriched_time_series_points_df(
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
            MeteringPointResolution.QUARTER.value,
            96,
        ),
        # DST has 24 hours
        (
            "2022-06-08T22:00:00.000Z",
            "2022-06-09T22:00:00.000Z",
            MeteringPointResolution.HOUR.value,
            24,
        ),
        # going from DST to standard time there are 25 hours (100 quarters)
        # where the 30 oktober is day with 25 hours.and
        # Therefore there should be 100 rows for quarter resolution and 25 for  hour resolution
        (
            "2022-10-29T22:00:00.000Z",
            "2022-10-30T23:00:00.000Z",
            MeteringPointResolution.QUARTER.value,
            100,
        ),
        (
            "2022-10-29T22:00:00.000Z",
            "2022-10-30T23:00:00.000Z",
            MeteringPointResolution.HOUR.value,
            25,
        ),
        # going from vinter to summertime there are 23 hours (92 quarters)
        (
            "2022-03-26T23:00:00.000Z",
            "2022-03-27T22:00:00.000Z",
            MeteringPointResolution.HOUR.value,
            23,
        ),
        (
            "2022-03-26T23:00:00.000Z",
            "2022-03-27T22:00:00.000Z",
            MeteringPointResolution.QUARTER.value,
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
    ).filter(col("metering_point_id") != "the-meteringpoint-id")

    metering_point_period_df = metering_point_period_df_factory(
        from_date=timestamp_factory(period_start),
        to_date=timestamp_factory(period_end),
        resolution=resolution,
    )

    # Act
    actual = get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(period_start),
        timestamp_factory(period_end),
    )
    assert actual.count() == expected_number_of_rows


@pytest.mark.parametrize(
    "from_date, to_date, from_date_hour_mp, to_date_hour_mp, from_date_quarter_mp, to_date_quarter_mp, total, quarterly, hourly",
    [
        (
            "2022-01-01T22:00:00.000Z",  # from_date
            "2022-01-03T22:00:00.000Z",  # to_date
            "2022-01-01T22:00:00.000Z",  # from_date_hour_mp
            "2022-01-02T22:00:00.000Z",  # to_date_hour_mp
            "2022-01-02T22:00:00.000Z",  # from_date_quarter_mp
            "2022-01-03T22:00:00.000Z",  # to_date_quarter_mp
            120,  # total
            96,  # quarterly
            24,  # hourly
        ),
        (
            "2022-01-01T22:00:00.000Z",  # from_date
            "2022-01-04T22:00:00.000Z",  # to_date
            "2022-01-01T22:00:00.000Z",  # from_date_hour_mp
            "2022-01-03T22:00:00.000Z",  # to_date_hour_mp
            "2022-01-03T22:00:00.000Z",  # from_date_quarter_mp
            "2022-01-04T22:00:00.000Z",  # to_date_quarter_mp
            144,  # total
            96,  # quarterly
            48,  # hourly
        ),
        (
            "2022-01-01T22:00:00.000Z",  # from_date
            "2022-01-05T22:00:00.000Z",  # to_date
            "2022-01-01T22:00:00.000Z",  # from_date_hour_mp
            "2022-01-03T22:00:00.000Z",  # to_date_hour_mp
            "2022-01-03T22:00:00.000Z",  # from_date_quarter_mp
            "2022-01-05T22:00:00.000Z",  # to_date_quarter_mp
            240,  # total
            192,  # quarterly
            48,  # hourly
        ),
    ],
)
def test__support_meteringpoint_period_switch_on_resolution_provides_correct_number_of_periods(
    raw_time_series_points_factory,
    metering_point_period_df_factory,
    timestamp_factory,
    from_date,
    to_date,
    from_date_hour_mp,
    to_date_hour_mp,
    from_date_quarter_mp,
    to_date_quarter_mp,
    total,
    quarterly,
    hourly,
):
    # Arrange
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory(from_date),
    )

    metering_point_period_df = metering_point_period_df_factory(
        resolution=MeteringPointResolution.HOUR.value,
        from_date=from_date_hour_mp,
        to_date=to_date_hour_mp,
    )

    second_metering_point_period_df = metering_point_period_df_factory(
        resolution=MeteringPointResolution.QUARTER.value,
        from_date=from_date_quarter_mp,
        to_date=to_date_quarter_mp,
    )

    # Act
    actual = get_enriched_time_series_points_df(
        raw_time_series_points,
        metering_point_period_df.union(second_metering_point_period_df),
        timestamp_factory(from_date),
        timestamp_factory(to_date),
    )

    hour = actual.filter(col("resolution") == MeteringPointResolution.HOUR.value)
    quarter = actual.filter(col("resolution") == MeteringPointResolution.QUARTER.value)

    # Assert
    assert actual.count() == total
    assert hour.count() == hourly
    assert quarter.count() == quarterly
