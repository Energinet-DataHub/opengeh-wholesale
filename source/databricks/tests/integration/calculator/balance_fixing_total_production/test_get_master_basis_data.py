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
from package.codelists import MeteringPointType
from decimal import Decimal
from package import calculate_balance_fixing_total_production
from package.balance_fixing_total_production import _get_master_basis_data
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, sum, lit
from functools import reduce
from operator import add
from pyspark.sql.types import (
    StructField,
    StringType,
    TimestampType,
    StructType,
)


@pytest.fixture(scope="module")
def metering_point_period_df_factory(spark, timestamp_factory):
    def factory(
        gsrn_number="the-gsrn-number",
        grid_area_code="some-grid-area-code",
        effective_date: datetime = timestamp_factory("2022-06-08T12:09:15.000Z"),
        to_effective_date: datetime = timestamp_factory("2022-06-10T22:00:00.000Z"),
        meteringpoint_type=MeteringPointType.production.value,
        from_grid_area_code="some-from-grid-area-code",
        to_grid_area_code="some-to-grid-area-code",
        settlement_method="some-settlement-method",
    ):
        row = {
            "GsrnNumber": gsrn_number,
            "GridAreaCode": grid_area_code,
            "MeteringPointType": meteringpoint_type,
            "EffectiveDate": effective_date,
            "toEffectiveDate": to_effective_date,
            "FromGridAreaCode": from_grid_area_code,
            "ToGridAreaCode": to_grid_area_code,
            "SettlementMethod": settlement_method,
        }
        return spark.createDataFrame([row])

    return factory


def test__get_master_basis_data_has_expected_columns(
    metering_point_period_df_factory, timestamp_factory
):

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
        "GRIDAREA",
        "TOGRIDAREA",
        "FROMGRIDAREA",
        "TYPEOFMP",
        "SETTLEMENTMETHOD",
        "ENERGYSUPPLIERID",
    ]


def test__each_meteringpoint_has_a_row(
    metering_point_period_df_factory, timestamp_factory
):
    expected_number_of_metering_points = 3
    metering_point_period_df = (
        metering_point_period_df_factory(gsrn_number="1")
        .union(metering_point_period_df_factory(gsrn_number="2"))
        .union(metering_point_period_df_factory(gsrn_number="3"))
    )

    master_basis_data = _get_master_basis_data(metering_point_period_df)

    # Assert
    assert master_basis_data.count() == expected_number_of_metering_points


def test__columns_have_expected_values(
    metering_point_period_df_factory, timestamp_factory
):
    expected_gsrn_number = "the-gsrn-number"
    expected_grid_area_code = "some-grid-area-code"
    expected_effective_date = timestamp_factory("2022-06-08T12:09:15.000Z")
    expected_to_effective_date = timestamp_factory("2022-06-10T22:00:00.000Z")
    expected_meteringpoint_type = "E18"
    expected_from_grid_area_code = "some-from-grid-area-code"
    expected_to_grid_area_code = "some-to-grid-area-code"
    expected_settlement_method = "some-settlement-method"

    metering_point_period_df = metering_point_period_df_factory(
        gsrn_number=expected_gsrn_number,
        grid_area_code=expected_grid_area_code,
        effective_date=expected_effective_date,
        to_effective_date=expected_to_effective_date,
        meteringpoint_type=MeteringPointType.production.value,
        from_grid_area_code=expected_from_grid_area_code,
        to_grid_area_code=expected_to_grid_area_code,
        settlement_method=expected_settlement_method,
    )

    master_basis_data = _get_master_basis_data(metering_point_period_df)

    # Assert
    actual = master_basis_data.first()

    assert actual.GridAreaCode == expected_grid_area_code
    assert actual.METERINGPOINTID == expected_gsrn_number
    assert actual.VALIDFROM == expected_effective_date
    assert actual.VALIDTO == expected_to_effective_date
    assert actual.GRIDAREA == expected_grid_area_code
    assert actual.TOGRIDAREA == expected_to_grid_area_code
    assert actual.FROMGRIDAREA == expected_from_grid_area_code
    assert actual.TYPEOFMP == expected_meteringpoint_type
    assert actual.SETTLEMENTMETHOD == expected_settlement_method
    assert actual.ENERGYSUPPLIERID == ""
