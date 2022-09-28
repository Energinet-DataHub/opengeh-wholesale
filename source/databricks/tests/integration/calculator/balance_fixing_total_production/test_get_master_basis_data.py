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
from package.balance_fixing_total_production import _get_master_basis_data
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, sum, lit
from functools import reduce
from operator import add


@pytest.fixture(scope="module")
def metering_point_period_df_factory(spark, timestamp_factory):
    def factory(
        effective_date: datetime = timestamp_factory("2022-06-08T12:09:15.000Z"),
        to_effective_date: datetime = timestamp_factory("2022-06-10T22:00:00.000Z"),
        number_of_points=1,
    ):
        df_array = []

        for i in range(number_of_points):
            df_array.append(
                {
                    "GsrnNumber": "the-gsrn-number",
                    "GridAreaCode": "805",
                    "MeteringPointType": "the_metering_point_type",
                    "EffectiveDate": effective_date,
                    "toEffectiveDate": to_effective_date,
                    "FromGridAreaCode": "some-from-grid-area-code",
                    "ToGridAreaCode": "some-to-grid-area-code",
                    "SettlementMethod": "the_settlement_method",
                }
            )
        return spark.createDataFrame(df_array)

    return factory


def test__get_master_basis_data(metering_point_period_df_factory, timestamp_factory):

    metering_point_period_df = metering_point_period_df_factory().union(
        metering_point_period_df_factory(
            effective_date=timestamp_factory("2022-06-10T22:00:00.000Z"),
            to_effective_date=timestamp_factory("2022-06-12T22:00:00.000Z"),
        )
    )
    master_basis_data = _get_master_basis_data(metering_point_period_df)

    # Assert
    assert master_basis_data.columns == [
        "GridAreaCode",
        "METERINGPOINTID",
        "VALIDFROM",
        "VALIDTO",
        "GRIDAREAID",
        "TOGRIDAREAID",
        "FROMGRIDAREAID",
        "TYPEOFMP",
        "SETTLEMENTMETHOD",
    ]


def test__each_meteringpoint_has_a_row(
    metering_point_period_df_factory, timestamp_factory
):

    metering_point_period_df = metering_point_period_df_factory(number_of_points=3)

    master_basis_data = _get_master_basis_data(metering_point_period_df)

    # Assert: number of rows
    assert master_basis_data.count() == 3
