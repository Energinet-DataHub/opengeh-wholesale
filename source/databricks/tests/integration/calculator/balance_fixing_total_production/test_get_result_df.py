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
from package import calculate_balance_fixing_total_production
from package.balance_fixing_total_production import _get_result_df
from pyspark.sql import DataFrame
from pyspark.sql.functions import col

first_of_june = (datetime.strptime("31/05/2022 22:00", "%d/%m/%Y %H:%M"),)


@pytest.fixture
def new_df_factory(spark, timestamp_factory):
    def factory():
        df = [
            {
                "GridAreaCode": "805",
                "GsrnNumber": "2045555014",
                "Resolution": Resolution.quarter,
                "GridAreaLinkId": "GridAreaLinkId",
                "time": timestamp_factory("2022-06-08T12:09:15.000Z"),
                "Quantity": 1,
                "Quality": 4,
            },
            {
                "GridAreaCode": "805",
                "GsrnNumber": "2045555014",
                "Resolution": Resolution.quarter,
                "GridAreaLinkId": "GridAreaLinkId",
                "time": timestamp_factory("2022-06-08T12:09:30.000Z"),
                "Quantity": 1,
                "Quality": 4,
            },
        ]

        return spark.createDataFrame(df)

    return factory


def test__stored_time_matches_persister(new_df_factory):
    """Test that checks quantity is summed with only quaterly times"""
    new_df = new_df_factory()

    df = _get_result_df(new_df, [805])

    assert df.collect()["Quantity"] == 2.0
