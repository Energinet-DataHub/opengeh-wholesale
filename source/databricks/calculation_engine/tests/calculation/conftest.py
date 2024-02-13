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
from pyspark.sql import SparkSession, DataFrame
import pytest
from typing import Callable
from package.codelists import ChargeType
from package.calculation.wholesale.schemas.calculate_daily_subscription_price_schema import (
    calculate_daily_subscription_price_schema,
)
from package.calculation.wholesale.schemas.calculate_fee_charge_price_schema import (
    calculate_fee_charge_price_schema,
)
from tests.calculation.dataframe_defaults import DataframeDefaults
from package.calculation.wholesale.schemas.charges_schema import (
    charges_schema,
    charges_master_data_schema,
    charge_link_metering_points_schema,
    charge_prices_schema,
)
from package.calculation_input.schemas import (
    time_series_point_schema,
    metering_point_period_schema,
)
from package.constants import Colname


@pytest.fixture(scope="session")
def calculate_daily_subscription_price_factory(
    spark: SparkSession,
) -> Callable[..., DataFrame]:
    def factory(
        time: datetime,
        price_per_day: Decimal,
        charge_count: int,
        total_daily_charge_price: Decimal = DataframeDefaults.default_charge_price,
        charge_key: str = DataframeDefaults.default_charge_key,
        charge_code: str = DataframeDefaults.default_charge_code,
        charge_type: str = DataframeDefaults.default_charge_type,
        charge_owner: str = DataframeDefaults.default_charge_owner,
        charge_price: Decimal = DataframeDefaults.default_charge_price,
        metering_point_type: str = DataframeDefaults.default_metering_point_type,
        settlement_method: str = DataframeDefaults.default_settlement_method,
        grid_area: str = DataframeDefaults.default_grid_area,
        energy_supplier_id: str = DataframeDefaults.default_energy_supplier_id,
    ) -> DataFrame:
        data = [
            {
                Colname.charge_key: charge_key,
                Colname.charge_code: charge_code,
                Colname.charge_type: charge_type,
                Colname.charge_owner: charge_owner,
                Colname.charge_price: charge_price,
                Colname.charge_time: time,
                Colname.price_per_day: price_per_day,
                Colname.charge_count: charge_count,
                Colname.total_daily_charge_price: total_daily_charge_price,
                Colname.metering_point_type: metering_point_type,
                Colname.settlement_method: settlement_method,
                Colname.grid_area: grid_area,
                Colname.energy_supplier_id: energy_supplier_id,
            }
        ]

        return spark.createDataFrame(
            data, schema=calculate_daily_subscription_price_schema
        )

    return factory


@pytest.fixture(scope="session")
def calculate_fee_charge_price_factory(spark: SparkSession) -> Callable[..., DataFrame]:
    def factory(
        time: datetime,
        charge_count: int,
        total_daily_charge_price: Decimal,
        charge_key: str = DataframeDefaults.default_charge_key,
        charge_code: str = DataframeDefaults.default_charge_code,
        charge_type: str = ChargeType.FEE.value,
        charge_owner: str = DataframeDefaults.default_charge_owner,
        charge_price: Decimal = DataframeDefaults.default_charge_price,
        metering_point_type: str = DataframeDefaults.default_metering_point_type,
        settlement_method: str = DataframeDefaults.default_settlement_method,
        grid_area: str = DataframeDefaults.default_grid_area,
        energy_supplier_id: str = DataframeDefaults.default_energy_supplier_id,
    ) -> DataFrame:
        data = [
            {
                Colname.charge_key: charge_key,
                Colname.charge_code: charge_code,
                Colname.charge_type: charge_type,
                Colname.charge_owner: charge_owner,
                Colname.charge_price: charge_price,
                Colname.charge_time: time,
                Colname.charge_count: charge_count,
                Colname.total_daily_charge_price: total_daily_charge_price,
                Colname.metering_point_type: metering_point_type,
                Colname.settlement_method: settlement_method,
                Colname.grid_area: grid_area,
                Colname.energy_supplier_id: energy_supplier_id,
            }
        ]

        return spark.createDataFrame(data, schema=calculate_fee_charge_price_schema)

    return factory


@pytest.fixture(scope="session")
def charge_master_data_factory(spark: SparkSession) -> Callable[..., DataFrame]:
    def factory(
        from_date: datetime,
        to_date: datetime,
        charge_key: str = DataframeDefaults.default_charge_key,
        charge_code: str = DataframeDefaults.default_charge_code,
        charge_type: str = DataframeDefaults.default_charge_type,
        charge_owner: str = DataframeDefaults.default_charge_owner,
        resolution: str = DataframeDefaults.default_charge_resolution,
        charge_tax: str = DataframeDefaults.default_charge_tax,
        currency: str = DataframeDefaults.default_currency,
    ) -> DataFrame:
        data = [
            {
                Colname.charge_key: charge_key,
                Colname.charge_code: charge_code,
                Colname.charge_type: charge_type,
                Colname.charge_owner: charge_owner,
                Colname.resolution: resolution,
                Colname.charge_tax: charge_tax,
                Colname.currency: currency,
                Colname.from_date: from_date,
                Colname.to_date: to_date,
            }
        ]

        return spark.createDataFrame(data, schema=charges_master_data_schema)

    return factory


