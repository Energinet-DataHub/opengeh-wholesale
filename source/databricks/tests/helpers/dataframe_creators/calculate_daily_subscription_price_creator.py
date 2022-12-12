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
from geh_stream.schemas.output import calculate_daily_subscription_price_schema
from tests.helpers import DataframeDefaults
import pytest
import pandas as pd


@pytest.fixture(scope="module")
def calculate_daily_subscription_price_factory(spark):
    def factory(
        time=datetime,
        price_per_day=Decimal,
        charge_count=int,
        total_daily_charge_price=Decimal,
        charge_key=DataframeDefaults.default_charge_key,
        charge_id=DataframeDefaults.default_charge_id,
        charge_type=DataframeDefaults.default_charge_type,
        charge_owner=DataframeDefaults.default_charge_owner,
        charge_price=DataframeDefaults.default_charge_price,
        metering_point_type=DataframeDefaults.default_metering_point_type,
        settlement_method=DataframeDefaults.default_settlement_method,
        grid_area=DataframeDefaults.default_grid_area,
        connection_state=DataframeDefaults.default_connection_state,
        energy_supplier_id=DataframeDefaults.default_energy_supplier_id
    ):
        pandas_df = pd.DataFrame().append([{
            Colname.charge_key: charge_key,
            Colname.charge_id: charge_id,
            Colname.charge_type: charge_type,
            Colname.charge_owner: charge_owner,
            Colname.charge_price: charge_price,
            Colname.time: time,
            Colname.price_per_day: price_per_day,
            Colname.charge_count: charge_count,
            Colname.total_daily_charge_price: total_daily_charge_price,
            Colname.metering_point_type: metering_point_type,
            Colname.settlement_method: settlement_method,
            Colname.grid_area: grid_area,
            Colname.connection_state: connection_state,
            Colname.energy_supplier_id: energy_supplier_id}],
            ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=calculate_daily_subscription_price_schema)
    return factory
