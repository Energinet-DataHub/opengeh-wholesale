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


from datetime import datetime
import pytest
from package.balance_fixing_total_production import (
    _get_metering_point_periods_from_static_datasource_df,
)
import os


# Factory defaults
grid_area_code = "805"
grid_area_link_id = "the-grid-area-link-id"


@pytest.fixture
def grid_area_df(spark):
    row = {
        "GridAreaCode": grid_area_code,
        "GridAreaLinkId": grid_area_link_id,
    }
    return spark.createDataFrame([row])


def test_metering_points_have_energy_supplier(
    databricks_path,
    spark,
    grid_area_df,
):
    metering_points_periods_df = spark.read.option("header", "true").csv(
        f"{databricks_path}/package/datasources/MeteringPoints.csv"
    )
    market_roles_periods_df = spark.read.option("header", "true").csv(
        f"{databricks_path}/package/datasources/MarketRolesPeriods.csv"
    )
    period_start_datetime = datetime.strptime("01/01/2018 00:00", "%d/%m/%Y %H:%M")
    period_end_datetime = datetime.strptime("01/03/2018 22:00", "%d/%m/%Y %H:%M")

    actual = _get_metering_point_periods_from_static_datasource_df(
        metering_points_periods_df,
        market_roles_periods_df,
        grid_area_df,
        period_start_datetime,
        period_end_datetime,
    )

    assert actual.count() > 1