@pytest.fixture(scope="session")
def charge_links_factory(spark: SparkSession) -> Callable[..., DataFrame]:
    def factory(
        from_date: datetime,
        to_date: datetime,
        charge_key: str = DataframeDefaults.default_charge_key,
        metering_point_id: str = DataframeDefaults.default_metering_point_id,
    ) -> DataFrame:
        data = [
            {
                Colname.charge_key: charge_key,
                Colname.metering_point_id: metering_point_id,
                Colname.from_date: from_date,
                Colname.to_date: to_date,
            }
        ]

        return spark.createDataFrame(data, schema=charge_link_metering_points_schema)

    return factory


@pytest.fixture(scope="session")
def charge_prices_factory(spark: SparkSession) -> Callable[..., DataFrame]:
    def factory(
        time: datetime,
        charge_key: str = DataframeDefaults.default_charge_key,
        charge_price: Decimal = DataframeDefaults.default_charge_price,
    ) -> DataFrame:
        data = [
            {
                Colname.charge_key: charge_key,
                Colname.charge_price: charge_price,
                Colname.charge_time: time,
            }
        ]

        return spark.createDataFrame(data, schema=charge_prices_schema)

    return factory


@pytest.fixture(scope="session")
def charges_factory(spark: SparkSession) -> Callable[..., DataFrame]:
    def factory(
        charge_code: str = DataframeDefaults.default_charge_code,
        charge_type: str = DataframeDefaults.default_charge_type,
        charge_owner: str = DataframeDefaults.default_charge_owner,
        charge_resolution: str = DataframeDefaults.default_charge_resolution,
        charge_tax: str = DataframeDefaults.default_charge_tax,
        time: datetime = DataframeDefaults.default_charge_time,
        charge_price: Decimal = DataframeDefaults.default_charge_price,
    ) -> DataFrame:
        data = [
            {
                Colname.charge_key: DataframeDefaults.default_charge_key,
                Colname.charge_code: charge_code,
                Colname.charge_type: charge_type,
                Colname.charge_owner: charge_owner,
                Colname.charge_tax: charge_tax,
                Colname.resolution: charge_resolution,
                Colname.charge_time: time,
                Colname.from_date: DataframeDefaults.default_from_date,
                Colname.to_date: DataframeDefaults.default_to_date,
                Colname.charge_price: charge_price,
                Colname.metering_point_id: DataframeDefaults.default_metering_point_id,
            }
        ]

        return spark.createDataFrame(data, schema=charges_schema)

    return factory


@pytest.fixture(scope="session")
def metering_point_period_factory(spark: SparkSession) -> Callable[..., DataFrame]:
    def factory(
        from_date: datetime,
        to_date: datetime,
        metering_point_id: str = DataframeDefaults.default_metering_point_id,
        metering_point_type: str = DataframeDefaults.default_metering_point_type,
        settlement_method: str = DataframeDefaults.default_settlement_method,
        grid_area: str = DataframeDefaults.default_grid_area,
        resolution: str = DataframeDefaults.default_metering_point_resolution,
        to_grid_area: str = DataframeDefaults.default_to_grid_area,
        from_grid_area: str = DataframeDefaults.default_from_grid_area,
        parent_metering_point_id: str = DataframeDefaults.default_parent_metering_point_id,
        energy_supplier_id: str = DataframeDefaults.default_energy_supplier_id,
        balance_responsible_id: str = DataframeDefaults.default_balance_responsible_id,
    ) -> DataFrame:
        data = [
            {
                Colname.metering_point_id: metering_point_id,
                Colname.metering_point_type: metering_point_type,
                Colname.calculation_type: DataframeDefaults.default_calculation_type,
                Colname.settlement_method: settlement_method,
                Colname.grid_area: grid_area,
                Colname.resolution: resolution,
                Colname.from_grid_area: from_grid_area,
                Colname.to_grid_area: to_grid_area,
                Colname.parent_metering_point_id: parent_metering_point_id,
                Colname.energy_supplier_id: energy_supplier_id,
                Colname.balance_responsible_id: balance_responsible_id,
                Colname.from_date: from_date,
                Colname.to_date: to_date,
            }
        ]

        return spark.createDataFrame(data, schema=metering_point_period_schema)

    return factory


@pytest.fixture(scope="session")
def time_series_factory(spark: SparkSession) -> Callable[..., DataFrame]:
    def factory(
        time: datetime,
        metering_point_id: str = DataframeDefaults.default_metering_point_id,
        quantity: Decimal = DataframeDefaults.default_quantity,
        ts_quality: str = DataframeDefaults.default_quality,
    ) -> DataFrame:
        data = [
            {
                Colname.metering_point_id: metering_point_id,
                Colname.quantity: quantity,
                Colname.quality: ts_quality,
                Colname.observation_time: time,
            }
        ]

        return spark.createDataFrame(data, schema=time_series_point_schema)

    return factory
