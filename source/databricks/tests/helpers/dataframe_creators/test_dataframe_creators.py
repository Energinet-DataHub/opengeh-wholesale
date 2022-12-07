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
from decimal import Decimal
from geh_stream.codelists import Colname
from geh_stream.schemas import charges_schema, charge_links_schema, charge_prices_schema, metering_point_schema, market_roles_schema
from geh_stream.schemas.output import calculate_daily_subscription_price_schema
from geh_stream.schemas import time_series_points_schema
from tests.helpers.dataframe_creators.calculate_daily_subscription_price_creator import calculate_daily_subscription_price_factory
from tests.helpers.dataframe_creators.charges_creator import charges_factory, charge_links_factory, charge_prices_factory
from tests.helpers.dataframe_creators.market_roles_creator import market_roles_factory
from tests.helpers.dataframe_creators.metering_point_creator import metering_point_factory
from tests.helpers.dataframe_creators.time_series_creator import time_series_factory
from tests.helpers import DataframeDefaults
import pytest
import pandas as pd


def test_calculate_daily_subscription_price(calculate_daily_subscription_price_factory):
    time = datetime(2020, 1, 1, 0, 0)
    price_per_day = Decimal("1.123456")
    charge_count = 1
    total_daily_charge_price = Decimal("1.123456")
    df = calculate_daily_subscription_price_factory(time, price_per_day, charge_count, total_daily_charge_price)
    result = df.collect()[0]
    assert len(df.columns) == len(calculate_daily_subscription_price_schema.fields)
    assert result[Colname.charge_key] == DataframeDefaults.default_charge_key
    assert result[Colname.charge_id] == DataframeDefaults.default_charge_id
    assert result[Colname.charge_type] == DataframeDefaults.default_charge_type
    assert result[Colname.charge_owner] == DataframeDefaults.default_charge_owner
    assert result[Colname.charge_price] == DataframeDefaults.default_charge_price
    assert result[Colname.time] == time
    assert result[Colname.price_per_day] == price_per_day
    assert result[Colname.charge_count] == charge_count
    assert result[Colname.total_daily_charge_price] == total_daily_charge_price
    assert result[Colname.metering_point_type] == DataframeDefaults.default_metering_point_type
    assert result[Colname.settlement_method] == DataframeDefaults.default_settlement_method
    assert result[Colname.grid_area] == DataframeDefaults.default_grid_area
    assert result[Colname.connection_state] == DataframeDefaults.default_connection_state
    assert result[Colname.energy_supplier_id] == DataframeDefaults.default_energy_supplier_id


def test_charges(charges_factory):
    from_date = datetime(2020, 1, 1, 0, 0)
    to_date = datetime(2020, 1, 2, 0, 0)
    df = charges_factory(from_date, to_date)
    result = df.collect()[0]
    assert len(df.columns) == len(charges_schema.fields)
    assert result[Colname.charge_key] == DataframeDefaults.default_charge_key
    assert result[Colname.charge_id] == DataframeDefaults.default_charge_id
    assert result[Colname.charge_type] == DataframeDefaults.default_charge_type
    assert result[Colname.charge_owner] == DataframeDefaults.default_charge_owner
    assert result[Colname.resolution] == DataframeDefaults.default_resolution
    assert result[Colname.charge_tax] == DataframeDefaults.default_charge_tax
    assert result[Colname.currency] == DataframeDefaults.default_currency
    assert result[Colname.from_date] == from_date
    assert result[Colname.to_date] == to_date


def test_charge_links(charge_links_factory):
    from_date = datetime(2020, 1, 1, 0, 0)
    to_date = datetime(2020, 1, 2, 0, 0)
    df = charge_links_factory(from_date, to_date)
    result = df.collect()[0]
    assert len(df.columns) == len(charge_links_schema.fields)
    assert result[Colname.charge_key] == DataframeDefaults.default_charge_key
    assert result[Colname.metering_point_id] == DataframeDefaults.default_metering_point_id
    assert result[Colname.from_date] == from_date
    assert result[Colname.to_date] == to_date


def test_charge_prices(charge_prices_factory):
    time = datetime(2020, 1, 1, 0, 0)
    df = charge_prices_factory(time)
    result = df.collect()[0]
    assert len(df.columns) == len(charge_prices_schema.fields)
    assert result[Colname.charge_key] == DataframeDefaults.default_charge_key
    assert result[Colname.charge_price] == DataframeDefaults.default_charge_price
    assert result[Colname.time] == time


def test_market_roles(market_roles_factory):
    from_date = datetime(2020, 1, 1, 0, 0)
    to_date = datetime(2020, 1, 2, 0, 0)
    df = market_roles_factory(from_date, to_date)
    result = df.collect()[0]
    assert len(df.columns) == len(market_roles_schema.fields)
    assert result[Colname.energy_supplier_id] == DataframeDefaults.default_energy_supplier_id
    assert result[Colname.metering_point_id] == DataframeDefaults.default_metering_point_id
    assert result[Colname.from_date] == from_date
    assert result[Colname.to_date] == to_date


def test_metering_point(metering_point_factory):
    from_date = datetime(2020, 1, 1, 0, 0)
    to_date = datetime(2020, 1, 2, 0, 0)
    df = metering_point_factory(from_date, to_date)
    result = df.collect()[0]
    assert len(df.columns) == len(metering_point_schema.fields)
    assert result[Colname.metering_point_id] == DataframeDefaults.default_metering_point_id
    assert result[Colname.metering_point_type] == DataframeDefaults.default_metering_point_type
    assert result[Colname.settlement_method] == DataframeDefaults.default_settlement_method
    assert result[Colname.grid_area] == DataframeDefaults.default_grid_area
    assert result[Colname.connection_state] == DataframeDefaults.default_connection_state
    assert result[Colname.resolution] == DataframeDefaults.default_resolution
    assert result[Colname.in_grid_area] == DataframeDefaults.default_in_grid_area
    assert result[Colname.out_grid_area] == DataframeDefaults.default_out_grid_area
    assert result[Colname.metering_method] == DataframeDefaults.default_metering_method
    assert result[Colname.parent_metering_point_id] == DataframeDefaults.default_parent_metering_point_id
    assert result[Colname.unit] == DataframeDefaults.default_unit
    assert result[Colname.product] == DataframeDefaults.default_product
    assert result[Colname.from_date] == from_date
    assert result[Colname.to_date] == to_date


def test_time_series(time_series_factory):
    time = datetime(2020, 1, 1, 0, 0)
    df = time_series_factory(time)
    result = df.collect()[0]
    assert len(df.columns) == len(time_series_points_schema.fields)
    assert result[Colname.metering_point_id] == DataframeDefaults.default_metering_point_id
    assert result[Colname.quantity] == DataframeDefaults.default_quantity
    assert result[Colname.quality] == DataframeDefaults.default_quality
    assert result[Colname.time] == time
