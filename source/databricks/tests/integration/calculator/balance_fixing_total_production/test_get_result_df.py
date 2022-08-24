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
from package.codelists import Resolution
from decimal import Decimal
from package import calculate_balance_fixing_total_production
from package.balance_fixing_total_production import _get_result_df
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, sum


@pytest.fixture
def enriched_time_series_quarterly_same_time_factory(spark, timestamp_factory):
    def factory(
        first_resolution=Resolution.quarter.value,
        second_resolution=Resolution.quarter.value,
        first_quantity=1,
        second_quantity=2,
        first_time="2022-06-08T12:09:15.000Z",
        second_time="2022-06-08T12:09:15.000Z",
        first_grid_area_code="805",
        second_grid_area_code="805",
    ):
        time = timestamp_factory(first_time)
        time2 = timestamp_factory(second_time)

        df = [
            {
                "GridAreaCode": first_grid_area_code,
                "Resolution": first_resolution,
                "GridAreaLinkId": "GridAreaLinkId",
                "time": time,
                "Quantity": first_quantity,
            },
            {
                "GridAreaCode": second_grid_area_code,
                "Resolution": second_resolution,
                "GridAreaLinkId": "GridAreaLinkId",
                "time": time2,
                "Quantity": second_quantity,
            },
        ]

        return spark.createDataFrame(df)

    return factory


@pytest.fixture
def enriched_time_series_factory(spark, timestamp_factory):
    def factory(
        resolution=Resolution.quarter.value, quantity=1, gridArea="805", amount=1
    ):
        time = timestamp_factory("2022-06-08T12:09:15.000Z")

        df = [
            {
                "GridAreaCode": gridArea,
                "Resolution": resolution,
                "GridAreaLinkId": "GridAreaLinkId",
                "time": time,
                "Quantity": quantity,
            }
        ] * amount

        return spark.createDataFrame(df)

    return factory


@pytest.fixture
def large_time_series_data_frame_factory(spark, timestamp_factory):
    def factory(
        resolution=Resolution.quarter.value, gridArea="805", quantity=1, amount=1
    ):
        time = timestamp_factory("2022-06-08T12:09:15.000Z")

        df = [
            {
                "GridAreaCode": gridArea,
                "GsrnNumber": "2045555014",
                "Resolution": resolution,
                "GridAreaLinkId": "GridAreaLinkId",
                "time": time,
                "Quantity": quantity,
                "Quality": 4,
            }
        ] * amount

        base_df = spark.createDataFrame(df)
        large_df = base_df

        for x in range(14):
            large_df = large_df.union(base_df)

        return large_df

    return factory


# Test sums with only quarterly can be calculated
def test__quarterly_sums_correctly(
    enriched_time_series_quarterly_same_time_factory,
):
    """Test that checks quantity is summed correctly with only quarterly times"""
    df = enriched_time_series_quarterly_same_time_factory(
        first_quantity=1, second_quantity=2
    )
    result_df = _get_result_df(df, [805])
    assert result_df.first()["Quantity"] == 3


def test__hourly_sums_are_rounded_correctly(
    enriched_time_series_factory,
):
    """Test that checks acceptable rounding erros for hourly quantities summed on a quarterly basis"""
    df = enriched_time_series_factory(Resolution.hour.value, 0.003)
    result_df = _get_result_df(df, [805])
    points = result_df.collect()

    assert len(points) == 4  # one hourly quantity should yield 4 points

    for x in points:
        assert x["Quantity"] == Decimal("0.001")


def test__quarterly_and_hourly_sums_correctly(
    enriched_time_series_quarterly_same_time_factory,
):
    """Test that checks quantity is summed correctly with quarterly and hourly times"""
    df = enriched_time_series_quarterly_same_time_factory(
        first_resolution=Resolution.quarter.value,
        first_quantity=2,
        second_resolution=Resolution.hour.value,
        second_quantity=2,
    )
    result_df = _get_result_df(df, [805])
    sum_quant = result_df.agg(sum("Quantity").alias("sum_quant"))
    assert sum_quant.first().sum_quant == 4  # total Quantity is 4



def test__points_with_same_time_quantities_are_on_same_position(
    enriched_time_series_quarterly_same_time_factory,
):
    """Test that points with the same 'time' have added their 'Quantity's together on the same position"""
    df = enriched_time_series_quarterly_same_time_factory(
        first_resolution=Resolution.quarter.value,
        first_quantity=2,
        second_resolution=Resolution.hour.value,
        second_quantity=2,
    )
    result_df = _get_result_df(df, [805])
    # total 'Quantity' on first position
    assert result_df.first().Quantity == 2.5
    # first point with quarter resolution 'quantity' is 2, second is 2 but is hourly so 0.5 should be added to first position


def test__hourly_sums_are_rounded_correctly_to_zero(
    enriched_time_series_factory,
):
    """Test with 0.001 which should be 0.000 in result for hourly resolution"""
    df = enriched_time_series_factory(Resolution.hour.value, 0.001)
    result_df = _get_result_df(df, [805])
    points = result_df.collect()

    assert len(points) == 4  # one hourly quantity should yield 4 points

    for point in points:
        assert point.Quantity == Decimal("0.000")



