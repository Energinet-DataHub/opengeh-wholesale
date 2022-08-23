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
from pyspark.sql.functions import col


@pytest.fixture
def enriched_time_series_quaterly_same_time_factory(spark, timestamp_factory):
    def factory():
        time = timestamp_factory("2022-06-08T12:09:15.000Z")

        df = [
            {
                "GridAreaCode": "805",
                "GsrnNumber": "2045555014",
                "Resolution": Resolution.quarter,
                "GridAreaLinkId": "GridAreaLinkId",
                "time": time,
                "Quantity": 1,
                "Quality": 4,
            },
            {
                "GridAreaCode": "805",
                "GsrnNumber": "2045555014",
                "Resolution": Resolution.quarter,
                "GridAreaLinkId": "GridAreaLinkId",
                "time": time,
                "Quantity": 2,
                "Quality": 4,
            },
        ]

        return spark.createDataFrame(df)

    return factory


@pytest.fixture
def enriched_time_series_with_same_time_factory(spark, timestamp_factory):
    def factory(resolution=Resolution.quarter, quantity=1):
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
    df = enriched_time_series_quaterly_same_time_factory()
    result_df = _get_result_df(df, [805])
    assert result_df.first()["Quantity"] == 3


def test__hourly_sums_are_rounded_correctly(
    enriched_time_series_with_same_time_factory,
):
    """Test that checks accetable rounding erros for hourly quantitys summed on a quaterly basis"""
    df = enriched_time_series_with_same_time_factory(Resolution.hour, 0.003)
    result_df = _get_result_df(df, [805])
    result_df.show()
    points = result_df.collect()

    assert len(points) == 4  # one hourly quantity should yield 4 points

    for x in points:
        assert x["Quantity"] == Decimal("0.001")


# Test sums with both hourly and quarterly can be calculated
# Test that position works correctly
# Test that Quality is set and is None
# Test smallest Quantity supports that rounding up and
# Test that hourly Quantity is summed as quarterly?
# Test that GridAreaCode is in input is in output
# Test that only series from the GridArea is used to sum with
# Test rounding method? Ask Khatozen and Mads for type to use
# Test that multiple GridAreas receive each their calculation for a period
