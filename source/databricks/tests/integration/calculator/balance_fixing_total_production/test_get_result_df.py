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
from package.codelists import Resolution, TimeSeriesQuality, Quality
from decimal import Decimal
from package import calculate_balance_fixing_total_production
from package.balance_fixing_total_production import _get_result_df
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, sum

minimum_quantity = Decimal("0.001")
grid_area_code_805 = "805"
grid_area_code_806 = "806"


@pytest.fixture
def enriched_time_series_quarterly_same_time_factory(spark, timestamp_factory):
    def factory(
        first_resolution=Resolution.quarter.value,
        second_resolution=Resolution.quarter.value,
        first_quantity=Decimal("1"),
        second_quantity=Decimal("2"),
        first_time="2022-06-08T12:09:15.000Z",
        second_time="2022-06-08T12:09:15.000Z",
        first_grid_area_code=grid_area_code_805,
        second_grid_area_code=grid_area_code_805,
    ):
        time = timestamp_factory(first_time)
        time2 = timestamp_factory(second_time)

        df = [
            {
                "GridAreaCode": first_grid_area_code,
                "Resolution": first_resolution,
                "time": time,
                "Quantity": first_quantity,
                "Quality": TimeSeriesQuality.measured.value,
            },
            {
                "GridAreaCode": second_grid_area_code,
                "Resolution": second_resolution,
                "time": time2,
                "Quantity": second_quantity,
                "Quality": TimeSeriesQuality.measured.value,
            },
        ]

        return spark.createDataFrame(df)

    return factory


@pytest.fixture
def enriched_time_series_factory(spark, timestamp_factory):
    def factory(
        resolution=Resolution.quarter.value,
        quantity=Decimal("1"),
        quality=TimeSeriesQuality.measured.value,
        gridArea="805",
    ):
        time = timestamp_factory("2022-06-08T12:09:15.000Z")

        df = [
            {
                "GridAreaCode": gridArea,
                "Resolution": resolution,
                "GridAreaLinkId": "GridAreaLinkId",
                "time": time,
                "Quantity": quantity,
                "Quality": quality,
            }
        ]
        return spark.createDataFrame(df)

    return factory


# Test sums with only quarterly can be calculated
def test__quarterly_sums_correctly(
    enriched_time_series_quarterly_same_time_factory,
):
    """Test that checks quantity is summed correctly with only quarterly times"""
    df = enriched_time_series_quarterly_same_time_factory(
        first_quantity=Decimal("1"), second_quantity=Decimal("2")
    )
    result_df = _get_result_df(df)
    assert result_df.first().Quantity == 3


@pytest.mark.parametrize(
    "quantity, expected_point_quantity",
    [
        # 0.001 / 4 = 0.000250 ≈ 0.000
        (Decimal("0.001"), Decimal("0.000")),
        # 0.002 / 4 = 0.000500 ≈ 0.001
        (Decimal("0.002"), Decimal("0.001")),
        # 0.003 / 4 = 0.000750 ≈ 0.001
        (Decimal("0.003"), Decimal("0.001")),
        # 0.004 / 4 = 0.001000 ≈ 0.001
        (Decimal("0.004"), Decimal("0.001")),
        # 0.005 / 4 = 0.001250 ≈ 0.001
        (Decimal("0.005"), Decimal("0.001")),
        # 0.006 / 4 = 0.001500 ≈ 0.002
        (Decimal("0.006"), Decimal("0.002")),
        # 0.007 / 4 = 0.001750 ≈ 0.002
        (Decimal("0.007"), Decimal("0.002")),
        # 0.008 / 4 = 0.002000 ≈ 0.002
        (Decimal("0.008"), Decimal("0.002")),
    ],
)
def test__hourly_sums_are_rounded_correctly(
    enriched_time_series_factory, quantity, expected_point_quantity
):
    """Test that checks acceptable rounding erros for hourly quantities summed on a quarterly basis"""
    df = enriched_time_series_factory(
        resolution=Resolution.hour.value, quantity=quantity
    )

    result_df = _get_result_df(df)

    assert result_df.count() == 4  # one hourly quantity should yield 4 points
    assert result_df.where(col("Quantity") == expected_point_quantity).count() == 4


def test__quarterly_and_hourly_sums_correctly(
    enriched_time_series_quarterly_same_time_factory,
):
    """Test that checks quantity is summed correctly with quarterly and hourly times"""
    first_quantity = Decimal("2")
    second_quantity = Decimal("2")
    df = enriched_time_series_quarterly_same_time_factory(
        first_resolution=Resolution.quarter.value,
        first_quantity=first_quantity,
        second_resolution=Resolution.hour.value,
        second_quantity=second_quantity,
    )
    result_df = _get_result_df(df)
    sum_quant = result_df.agg(sum("Quantity").alias("sum_quant"))
    assert sum_quant.first()["sum_quant"] == first_quantity + second_quantity


