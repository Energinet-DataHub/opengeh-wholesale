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

from pyspark.sql import SparkSession
from pyspark.sql.types import Row

from package.calculation.energy.hour_to_quarter import metering_point_time_series_schema
from package.calculation.preparation.transformations import (
    get_metering_point_time_series,
)
from package.constants import Colname

from package.codelists import (
    MeteringPointResolution,
    QuantityQuality,
)
from pyspark.sql.functions import col
from package.calculation_input.schemas import (
    time_series_point_schema,
    metering_point_period_schema,
)


DEFAULT_METERING_POINT_ID = "the-metering-point-id"


@pytest.fixture(scope="module")
def raw_time_series_points_factory(spark, timestamp_factory):
    def factory(
        time: datetime = timestamp_factory("2022-06-08T12:15:00.000Z"),
    ):
        row = {
            Colname.metering_point_id: DEFAULT_METERING_POINT_ID,
            Colname.quantity: Decimal("1.1"),
            Colname.quality: QuantityQuality.CALCULATED.value,
            Colname.observation_time: time,
        }
        rows = [Row(**row)]
        return spark.createDataFrame(rows, time_series_point_schema)

    return factory


@pytest.fixture(scope="module")
def metering_point_period_df_factory(spark, timestamp_factory):
    def factory(
        resolution,
        from_date: datetime = timestamp_factory("2022-01-01T22:00:00.000Z"),
        to_date: datetime = timestamp_factory("2022-12-22T22:00:00.000Z"),
    ):
        row = {
            Colname.metering_point_id: DEFAULT_METERING_POINT_ID,
            Colname.metering_point_type: "the_metering_point_type",
            Colname.calculation_type: "calculation-type",
            Colname.settlement_method: "D01",
            Colname.grid_area: "805",
            Colname.resolution: resolution,
            Colname.from_grid_area: "",
            Colname.to_grid_area: "",
            Colname.parent_metering_point_id: "parent-metering-point-id",
            Colname.energy_supplier_id: "someId",
            Colname.balance_responsible_id: "someId",
            Colname.from_date: from_date,
            Colname.to_date: to_date,
        }
        rows = [Row(**row)]
        return spark.createDataFrame(rows, metering_point_period_schema)

    return factory


point_1_quantity = Decimal("1.100")
point_2_quantity = Decimal("2.200")


time_1 = "2022-06-10T12:15:00.000Z"
time_2 = "2022-06-10T13:15:00.000Z"


def test__when_success__returns_dataframe_with_expected_schema(
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
        from_date=timestamp_factory(start_time),
        to_date=timestamp_factory(end_time),
    )

    # Act
    actual = get_metering_point_time_series(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(start_time),
        timestamp_factory(end_time),
    )

    # Assert
    assert actual.schema == metering_point_time_series_schema


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
    """Test the outcome of _get_metering_point_time_series with different scenarios.
    expected_rows is the number of rows in the output dataframe when given different parameters,
    FromDate and ToDate on the metering point"""

    # Arrange
    raw_time_series_points = raw_time_series_points_factory(
        time=timestamp_factory("2022-06-08T12:15:00.000Z")
    )
    metering_point_period_df = metering_point_period_df_factory(
        from_date=timestamp_factory(from_date),
        to_date=timestamp_factory(to_date),
        resolution=resolution,
    )

    # Act
    actual = get_metering_point_time_series(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(from_date),
        timestamp_factory(to_date),
    )

    # Assert
    assert actual.count() == expected_rows


def test__missing_point_has_quantity_0_for_quarterly_resolution(
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
        from_date=timestamp_factory(start_time),
        to_date=timestamp_factory(end_time),
    )
    # Act
    actual = get_metering_point_time_series(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(start_time),
        timestamp_factory(end_time),
    )

    # Assert
    # We remove the point we created before inspecting the remaining
    actual = actual.where(
        col(Colname.observation_time) != timestamp_factory(start_time)
    )
    assert actual.where(col(Colname.quantity) == 0).count() == 95


def test__missing_point_has_quantity_0_for_hourly_resolution(
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
        from_date=timestamp_factory(start_time),
        to_date=timestamp_factory(end_time),
    )

    # Act
    actual = get_metering_point_time_series(
        raw_time_series_points,
        metering_point_period_df,
        timestamp_factory(start_time),
        timestamp_factory(end_time),
    )

    # Assert
    # We remove the point we created before inspecting the remaining
    actual = actual.where(
        col(Colname.observation_time) != timestamp_factory(start_time)
    )
    assert actual.where(col(Colname.quantity) == 0).count() == 23


