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
from package.constants import Colname
from package.calculation.wholesale.schemas.calculate_daily_subscription_price_schema import (
    calculate_daily_subscription_price_schema,
)
from package.calculation.wholesale.schemas.charges_schema import (
    charges_schema,
    charges_master_data_schema,
    charge_links_schema,
    charge_prices_schema,
)
from package.calculation_input.schemas import (
    time_series_point_schema,
    metering_point_period_schema,
)
from tests.calculation.dataframe_defaults import DataframeDefaults


def test_calculate_daily_subscription_price(calculate_daily_subscription_price_factory):
    time = datetime(2020, 1, 1, 0, 0)
    price_per_day = Decimal("1.123456")
    charge_count = 1
    total_daily_charge_price = Decimal("1.123456")
    df = calculate_daily_subscription_price_factory(
        time, price_per_day, charge_count, total_daily_charge_price
    )
    result = df.collect()[0]
    assert len(df.columns) == len(calculate_daily_subscription_price_schema.fields)
    assert result[Colname.charge_key] == DataframeDefaults.default_charge_key
    assert result[Colname.charge_code] == DataframeDefaults.default_charge_code
    assert result[Colname.charge_type] == DataframeDefaults.default_charge_type
    assert result[Colname.charge_owner] == DataframeDefaults.default_charge_owner
    assert result[Colname.charge_price] == DataframeDefaults.default_charge_price
    assert result[Colname.charge_time] == time
    assert result[Colname.price_per_day] == price_per_day
    assert result[Colname.charge_count] == charge_count
    assert result[Colname.total_daily_charge_price] == total_daily_charge_price
    assert (
        result[Colname.metering_point_type]
        == DataframeDefaults.default_metering_point_type
    )
    assert (
        result[Colname.settlement_method] == DataframeDefaults.default_settlement_method
    )
    assert result[Colname.grid_area] == DataframeDefaults.default_grid_area
    assert (
        result[Colname.energy_supplier_id]
        == DataframeDefaults.default_energy_supplier_id
    )


def test_charges(charges_factory):
    df = charges_factory()
    result = df.collect()[0]
    assert len(df.columns) == len(charges_schema.fields)
    assert result[Colname.charge_key] == DataframeDefaults.default_charge_key
    assert result[Colname.charge_code] == DataframeDefaults.default_charge_code
    assert result[Colname.charge_type] == DataframeDefaults.default_charge_type
    assert result[Colname.charge_owner] == DataframeDefaults.default_charge_owner
    assert result[Colname.charge_tax] == DataframeDefaults.default_charge_tax
    assert (
        result[Colname.charge_resolution] == DataframeDefaults.default_charge_resolution
    )
    assert result[Colname.charge_time] == DataframeDefaults.default_charge_time
    assert result[Colname.from_date] == DataframeDefaults.default_from_date
    assert result[Colname.to_date] == DataframeDefaults.default_to_date
    assert result[Colname.charge_price] == DataframeDefaults.default_charge_price
    assert (
        result[Colname.metering_point_id] == DataframeDefaults.default_metering_point_id
    )


def test_charge_master_data(charge_master_data_factory):
    from_date = datetime(2020, 1, 1, 0, 0)
    to_date = datetime(2020, 1, 2, 0, 0)
    df = charge_master_data_factory(from_date, to_date)
    result = df.collect()[0]
    assert len(df.columns) == len(charges_master_data_schema.fields)
    assert result[Colname.charge_key] == DataframeDefaults.default_charge_key
    assert result[Colname.charge_code] == DataframeDefaults.default_charge_code
    assert result[Colname.charge_type] == DataframeDefaults.default_charge_type
    assert result[Colname.charge_owner] == DataframeDefaults.default_charge_owner
    assert (
        result[Colname.charge_resolution] == DataframeDefaults.default_charge_resolution
    )
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
    assert (
        result[Colname.metering_point_id] == DataframeDefaults.default_metering_point_id
    )
    assert result[Colname.from_date] == from_date
    assert result[Colname.to_date] == to_date


def test_charge_prices(charge_prices_factory):
    time = datetime(2020, 1, 1, 0, 0)
    df = charge_prices_factory(time)
    result = df.collect()[0]
    assert len(df.columns) == len(charge_prices_schema.fields)
    assert result[Colname.charge_key] == DataframeDefaults.default_charge_key
    assert result[Colname.charge_price] == DataframeDefaults.default_charge_price
    assert result[Colname.charge_time] == time


def test_metering_point(metering_point_period_factory):
    from_date = datetime(2020, 1, 1, 0, 0)
    to_date = datetime(2020, 1, 2, 0, 0)
    df = metering_point_period_factory(from_date, to_date)
    result = df.collect()[0]
    assert len(df.columns) == len(metering_point_period_schema.fields)
    assert (
        result[Colname.metering_point_id] == DataframeDefaults.default_metering_point_id
    )
    assert (
        result[Colname.metering_point_type]
        == DataframeDefaults.default_metering_point_type
    )
    assert (
        result[Colname.calculation_type] == DataframeDefaults.default_calculation_type
    )
    assert (
        result[Colname.settlement_method] == DataframeDefaults.default_settlement_method
    )
    assert result[Colname.grid_area] == DataframeDefaults.default_grid_area
    assert (
        result[Colname.resolution]
        == DataframeDefaults.default_metering_point_resolution
    )
    assert result[Colname.from_grid_area] == DataframeDefaults.default_from_grid_area
    assert result[Colname.to_grid_area] == DataframeDefaults.default_to_grid_area
    assert (
        result[Colname.parent_metering_point_id]
        == DataframeDefaults.default_parent_metering_point_id
    )
    assert (
        result[Colname.energy_supplier_id]
        == DataframeDefaults.default_energy_supplier_id
    )
    assert (
        result[Colname.balance_responsible_id]
        == DataframeDefaults.default_balance_responsible_id
    )
    assert result[Colname.from_date] == from_date
    assert result[Colname.to_date] == to_date


def test_time_series(time_series_factory):
    time = datetime(2020, 1, 1, 0, 0)
    df = time_series_factory(time)
    result = df.collect()[0]
    assert len(df.columns) == len(time_series_point_schema.fields)
    assert (
        result[Colname.metering_point_id] == DataframeDefaults.default_metering_point_id
    )
    assert result[Colname.quantity] == DataframeDefaults.default_quantity
    assert result[Colname.quality] == DataframeDefaults.default_quality
    assert result[Colname.observation_time] == time
