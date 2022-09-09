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
        gridArea="805",
        gsrnNumber="the_gsrn_number",
        MeteringPointType="the_metering_point_type",
        time="2022-06-08T22:00:00.000Z",
        numberOfPoints=24,
    ):
        df_array = []

        time = timestamp_factory(time)

        for i in range(numberOfPoints):

            df_array.append(
                {
                    "GridAreaCode": gridArea,
                    "Resolution": resolution,
                    "GridAreaLinkId": "GridAreaLinkId",
                    "time": time,
                    "Quantity": quantity,
                    "GsrnNumber": gsrnNumber,
                    "MeteringPointType": MeteringPointType,
                }
            )
            time = time + timedelta(minutes=15)
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
        time="2022-06-08T22:00:00.000Z"
    )
    timeseries_basis_data = _get_time_series_basis_data(enriched_time_series_points_df)
    assert len(timeseries_basis_data.columns) == 28
