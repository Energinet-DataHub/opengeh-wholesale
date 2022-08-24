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
def enriched_time_series_quaterly_same_time_factory(spark, timestamp_factory):
    def factory(
        first_resolution=Resolution.quarter.value,
        second_resolution=Resolution.quarter.value,
        first_quantity=1,
        second_quantity=2,
        first_time="2022-06-08T12:09:15.000Z",
        second_time="2022-06-08T12:09:15.000Z",
    ):
        time = timestamp_factory(first_time)
        time2 = timestamp_factory(second_time)

        df = [
            {
                "GridAreaCode": "805",
                "GsrnNumber": "2045555014",
                "Resolution": first_resolution,
                "GridAreaLinkId": "GridAreaLinkId",
                "time": time,
                "Quantity": first_quantity,
                "Quality": 4,
            },
            {
                "GridAreaCode": "805",
                "GsrnNumber": "2045555014",
                "Resolution": second_resolution,
                "GridAreaLinkId": "GridAreaLinkId",
                "time": time2,
                "Quantity": second_quantity,
                "Quality": 4,
            },
        ]

        return spark.createDataFrame(df)

    return factory


@pytest.fixture
def enriched_time_serie_factory(spark, timestamp_factory):
    def factory(resolution=Resolution.quarter.value, quantity=1):
        time = timestamp_factory("2022-06-08T12:09:15.000Z")

        df = [
            {
                "GridAreaCode": "805",
                "GsrnNumber": "2045555014",
                "Resolution": resolution,
                "GridAreaLinkId": "GridAreaLinkId",
                "time": time,
                "Quantity": quantity,
                "Quality": 4,
            }
        ]

        return spark.createDataFrame(df)

    return factory


# Test sums with only quaterly can be calculated
def test__quaterly_sums_correctly(
    enriched_time_series_quaterly_same_time_factory,
):
    """Test that checks quantity is summed correctly with only quaterly times"""
    df = enriched_time_series_quaterly_same_time_factory(
        first_quantity=1, second_quantity=2
    )
    result_df = _get_result_df(df, [805])
    assert result_df.first()["Quantity"] == 3


def test__hourly_sums_are_rounded_correctly(
    enriched_time_serie_factory,
):
    """Test that checks accetable rounding erros for hourly quantitys summed on a quaterly basis"""
    df = enriched_time_serie_factory(Resolution.hour.value, 0.003)
    result_df = _get_result_df(df, [805])
    points = result_df.collect()

    assert len(points) == 4  # one hourly quantity should yield 4 points

    for x in points:
        assert x["Quantity"] == Decimal("0.001")


# Test sums with both hourly and quarterly can be calculated
def test__quaterly_and_hourly_sums_correctly(
    enriched_time_series_quaterly_same_time_factory,
):
    """Test that checks quantity is summed correctly with quaterly and hourly times"""
    df = enriched_time_series_quaterly_same_time_factory(
        first_resolution=Resolution.quarter.value,
        first_quantity=2,
        second_resolution=Resolution.hour.value,
        second_quantity=2,
    )
    result_df = _get_result_df(df, [805])
    sum_quant = result_df.agg(sum("Quantity").alias("sum_quant"))
    assert sum_quant.collect()[0]["sum_quant"] == 4  # total Quantity is 4


# Test that points with the same 'time' have added their 'Quantity's together on the same position
def test__points_with_same_time_quantitys_are_on_same_poistion(
    enriched_time_series_quaterly_same_time_factory,
):
    """Test that checks quantity is summed correctly with quaterly and hourly times"""
    df = enriched_time_series_quaterly_same_time_factory(
        first_resolution=Resolution.quarter.value,
        first_quantity=2,
        second_resolution=Resolution.hour.value,
        second_quantity=2,
    )
    result_df = _get_result_df(df, [805])
    assert (
        # total 'Quantity' on first position
        result_df.first().Quantity
        == 2.5  # first point with quater resolution 'quantity' is 2, second is 2 but is hourly so 0.5 shoul be added to first position
    )


# Test with 0.001 which should be 0.000 in result for hourly resolution
def test__hourly_sums_are_rounded_correctly_to_zero(
    enriched_time_serie_factory,
):
    """Test that checks accetable rounding erros for hourly quantitys summed on a quaterly basis"""
    df = enriched_time_serie_factory(Resolution.hour.value, 0.001)
    result_df = _get_result_df(df, [805])
    points = result_df.collect()

    assert len(points) == 4  # one hourly quantity should yield 4 points

    for x in points:
        assert x["Quantity"] == Decimal("0.000")


def test__final_sum_below_midpoint_is_rounded_down(
    enriched_time_serie_factory,
):
    """Test that ensures rounding is done correctly for sums below midpoint"""
    df = enriched_time_serie_factory(Resolution.hour.value, 0.001)
    result_df = _get_result_df(df, [805])
    points = result_df.collect()

    assert len(points) == 4  # one hourly quantity should yield 4 points

    for x in points:
        assert x["Quantity"] == Decimal("0.000")


# TODO: Do we want up or down here?
def test__final_sum_at_midpoint_is_rounded_up(
    enriched_time_serie_factory,
):
    """Test that ensures rounding is done correctly for sums at midpoint"""
    df = enriched_time_serie_factory(Resolution.hour.value, 0.001).union(
        enriched_time_serie_factory(Resolution.hour.value, 0.001)
    )
    result_df = _get_result_df(df, [805])
    points = result_df.collect()

    assert len(points) == 4  # one hourly quantity should yield 4 points

    for x in points:
        assert x["Quantity"] == Decimal("0.001")


def test__final_sum_past_midpoint_is_rounded_up(
    enriched_time_serie_factory,
):
    """Test that ensures rounding is done correctly for sums past midpoint"""
    df = (
        enriched_time_serie_factory(Resolution.hour.value, 0.001)
        .union(enriched_time_serie_factory(Resolution.hour.value, 0.001))
        .union(enriched_time_serie_factory(Resolution.hour.value, 0.001))
    )
    result_df = _get_result_df(df, [805])
    points = result_df.collect()

    assert len(points) == 4  # one hourly quantity should yield 4 points

    for x in points:
        assert x["Quantity"] == Decimal("0.001")


# Test that position works correctly LRN
def test__position_is_based_on_time_correctly(
    enriched_time_series_quaterly_same_time_factory,
):
    """Test that checks quantity is summed correctly with quaterly and hourly times"""
    df = enriched_time_series_quaterly_same_time_factory(
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
    assert points[1]["position"] == 2


# Test that Quality is set and is None
# Test smallest Quantity supports that rounding up and
# Test that hourly Quantity is summed as quarterly? [johevemi]
# Test that GridAreaCode is in input is in output
# Test that only series from the GridArea is used to sum with
# Test that multiple GridAreas receive each their calculation for a period
# Test that limits work all the way: sum 1 mill rows of 0.001 hourly [AMI]
# Should we crash/stop if resolution is neither hour nor quarter?
