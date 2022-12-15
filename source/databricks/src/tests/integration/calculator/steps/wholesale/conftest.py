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

from package.codelists import ChargeType
from package.schemas.output import (
    aggregation_result_schema,
    calculate_daily_subscription_price_schema,
    calculate_fee_charge_price_schema,
)
from tests.helpers import DataframeDefaults
import pytest
import pandas as pd
from datetime import datetime
from decimal import Decimal
from package.schemas import (
    charges_schema,
    charge_links_schema,
    charge_prices_schema,
    es_brp_relations_schema,
    market_roles_schema,
    metering_point_schema,
    time_series_point_schema,
)
from package.constants import Colname


@pytest.fixture(scope="session")
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
        energy_supplier_id=DataframeDefaults.default_energy_supplier_id,
    ):
        pandas_df = pd.DataFrame().append(
            [
                {
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
                    Colname.energy_supplier_id: energy_supplier_id,
                }
            ],
            ignore_index=True,
        )

        return spark.createDataFrame(
            pandas_df, schema=calculate_daily_subscription_price_schema
        )

    return factory


@pytest.fixture(scope="session")
def calculate_fee_charge_price_factory(spark):
    def factory(
        time=datetime,
        charge_count=int,
        total_daily_charge_price=Decimal,
        charge_key=DataframeDefaults.default_charge_key,
        charge_id=DataframeDefaults.default_charge_id,
        charge_type=ChargeType.fee,
        charge_owner=DataframeDefaults.default_charge_owner,
        charge_price=DataframeDefaults.default_charge_price,
        metering_point_type=DataframeDefaults.default_metering_point_type,
        settlement_method=DataframeDefaults.default_settlement_method,
        grid_area=DataframeDefaults.default_grid_area,
        connection_state=DataframeDefaults.default_connection_state,
        energy_supplier_id=DataframeDefaults.default_energy_supplier_id,
    ):
        pandas_df = pd.DataFrame().append(
            [
                {
                    Colname.charge_key: charge_key,
                    Colname.charge_id: charge_id,
                    Colname.charge_type: charge_type,
                    Colname.charge_owner: charge_owner,
                    Colname.charge_price: charge_price,
                    Colname.time: time,
                    Colname.charge_count: charge_count,
                    Colname.total_daily_charge_price: total_daily_charge_price,
                    Colname.metering_point_type: metering_point_type,
                    Colname.settlement_method: settlement_method,
                    Colname.grid_area: grid_area,
                    Colname.connection_state: connection_state,
                    Colname.energy_supplier_id: energy_supplier_id,
                }
            ],
            ignore_index=True,
        )

        return spark.createDataFrame(
            pandas_df, schema=calculate_fee_charge_price_schema
        )

    return factory


@pytest.fixture(scope="session")
def charges_factory(spark):
    def factory(
        from_date: datetime,
        to_date: datetime,
        charge_key=DataframeDefaults.default_charge_key,
        charge_id=DataframeDefaults.default_charge_id,
        charge_type=DataframeDefaults.default_charge_type,
        charge_owner=DataframeDefaults.default_charge_owner,
        charge_resolution=DataframeDefaults.default_charge_resolution,
        charge_tax=DataframeDefaults.default_charge_tax,
        currency=DataframeDefaults.default_currency,
    ):
        pandas_df = pd.DataFrame().append(
            [
                {
                    Colname.charge_key: charge_key,
                    Colname.charge_id: charge_id,
                    Colname.charge_type: charge_type,
                    Colname.charge_owner: charge_owner,
                    Colname.resolution: charge_resolution,
                    Colname.charge_tax: charge_tax,
                    Colname.currency: currency,
                    Colname.from_date: from_date,
                    Colname.to_date: to_date,
                }
            ],
            ignore_index=True,
        )

        return spark.createDataFrame(pandas_df, schema=charges_schema)

    return factory


@pytest.fixture(scope="session")
def charge_links_factory(spark):
    def factory(
        from_date: datetime,
        to_date: datetime,
        charge_key=DataframeDefaults.default_charge_key,
        metering_point_id=DataframeDefaults.default_metering_point_id,
    ):
        pandas_df = pd.DataFrame().append(
            [
                {
                    Colname.charge_key: charge_key,
                    Colname.metering_point_id: metering_point_id,
                    Colname.from_date: from_date,
                    Colname.to_date: to_date,
                }
            ],
            ignore_index=True,
        )

        return spark.createDataFrame(pandas_df, schema=charge_links_schema)

    return factory


