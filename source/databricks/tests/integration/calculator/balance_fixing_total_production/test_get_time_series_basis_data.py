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
import time
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
from pyspark.sql.functions import col, sum

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
        number_of_points=500,
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
            time = time + timedelta(minutes=15)
            # (
            #    time + timedelta(minutes=60)
            #    if resolution == Resolution.hour.value
            #    else time + timedelta(minutes=15)
            # )
        return spark.createDataFrame(df_array)

    return factory


# +------------------+-------------+---------+-------+----------+--------------------+--------------------+-------------------+----+-----+---+
# |        GsrnNumber|TransactionId| Quantity|Quality|Resolution|RegistrationDateTime|          storedTime|               time|year|month|day|
# +------------------+-------------+---------+-------+----------+--------------------+--------------------+-------------------+----+-----+---+
# |576003432716622530|     C1875000|    9.090|      4|         1| 2022-08-04 08:30:00|2022-08-09 13:10:...|2022-06-01 00:00:00|2022|    6|  1|
# |576003432716622530|     C1875000|   10.100|      4|         1| 2022-08-04 08:30:00|2022-08-09 13:10:...|2022-06-01 00:15:00|2022|    6|  1|
# |576003432716622530|     C1875000|   11.110|      4|         1| 2022-08-04 08:30:00|2022-08-09 13:10:...|2022-06-01 00:30:00|2022|    6|  1|

# GridAreaCode|            Quantity|Resolution|               time|
# +------------+--------------------+----------+-------------------+
# |         805|1.000000000000000000|         1|2022-06-08 12:09:15|
# |         805|2.000000000000000000|         1|2022-06-08 12:09:15|
# +------------+--------------------+----------+-------------------+
# |GridAreaCode|     GsrnNumber|Resolution|               time|            Quantity|
# +------------+---------------+----------+-------------------+--------------------+
# |         805|the-gsrn-number|         2|2022-06-08 12:09:15|1.100000000000000000|
# +------------+---------------+----------+-------------------+--------------------+
# METERINGPOINTID	TYPEOFMP	STARTDATETIME	RESOLUTIONDURATION


def test__get_timeseries_basis_data(enriched_time_series_factory):

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

    # Assert: Metering point id, type of mp, 96 energi quantities = 99 columns
    assert len(quarter_df.columns) == 99

    # Assert: Metering point id, type of mp, 24 energi quantities = 27 columns
    assert len(hour_df.columns) == 27


def test__has_correct_number_of_quantity_columns_according_to_dst():
    # At least 3 test cases for 23, 24, 25 columns
    raise Exception("todo")


def test__returns_dataframe_with_quarter_resolution_metering_points():
    raise Exception("todo")


def test__returns_dataframe_with_hour_resolution_metering_points():
    raise Exception("todo")


def test__splits_single_metering_point_with_different_resolution_on_different_dates():
    raise Exception("todo")


def test__returns_expected_data_for_each_column():
    # Test for both hour and quarter dataframes
    raise Exception("todo")


def test__multiple_dates_are_split_into_rows():
    # each date per metering point is a separate row
    raise Exception("todo")