def test__df_is_not_empty_when_no_time_series_points(
    raw_time_series_points_factory, metering_point_period_df_factory, timestamp_factory
):
    # Arrange
    start_time = "2022-06-08T22:00:00.000Z"
    end_time = "2022-06-09T22:00:00.000Z"

    empty_raw_time_series_points = raw_time_series_points_factory().where(
        col(Colname.metering_point_id) == ""
    )
    metering_point_period_df = metering_point_period_df_factory(
        resolution=MeteringPointResolution.QUARTER.value,
        from_date=timestamp_factory(start_time),
        to_date=timestamp_factory(end_time),
    )

    # Act
    actual = get_metering_point_time_series(
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
        # where the 30 october is day with 25 hours.and
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
        # going from winter to summer there are 23 hours (92 quarters)
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
    ).where(col(Colname.metering_point_id) != DEFAULT_METERING_POINT_ID)

    metering_point_period_df = metering_point_period_df_factory(
        from_date=timestamp_factory(period_start),
        to_date=timestamp_factory(period_end),
        resolution=resolution,
    )

    # Act
    actual = get_metering_point_time_series(
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
def test__support_metering_point_period_switch_on_resolution_provides_correct_number_of_periods(
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
        from_date=timestamp_factory(from_date_hour_mp),
        to_date=timestamp_factory(to_date_hour_mp),
    )

    second_metering_point_period_df = metering_point_period_df_factory(
        resolution=MeteringPointResolution.QUARTER.value,
        from_date=timestamp_factory(from_date_quarter_mp),
        to_date=timestamp_factory(to_date_quarter_mp),
    )

    # Act
    actual = get_metering_point_time_series(
        raw_time_series_points,
        metering_point_period_df.union(second_metering_point_period_df),
        timestamp_factory(from_date),
        timestamp_factory(to_date),
    )

    hour = actual.filter(col(Colname.resolution) == MeteringPointResolution.HOUR.value)
    quarter = actual.filter(
        col(Colname.resolution) == MeteringPointResolution.QUARTER.value
    )

    # Assert
    assert actual.count() == total
    assert hour.count() == hourly
    assert quarter.count() == quarterly


@pytest.mark.parametrize(
    "resolution",
    [MeteringPointResolution.HOUR, MeteringPointResolution.QUARTER],
)
def test__when_time_series_point_is_missing__quality_has_value_incomplete(
    spark: SparkSession,
    raw_time_series_points_factory,
    metering_point_period_df_factory,
    timestamp_factory,
    resolution: MeteringPointResolution,
):
    # Arrange
    start_time = "2022-06-08T22:00:00.000Z"
    end_time = "2022-06-08T23:00:00.000Z"
    empty_time_series = spark.createDataFrame(data=[], schema=time_series_point_schema)

    metering_point_period_df = metering_point_period_df_factory(
        resolution=resolution.value,
        from_date=timestamp_factory(start_time),
        to_date=timestamp_factory(end_time),
    )

    # Act
    actual = get_metering_point_time_series(
        empty_time_series,
        metering_point_period_df,
        timestamp_factory(start_time),
        timestamp_factory(end_time),
    )

    # Assert
    actual_row = actual.collect()[0]
    assert actual_row[Colname.quality] == QuantityQuality.MISSING.value


@pytest.mark.parametrize(
    "resolution",
    [MeteringPointResolution.HOUR, MeteringPointResolution.QUARTER],
)
def test__when_time_series_point_is_missing__quantity_is_zero(
    spark: SparkSession,
    raw_time_series_points_factory,
    metering_point_period_df_factory,
    timestamp_factory,
    resolution: MeteringPointResolution,
):
    # Arrange
    start_time = "2022-06-08T22:00:00.000Z"
    end_time = "2022-06-08T23:00:00.000Z"
    empty_time_series = spark.createDataFrame(data=[], schema=time_series_point_schema)

    metering_point_period_df = metering_point_period_df_factory(
        resolution=resolution.value,
        from_date=timestamp_factory(start_time),
        to_date=timestamp_factory(end_time),
    )

    # Act
    actual = get_metering_point_time_series(
        empty_time_series,
        metering_point_period_df,
        timestamp_factory(start_time),
        timestamp_factory(end_time),
    )

    # Assert
    actual_row = actual.collect()[0]
    assert actual_row[Colname.quantity] == 0
