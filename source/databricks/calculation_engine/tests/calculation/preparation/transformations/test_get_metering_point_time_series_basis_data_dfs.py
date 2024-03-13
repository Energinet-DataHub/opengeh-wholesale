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

from datetime import timedelta
from decimal import Decimal

import pytest
from pyspark.sql.functions import lit
from pyspark.sql.types import (
    DecimalType,
)

import prepared_metering_point_time_series_factory as factories
from package.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from package.calculation.preparation.transformations.basis_data import (
    get_metering_point_time_series_basis_data_dfs,
)
from package.codelists import MeteringPointResolution

minimum_quantity = Decimal("0.001")


@pytest.fixture
def metering_point_time_series_factory(spark, timestamp_factory):
    def factory(
        resolution=MeteringPointResolution.QUARTER,
        time="2022-06-08T22:00:00.000Z",
        number_of_points=1,
    ) -> PreparedMeteringPointTimeSeries:
        rows = []
        time = timestamp_factory(time)
        quantity = Decimal("1")

        for i in range(number_of_points):
            rows.append(
                factories.create_row(
                    resolution=resolution,
                    observation_time=time,
                    quantity=quantity + i,
                )
            )
            time = (
                time + timedelta(minutes=60)
                if resolution == MeteringPointResolution.HOUR
                else time + timedelta(minutes=15)
            )
        return factories.create(spark, rows)

    return factory


@pytest.mark.parametrize(
    "period_start, resolution, number_of_points, expected_number_of_quarter_quantity_columns, expected_number_of_hour_quantity_columns",
    [
        # DST has 24 hours
        (
            "2022-06-08T22:00:00.000Z",
            MeteringPointResolution.QUARTER,
            96,
            96,
            0,
        ),
        # DST has 24 hours
        ("2022-06-08T22:00:00.000Z", MeteringPointResolution.HOUR, 24, 0, 24),
        # standard time has 24 hours
        (
            "2022-06-08T22:00:00.000Z",
            MeteringPointResolution.QUARTER,
            96,
            96,
            0,
        ),
        # standard time has 24 hours
        ("2022-06-08T22:00:00.000Z", MeteringPointResolution.HOUR, 24, 0, 24),
        # going from DST to standard time there are 25 hours (100 quarters)
        # creating 292 points from 22:00 the 29 oktober will create points for 3 days
        # where the 30 oktober is day with 25 hours.and
        # Therefore there should be 100 columns for quarter resolution and 25 for  hour resolution
        (
            "2022-10-29T22:00:00.000Z",
            MeteringPointResolution.QUARTER,
            292,
            100,
            0,
        ),
        ("2022-10-29T22:00:00.000Z", MeteringPointResolution.HOUR, 73, 0, 25),
        # going from winter to summertime there are 23 hours (92 quarters)
        ("2022-03-26T23:00:00.000Z", MeteringPointResolution.HOUR, 23, 0, 23),
    ],
)
def test__has_correct_number_of_quantity_columns_according_to_dst(
    metering_point_time_series_factory,
    period_start,
    resolution,
    number_of_points,
    expected_number_of_quarter_quantity_columns,
    expected_number_of_hour_quantity_columns,
):
    # Arrange
    metering_point_time_series = metering_point_time_series_factory(
        time=period_start,
        resolution=resolution,
        number_of_points=number_of_points,
    )

    # Act
    quarter_df, hour_df = get_metering_point_time_series_basis_data_dfs(
        metering_point_time_series, "Europe/Copenhagen"
    )

    # Assert
    quantity_columns_quarter = list(
        filter(lambda column: column.startswith("ENERGYQUANTITY"), quarter_df.columns)
    )
    quantity_columns_hour = list(
        filter(lambda column: column.startswith("ENERGYQUANTITY"), hour_df.columns)
    )
    assert len(quantity_columns_quarter) == expected_number_of_quarter_quantity_columns
    assert len(quantity_columns_hour) == expected_number_of_hour_quantity_columns


def test__returns_dataframe_with_quarter_resolution_metering_points(
    metering_point_time_series_factory,
):
    metering_point_time_series = metering_point_time_series_factory(
        time="2022-10-28T22:00:00.000Z",
        resolution=MeteringPointResolution.QUARTER,
        number_of_points=96,
    )
    (quarter_df, hour_df) = get_metering_point_time_series_basis_data_dfs(
        metering_point_time_series, "Europe/Copenhagen"
    )
    assert quarter_df.count() == 1
    assert hour_df.count() == 0