def test__final_sum_below_midpoint_is_rounded_down(
    enriched_time_series_factory,
):
    """Test that ensures rounding is done correctly for sums below midpoint"""
    df = enriched_time_series_factory(Resolution.hour.value, 0.001)
    result_df = _get_result_df(df, [805])
    points = result_df.collect()

    assert len(points) == 4  # one hourly quantity should yield 4 points

    for point in points:
        assert point.Quantity == Decimal("0.000")



def test__final_sum_at_midpoint_is_rounded_up(
    enriched_time_series_factory,
):
    """Test that ensures rounding is done correctly for sums at midpoint"""
    df = enriched_time_series_factory(Resolution.hour.value, 0.001).union(
        enriched_time_series_factory(Resolution.hour.value, 0.001)
    )
    result_df = _get_result_df(df, [805])
    points = result_df.collect()

    assert len(points) == 4  # one hourly quantity should yield 4 points

    for point in points:
        assert point.Quantity == Decimal("0.001")



def test__final_sum_past_midpoint_is_rounded_up(
    enriched_time_series_factory,
):
    """Test that ensures rounding is done correctly for sums past midpoint"""
    df = (
        enriched_time_series_factory(Resolution.hour.value, 0.001)
        .union(enriched_time_series_factory(Resolution.hour.value, 0.001))
        .union(enriched_time_series_factory(Resolution.hour.value, 0.001))
    )
    result_df = _get_result_df(df, [805])
    points = result_df.collect()

    assert len(points) == 4  # one hourly quantity should yield 4 points

    for point in points:
        assert point.Quantity == Decimal("0.001")



def test__position_is_based_on_time_correctly(
    enriched_time_series_quarterly_same_time_factory,
):
    """'position' is correctly placed based on 'time'"""
    df = enriched_time_series_quarterly_same_time_factory(
        first_resolution=Resolution.quarter.value,
        first_quantity=1,
        second_resolution=Resolution.quarter.value,
        second_quantity=2,
        first_time="2022-06-08T12:09:15.000Z",
        second_time="2022-06-08T12:09:30.000Z",
    )
    result_df = _get_result_df(df, [805])
    points = result_df.collect()
    assert points[0]["position"] == 1
    assert points[0]["Quantity"] == 1
    assert points[1]["position"] == 2
    assert points[1]["Quantity"] == 2


def test__that_hourly_quantity_is_summed_as_quarterly(
    enriched_time_series_quarterly_same_time_factory,
):
    "Test that checks if hourly quantities are summed as quarterly"
    df = enriched_time_series_quarterly_same_time_factory(
        first_resolution=Resolution.hour.value,
        first_quantity=4,
        second_resolution=Resolution.hour.value,
        second_quantity=8,
        first_time="2022-06-08T12:09:15.000Z",
        second_time="2022-06-08T13:09:15.000Z",
    )
    result_df = _get_result_df(df, [805])
    result_df.show()
    assert result_df.count() == 8
    actual = result_df.collect()
    assert actual[0].Quantity == 1
    assert actual[4].Quantity == 2


def test__Quality_is_present_and_None(
    enriched_time_series_factory,
):
    """Test that ensures 'Quality' is set, and the value is None"""
    df = enriched_time_series_factory()
    result_df = _get_result_df(df, [805])
    points = result_df.collect()

    for x in points:
        assert x["Quality"] is None


def test__filter_time_series_by_given_grid_area(
    enriched_time_series_factory,
):
    """Test that time series are correctly filtered using grid area code"""
    df = (
        enriched_time_series_factory(Resolution.hour.value, quantity=3)
        .union(
            enriched_time_series_factory(
                Resolution.hour.value, quantity=4, gridArea="100"
            )
        )
        .union(enriched_time_series_factory(Resolution.hour.value, quantity=3))
    )
    result_df = _get_result_df(df, [805])
    points = result_df.collect()

    assert len(points) == 4  # one hourly quantity should yield 4 points

    for x in points:
        assert x["Quantity"] == Decimal("1.5")


def test__that_grid_area_code_in_input_is_in_output(
    enriched_time_series_quarterly_same_time_factory,
):
    "Test that the grid area codes in input are in result"
    grid_area_code = 805
    df = enriched_time_series_quarterly_same_time_factory()
    result_df = _get_result_df(df, [grid_area_code])
    assert result_df.first().GridAreaCode == str(grid_area_code)


def test__each_grid_area_has_a_sum(
    enriched_time_series_quarterly_same_time_factory,
):
    """Test that multiple GridAreas receive each their calculation for a period"""
    df = enriched_time_series_quarterly_same_time_factory(second_grid_area_code="806")
    result_df = _get_result_df(df, [805, 806])
    assert result_df.count() == 2
    assert result_df.where("GridAreaCode == 805").count() == 1
    assert result_df.where("GridAreaCode == 806").count() == 1


def test__final_sum_of_small_values_should_not_lose_precision(
    large_time_series_data_frame_factory,
):
    """Test that checks many small values accumulated does not lose precision"""
    df = large_time_series_data_frame_factory(
        Resolution.hour.value, quantity=0.001, amount=80000
    )
    result_df = _get_result_df(df, [805])
    points = result_df.collect()

    assert len(points) == 4  # one hourly quantity should yield 4 points

    for x in points:
        assert x["Quantity"] == Decimal("300")
