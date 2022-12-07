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
from geh_stream.codelists import Colname
from geh_stream.schemas import charges_schema, charge_links_schema, charge_prices_schema
from tests.helpers import DataframeDefaults
import pytest
import pandas as pd


@pytest.fixture(scope="module")
def charges_factory(spark):
    def factory(
        from_date: datetime,
        to_date: datetime,
        charge_key=DataframeDefaults.default_charge_key,
        charge_id=DataframeDefaults.default_charge_id,
        charge_type=DataframeDefaults.default_charge_type,
        charge_owner=DataframeDefaults.default_charge_owner,
        resolution=DataframeDefaults.default_resolution,
        charge_tax=DataframeDefaults.default_charge_tax,
        currency=DataframeDefaults.default_currency
    ):
        pandas_df = pd.DataFrame().append([{
            Colname.charge_key: charge_key,
            Colname.charge_id: charge_id,
            Colname.charge_type: charge_type,
            Colname.charge_owner: charge_owner,
            Colname.resolution: resolution,
            Colname.charge_tax: charge_tax,
            Colname.currency: currency,
            Colname.from_date: from_date,
            Colname.to_date: to_date}],
            ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=charges_schema)
    return factory


@pytest.fixture(scope="module")
def charge_links_factory(spark):
    def factory(
        from_date: datetime,
        to_date: datetime,
        charge_key=DataframeDefaults.default_charge_key,
        metering_point_id=DataframeDefaults.default_metering_point_id
    ):
        pandas_df = pd.DataFrame().append([{
            Colname.charge_key: charge_key,
            Colname.metering_point_id: metering_point_id,
            Colname.from_date: from_date,
            Colname.to_date: to_date}],
            ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=charge_links_schema)
    return factory


@pytest.fixture(scope="module")
def charge_prices_factory(spark):
    def factory(
        time: datetime,
        charge_key=DataframeDefaults.default_charge_key,
        charge_price=DataframeDefaults.default_charge_price
    ):
        pandas_df = pd.DataFrame().append([{
            Colname.charge_key: charge_key,
            Colname.charge_price: charge_price,
            Colname.time: time}],
            ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=charge_prices_schema)
    return factory