def test__returns_dataframe_with_hour_resolution_metering_points(
    metering_point_time_series_factory,
):
    metering_point_time_series = metering_point_time_series_factory(
        time="2022-10-28T22:00:00.000Z",
        resolution=MeteringPointResolution.HOUR,
        number_of_points=24,
    )
    (quarter_df, hour_df) = get_metering_point_time_series_basis_data_dfs(
        metering_point_time_series, "Europe/Copenhagen"
    )
    assert quarter_df.count() == 0
    assert hour_df.count() == 1


def test__splits_single_metering_point_with_different_resolution_on_different_dates(
    metering_point_time_series_factory,
):
    metering_point_time_series = metering_point_time_series_factory(
        time="2022-10-28T22:00:00.000Z",
        resolution=MeteringPointResolution.QUARTER,
        number_of_points=96,
    ).df.union(
        metering_point_time_series_factory(
            time="2022-10-29T22:00:00.000Z",
            resolution=MeteringPointResolution.HOUR,
            number_of_points=24,
        ).df
    )
    quarter_df, hour_df = get_metering_point_time_series_basis_data_dfs(
        PreparedMeteringPointTimeSeries(metering_point_time_series), "Europe/Copenhagen"
    )
    assert quarter_df.count() == 1
    assert hour_df.count() == 1


def test__returns_expected_quantity_for_each_hour_column(
    metering_point_time_series_factory,
) -> None:
    metering_point_time_series = metering_point_time_series_factory(
        time="2022-10-28T22:00:00.000Z",
        resolution=MeteringPointResolution.HOUR,
        number_of_points=24,
    )

    (_, hour_df) = get_metering_point_time_series_basis_data_dfs(
        metering_point_time_series, "Europe/Copenhagen"
    )

    for position in range(1, 25):
        expected_quantity = (
            position  # This value is corresponding to the one generated by the factory
        )
        assert hour_df.first()[f"ENERGYQUANTITY{position}"] == expected_quantity


def test__returns_expected_quantity_for_each_quarter_column(
    metering_point_time_series_factory,
):
    metering_point_time_series = metering_point_time_series_factory(
        time="2022-10-28T22:00:00.000Z",
        resolution=MeteringPointResolution.QUARTER,
        number_of_points=96,
    )

    (quarter_df, _) = get_metering_point_time_series_basis_data_dfs(
        metering_point_time_series, "Europe/Copenhagen"
    )

    for position in range(1, 97):
        expected_quantity = (
            position  # This value is corresponding to the one generated by the factory
        )
        assert quarter_df.first()[f"ENERGYQUANTITY{position}"] == expected_quantity


@pytest.mark.parametrize(
    "number_of_points,expected_number_of_rows",
    [
        (0, 0),
        (1, 1),
        (96, 1),
        (97, 2),
    ],
)
def test__multiple_dates_are_split_into_rows_for_quarterly_meteringpoints(
    metering_point_time_series_factory, number_of_points, expected_number_of_rows
):
    metering_point_time_series = metering_point_time_series_factory(
        time="2022-10-18T22:00:00.000Z",
        resolution=MeteringPointResolution.QUARTER,
        number_of_points=number_of_points,
    )

    (quarter_df, _) = get_metering_point_time_series_basis_data_dfs(
        metering_point_time_series, "Europe/Copenhagen"
    )

    assert quarter_df.count() == expected_number_of_rows


@pytest.mark.parametrize(
    "number_of_points,expected_number_of_rows",
    [
        (0, 0),
        (1, 1),
        (24, 1),
        (25, 2),
    ],
)
def test__multiple_dates_are_split_into_rows_for_hourly_meteringpoints(
    metering_point_time_series_factory, number_of_points, expected_number_of_rows
):
    metering_point_time_series = metering_point_time_series_factory(
        time="2022-10-18T22:00:00.000Z",
        resolution=MeteringPointResolution.HOUR,
        number_of_points=number_of_points,
    )

    _, hour_df = get_metering_point_time_series_basis_data_dfs(
        metering_point_time_series, "Europe/Copenhagen"
    )

    assert hour_df.count() == expected_number_of_rows


def test__missing_point_has_empty_quantity(
    metering_point_time_series_factory,
):
    metering_point_time_series = metering_point_time_series_factory(
        time="2022-10-28T22:00:00.000Z",
        resolution=MeteringPointResolution.QUARTER,
        number_of_points=96,
    ).df.withColumn("quantity", lit(None).cast(DecimalType()))
    quarter_df, _ = get_metering_point_time_series_basis_data_dfs(
        PreparedMeteringPointTimeSeries(metering_point_time_series), "Europe/Copenhagen"
    )

    for position in range(1, 97):
        assert quarter_df.first()[f"ENERGYQUANTITY{position}"] is None
