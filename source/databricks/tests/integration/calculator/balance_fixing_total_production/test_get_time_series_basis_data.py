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

from datetime import datetime, timedelta, tzinfo, date
from pytz import timezone
import pytz
import os
import shutil
import pytest
import json
from package.codelists import Resolution, MeteringPointType
from decimal import Decimal
from package import calculate_balance_fixing_total_production
from package.balance_fixing_total_production import (
    _get_time_series_basis_data,
    _get_enriched_time_series_points_df,
)
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, sum, lit
from functools import reduce
from operator import add

minimum_quantity = Decimal("0.001")
grid_area_code_805 = "805"
grid_area_code_806 = "806"


@pytest.fixture
def enriched_time_series_factory(spark, timestamp_factory):
    def factory(
        resolution=Resolution.quarter.value,
        quantity=Decimal("1"),
        grid_area="805",
        gsrn_number="the_gsrn_number",
        metering_point_type=MeteringPointType.production.value,
        time="2022-06-08T22:00:00.000Z",
        number_of_points=1,
    ):
        df_array = []

        time = timestamp_factory(time)

        for i in range(number_of_points):

            df_array.append(
                {
                    "GridAreaCode": grid_area,
                    "Resolution": resolution,
                    "GridAreaLinkId": "GridAreaLinkId",
                    "time": time,
                    "Quantity": quantity + i,
                    "GsrnNumber": gsrn_number,
                    "MeteringPointType": metering_point_type,
                }
            )
            time = (
                time + timedelta(minutes=60)
                if resolution == Resolution.hour.value
                else time + timedelta(minutes=15)
            )
        return spark.createDataFrame(df_array)

    return factory


@pytest.mark.parametrize(
    "period_start, resolution, number_of_points, expected_number_of_quarter_quantity_collumns, expected_number_of_hour_quantity_collumns",
    [
        # DST has 24 hours
        ("2022-06-08T22:00:00.000Z", Resolution.quarter.value, 96, 96, 0),
        # DST has 24 hours
        ("2022-06-08T22:00:00.000Z", Resolution.hour.value, 24, 0, 24),
        # standard time has 24 hours
        ("2022-06-08T22:00:00.000Z", Resolution.quarter.value, 96, 96, 0),
        # standard time has 24 hours
        ("2022-06-08T22:00:00.000Z", Resolution.hour.value, 24, 0, 24),
        # going from DST to standard time there are 25 hours (100 quarters)
        # creating 292 points from 22:00 the 29 oktober will create points for 3 days
        # where the 30 oktober is day with 25 hours.and
        # Therefore there should be 100 columns for quarter resolution and 25 for  hour resolution
        ("2022-10-29T22:00:00.000Z", Resolution.quarter.value, 292, 100, 0),
        ("2022-10-29T22:00:00.000Z", Resolution.hour.value, 73, 0, 25),
        # going from vinter to summertime there are 23 hours (92 quarters)
        ("2022-03-26T23:00:00.000Z", Resolution.hour.value, 23, 0, 23),
    ],
)
def test__has_correct_number_of_quantity_columns_according_to_dst(
    enriched_time_series_factory,
    period_start,
    resolution,
    number_of_points,
    expected_number_of_quarter_quantity_columns,
    expected_number_of_hour_quantity_columns,
):
    enriched_time_series_points_df = enriched_time_series_factory(
        time=period_start,
        resolution=resolution,
        number_of_points=number_of_points,
    )
    (quarter_df, hour_df) = _get_time_series_basis_data(
        enriched_time_series_points_df, "Europe/Copenhagen"
    )

    quantity_columns_quarter = list(
        filter(lambda column: column.startswith("ENERGYQUANTITY"), quarter_df.columns)
    )
    quantity_columns_hour = list(
        filter(lambda column: column.startswith("ENERGYQUANTITY"), hour_df.columns)
    )
    assert len(quantity_columns_quarter) == expected_number_of_quarter_quantity_columns
    assert len(quantity_columns_hour) == expected_number_of_hour_quantity_columns