@pytest.fixture(scope="session")
def charge_prices_factory(spark):
    def factory(
        time: datetime,
        charge_key=DataframeDefaults.default_charge_key,
        charge_price=DataframeDefaults.default_charge_price,
    ):
        pandas_df = pd.DataFrame().append(
            [
                {
                    Colname.charge_key: charge_key,
                    Colname.charge_price: charge_price,
                    Colname.time: time,
                }
            ],
            ignore_index=True,
        )

        return spark.createDataFrame(pandas_df, schema=charge_prices_schema)

    return factory


@pytest.fixture(scope="session")
def es_brp_relations_factory(spark):
    def factory(
        from_date: datetime,
        to_date: datetime,
        energy_supplier_id=DataframeDefaults.default_energy_supplier_id,
        balance_responsible_id=DataframeDefaults.default_balance_responsible_id,
        grid_area=DataframeDefaults.default_grid_area,
        metering_point_type=DataframeDefaults.default_metering_point_type,
    ):
        pandas_df = pd.DataFrame().append(
            [
                {
                    Colname.energy_supplier_id: energy_supplier_id,
                    Colname.balance_responsible_id: balance_responsible_id,
                    Colname.grid_area: grid_area,
                    Colname.metering_point_type: metering_point_type,
                    Colname.from_date: from_date,
                    Colname.to_date: to_date,
                }
            ],
            ignore_index=True,
        )

        return spark.createDataFrame(pandas_df, schema=es_brp_relations_schema)

    return factory


@pytest.fixture(scope="session")
def market_roles_factory(spark):
    def factory(
        from_date: datetime,
        to_date: datetime,
        metering_point_id=DataframeDefaults.default_metering_point_id,
        energy_supplier_id=DataframeDefaults.default_energy_supplier_id,
    ):
        pandas_df = pd.DataFrame().append(
            [
                {
                    Colname.energy_supplier_id: energy_supplier_id,
                    Colname.metering_point_id: metering_point_id,
                    Colname.from_date: from_date,
                    Colname.to_date: to_date,
                }
            ],
            ignore_index=True,
        )

        return spark.createDataFrame(pandas_df, schema=market_roles_schema)

    return factory


@pytest.fixture(scope="session")
def metering_point_factory(spark):
    def factory(
        from_date: datetime,
        to_date: datetime,
        metering_point_id=DataframeDefaults.default_metering_point_id,
        metering_point_type=DataframeDefaults.default_metering_point_type,
        settlement_method=DataframeDefaults.default_settlement_method,
        grid_area=DataframeDefaults.default_grid_area,
        connection_state=DataframeDefaults.default_connection_state,
        resolution=DataframeDefaults.default_metering_point_resolution,
        in_grid_area=DataframeDefaults.default_in_grid_area,
        out_grid_area=DataframeDefaults.default_out_grid_area,
        metering_method=DataframeDefaults.default_metering_method,
        parent_metering_point_id=DataframeDefaults.default_parent_metering_point_id,
        unit=DataframeDefaults.default_unit,
        product=DataframeDefaults.default_product,
    ):
        pandas_df = pd.DataFrame().append(
            [
                {
                    Colname.metering_point_id: metering_point_id,
                    Colname.metering_point_type: metering_point_type,
                    Colname.settlement_method: settlement_method,
                    Colname.grid_area: grid_area,
                    Colname.connection_state: connection_state,
                    Colname.resolution: resolution,
                    Colname.in_grid_area: in_grid_area,
                    Colname.out_grid_area: out_grid_area,
                    Colname.metering_method: metering_method,
                    Colname.parent_metering_point_id: parent_metering_point_id,
                    Colname.unit: unit,
                    Colname.product: product,
                    Colname.from_date: from_date,
                    Colname.to_date: to_date,
                }
            ],
            ignore_index=True,
        )

        return spark.createDataFrame(pandas_df, schema=metering_point_schema)

    return factory


@pytest.fixture(scope="session")
def time_series_factory(spark):
    def factory(
        time: datetime,
        metering_point_id=DataframeDefaults.default_metering_point_id,
        quantity=DataframeDefaults.default_quantity,
        ts_quality=DataframeDefaults.default_quality,
    ):
        pandas_df = pd.DataFrame().append(
            [
                {
                    Colname.metering_point_id: metering_point_id,
                    Colname.quantity: quantity,
                    Colname.quality: ts_quality,
                    Colname.time: time,
                    Colname.year: time.year,
                    Colname.month: time.month,
                    Colname.day: time.day,
                }
            ],
            ignore_index=True,
        )

        return spark.createDataFrame(pandas_df, schema=time_series_point_schema)

    return factory