def test__points_with_same_time_quantities_are_on_same_position(
    enriched_time_series_quarterly_same_time_factory,
):
    """Test that points with the same 'time' have added their 'Quantity's together on the same position"""
    df = enriched_time_series_quarterly_same_time_factory(
        first_resolution=Resolution.quarter.value,
        first_quantity=Decimal("2"),
        second_resolution=Resolution.hour.value,
        second_quantity=Decimal("2"),
    )
    result_df = _get_result_df(df)
    # total 'Quantity' on first position
    assert result_df.first().Quantity == Decimal("2.5")
    # first point with quarter resolution 'quantity' is 2, second is 2 but is hourly so 0.5 should be added to first position


def test__position_is_based_on_time_correctly(
    enriched_time_series_quarterly_same_time_factory,
):
    """'position' is correctly placed based on 'time'"""
    df = enriched_time_series_quarterly_same_time_factory(
        first_resolution=Resolution.quarter.value,
        first_quantity=Decimal("1"),
        second_resolution=Resolution.quarter.value,
        second_quantity=Decimal("2"),
        first_time="2022-06-08T12:09:15.000Z",
        second_time="2022-06-08T12:09:30.000Z",
        first_grid_area_code=grid_area_code_805,
        second_grid_area_code=grid_area_code_805,
    )
    result_df = _get_result_df(df).collect()

    assert result_df[0]["position"] == 1
    assert result_df[0]["Quantity"] == Decimal("1")
    assert result_df[1]["position"] == 2
    assert result_df[1]["Quantity"] == Decimal("2")


def test__that_hourly_quantity_is_summed_as_quarterly(
    enriched_time_series_quarterly_same_time_factory,
):
    "Test that checks if hourly quantities are summed as quarterly"
    df = enriched_time_series_quarterly_same_time_factory(
        first_resolution=Resolution.hour.value,
        first_quantity=Decimal("4"),
        second_resolution=Resolution.hour.value,
        second_quantity=Decimal("8"),
        first_time="2022-06-08T12:09:15.000Z",
        second_time="2022-06-08T13:09:15.000Z",
    )
    result_df = _get_result_df(df)
    assert result_df.count() == 8
    actual = result_df.collect()
    assert actual[0].Quantity == Decimal("1")
    assert actual[4].Quantity == Decimal("2")


def test__that_grid_area_code_in_input_is_in_output(
    enriched_time_series_quarterly_same_time_factory,
):
    "Test that the grid area codes in input are in result"
    df = enriched_time_series_quarterly_same_time_factory()
    result_df = _get_result_df(df)
    assert result_df.first().GridAreaCode == str(grid_area_code_805)


def test__each_grid_area_has_a_sum(
    enriched_time_series_quarterly_same_time_factory,
):
    """Test that multiple GridAreas receive each their calculation for a period"""
    df = enriched_time_series_quarterly_same_time_factory(second_grid_area_code="806")
    result_df = _get_result_df(df)
    assert result_df.count() == 2
    assert result_df.where("GridAreaCode == 805").count() == 1
    assert result_df.where("GridAreaCode == 806").count() == 1


def test__final_sum_of_different_magnitudes_should_not_lose_precision(
    enriched_time_series_factory,
):
    """Test that values with different magnitudes do not lose precision when accumulated"""
    df = (
        enriched_time_series_factory(Resolution.hour.value, Decimal("400000000000"))
        .union(enriched_time_series_factory(Resolution.hour.value, minimum_quantity))
        .union(enriched_time_series_factory(Resolution.hour.value, minimum_quantity))
        .union(enriched_time_series_factory(Resolution.hour.value, minimum_quantity))
    )
    result_df = _get_result_df(df)

    assert result_df.count() == 4
    assert result_df.where(col("Quantity") == "100000000000.001").count() == 4


@pytest.mark.parametrize(
    "quality_1, quality_2, quality_3, expected_quality",
    [
        (
            TimeSeriesQuality.measured.value,
            TimeSeriesQuality.estimated.value,
            TimeSeriesQuality.missing.value,
            Quality.incomplete.value,
        ),
        (
            TimeSeriesQuality.measured.value,
            TimeSeriesQuality.estimated.value,
            TimeSeriesQuality.measured.value,
            Quality.estimated.value,
        ),
        (
            TimeSeriesQuality.measured.value,
            TimeSeriesQuality.measured.value,
            TimeSeriesQuality.measured.value,
            Quality.measured.value,
        ),
    ],
)
def test__quality_is_lowest_common_denominator_among_measured_estimated_and_missing(
    enriched_time_series_factory, quality_1, quality_2, quality_3, expected_quality
):
    df = (
        enriched_time_series_factory(quality=quality_1)
        .union(enriched_time_series_factory(quality=quality_2))
        .union(enriched_time_series_factory(quality=quality_3))
    )
    result_df = _get_result_df(df)
    assert result_df.first().quality == expected_quality