def test__returns_dataframe_with_quarter_resolution_metering_points(
    enriched_time_series_factory,
):
    enriched_time_series_points_df = enriched_time_series_factory(
        time="2022-10-28T22:00:00.000Z",
        resolution=Resolution.quarter.value,
        number_of_points=96,
    )
    (quarter_df, hour_df) = _get_time_series_basis_data(
        enriched_time_series_points_df, "Europe/Copenhagen"
    )
    assert quarter_df.count() == 1
    assert hour_df.count() == 0


def test__returns_dataframe_with_hour_resolution_metering_points(
    enriched_time_series_factory,
):
    enriched_time_series_points_df = enriched_time_series_factory(
        time="2022-10-28T22:00:00.000Z",
        resolution=Resolution.hour.value,
        number_of_points=24,
    )
    (quarter_df, hour_df) = _get_time_series_basis_data(
        enriched_time_series_points_df, "Europe/Copenhagen"
    )
    assert quarter_df.count() == 0
    assert hour_df.count() == 1


def test__splits_single_metering_point_with_different_resolution_on_different_dates(
    enriched_time_series_factory,
):
    enriched_time_series_points_df = enriched_time_series_factory(
        gsrn_number="the_gsrn_number",
        time="2022-10-28T22:00:00.000Z",
        resolution=Resolution.quarter.value,
        number_of_points=96,
    ).union(
        enriched_time_series_factory(
            gsrn_number="the_gsrn_number",
            time="2022-10-29T22:00:00.000Z",
            resolution=Resolution.hour.value,
            number_of_points=24,
        )
    )
    (quarter_df, hour_df) = _get_time_series_basis_data(
        enriched_time_series_points_df, "Europe/Copenhagen"
    )
    assert quarter_df.count() == 1
    assert hour_df.count() == 1


def test__returns_expected_data_for_each_column(enriched_time_series_factory):
    # Test for both hour and quarter dataframes
    enriched_time_series_points_df = enriched_time_series_factory(
        time="2022-10-28T22:00:00.000Z",
        resolution=Resolution.quarter.value,
        number_of_points=96,
    ).union(
        enriched_time_series_factory(
            time="2022-10-28T22:00:00.000Z",
            resolution=Resolution.hour.value,
            number_of_points=24,
        )
    )
    (quarter_df, hour_df) = _get_time_series_basis_data(
        enriched_time_series_points_df, "Europe/Copenhagen"
    )

    quantity_columns_quarter = list(
        filter(lambda column: column.startswith("ENERGYQUANTITY"), quarter_df.columns)
    )
    # get sum of all quantity columns for quarter resolution
    sum_quantity_quarter = int(
        quarter_df.withColumn(
            "sum", reduce(add, [col(x) for x in quantity_columns_quarter])
        )
        .select("sum")
        .first()[0]
    )

    quantity_columns_hour = list(
        filter(lambda column: column.startswith("ENERGYQUANTITY"), hour_df.columns)
    )
    # get sum of all quantity columns for hour resolutin
    sum_quantity_hour = int(
        hour_df.withColumn("sum", reduce(add, [col(x) for x in quantity_columns_hour]))
        .select("sum")
        .first()[0]
    )

    assert sum_quantity_quarter == 4656
    assert sum_quantity_hour == 300
    assert quarter_df.select("ENERGYQUANTITY1").first()[0] == 1
    assert quarter_df.select("ENERGYQUANTITY10").first()[0] == 10
    assert hour_df.select("ENERGYQUANTITY1").first()[0] == 1
    assert hour_df.select("ENERGYQUANTITY10").first()[0] == 10


def test__multiple_dates_are_split_into_rows(enriched_time_series_factory):
    enriched_time_series_points_df = enriched_time_series_factory(
        time="2022-10-18T22:00:00.000Z",
        resolution=Resolution.quarter.value,
        number_of_points=288,  # 3 days
    ).union(
        enriched_time_series_factory(
            time="2022-10-28T22:00:00.000Z",
            resolution=Resolution.hour.value,
            number_of_points=120,  # five days
        )
    )

    (quarter_df, hour_df) = _get_time_series_basis_data(
        enriched_time_series_points_df, "Europe/Copenhagen"
    )

    assert quarter_df.count() == 3
    assert hour_df.count() == 5
