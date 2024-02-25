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
    assert (
        result[Colname.charge_key]
        == f"{DataframeDefaults.default_charge_code}-{DataframeDefaults.default_charge_owner}-{DataframeDefaults.default_charge_type}"
    )
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
