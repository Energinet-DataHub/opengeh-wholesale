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
from pyspark.sql import Row, SparkSession

from package.calculation.preparation.charge_link_metering_point_periods import (
    ChargeLinkMeteringPointPeriods,
    charge_link_metering_point_periods_schema,
)
from package.calculation.preparation.charge_period_prices import (
    charge_period_prices_schema,
    ChargePeriodPrices,
)
from package.codelists import ChargeType
from package.constants import Colname

import package.codelists as e


class DefaultValues:
    DEFAULT_GRID_AREA = "543"
    DEFAULT_CHARGE_TYPE = ChargeType.TARIFF
    DEFAULT_CHARGE_CODE = "4000"
    DEFAULT_CHARGE_OWNER = "001"
    DEFAULT_CHARGE_TAX = True
    DEFAULT_CHARGE_TIME_HOUR_0 = datetime(2019, 12, 31, 23)
    DEFAULT_CHARGE_PRICE = Decimal("2.000005")
    DEFAULT_CHARGE_QUANTITY = 1
    DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"
    DEFAULT_METERING_POINT_ID = "123456789012345678901234567"
    DEFAULT_METERING_POINT_TYPE = e.MeteringPointType.CONSUMPTION
    DEFAULT_SETTLEMENT_METHOD = e.SettlementMethod.FLEX
    DEFAULT_QUANTITY = Decimal("1.005")
    DEFAULT_QUALITY = e.ChargeQuality.CALCULATED
    DEFAULT_PERIOD_START_DATETIME = datetime(2019, 12, 31, 23)
    DEFAULT_BALANCE_RESPONSIBLE_ID = "1234567890123"
    DEFAULT_FROM_GRID_AREA = None
    DEFAULT_TO_GRID_AREA = None
    DEFAULT_FROM_DATE: datetime = datetime(2019, 12, 31, 23)
    DEFAULT_TO_DATE: datetime = datetime(2020, 1, 31, 23)
    DEFAULT_PARENT_METERING_POINT_ID = None
    DEFAULT_CALCULATION_TYPE = None


def create_time_series_row(
    metering_point_id: str = DefaultValues.DEFAULT_METERING_POINT_ID,
    quantity: Decimal = DefaultValues.DEFAULT_QUANTITY,
    quality: e.QuantityQuality = e.QuantityQuality.CALCULATED,
    observation_time: datetime = datetime(2019, 12, 31, 23),
) -> Row:
    row = {
        Colname.metering_point_id: metering_point_id,
        Colname.quantity: quantity,
        Colname.quality: quality.value,
        Colname.observation_time: observation_time,
    }
    return Row(**row)


def create_tariff_charge_period_prices_row(
    charge_code: str = DefaultValues.DEFAULT_CHARGE_CODE,
    charge_owner: str = DefaultValues.DEFAULT_CHARGE_OWNER,
    charge_tax: bool = DefaultValues.DEFAULT_CHARGE_TAX,
    resolution: e.ChargeResolution = e.ChargeResolution.HOUR,
    charge_time: datetime = DefaultValues.DEFAULT_CHARGE_TIME_HOUR_0,
    from_date: datetime = datetime(2019, 12, 31, 23),
    to_date: datetime = datetime(2020, 1, 1, 0),
    charge_price: Decimal = DefaultValues.DEFAULT_CHARGE_PRICE,
) -> Row:
    charge_key: str = f"{charge_code}-{charge_owner}-{e.ChargeType.TARIFF.value}"

    row = {
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_type: e.ChargeType.TARIFF.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_tax: charge_tax,
        Colname.resolution: resolution.value,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
        Colname.charge_price: charge_price,
        Colname.charge_time: charge_time,
    }

    return Row(**row)


def create_charge_link_metering_point_periods_row(
    charge_type: e.ChargeType = DefaultValues.DEFAULT_CHARGE_TYPE,
    charge_code: str = DefaultValues.DEFAULT_CHARGE_CODE,
    charge_owner: str = DefaultValues.DEFAULT_CHARGE_OWNER,
    metering_point_id: str = DefaultValues.DEFAULT_METERING_POINT_ID,
    charge_quantity: int = DefaultValues.DEFAULT_CHARGE_QUANTITY,
    metering_point_type: (
        e.MeteringPointType
    ) = DefaultValues.DEFAULT_METERING_POINT_TYPE,
    settlement_method: e.SettlementMethod = DefaultValues.DEFAULT_SETTLEMENT_METHOD,
    grid_area: str = DefaultValues.DEFAULT_GRID_AREA,
    energy_supplier_id: str | None = DefaultValues.DEFAULT_ENERGY_SUPPLIER_ID,
    from_date: datetime = DefaultValues.DEFAULT_FROM_DATE,
    to_date: datetime | None = DefaultValues.DEFAULT_TO_DATE,
) -> Row:
    charge_key: str = f"{charge_code}-{charge_owner}-{charge_type.value}"

    row = {
        Colname.charge_key: charge_key,
        Colname.metering_point_id: metering_point_id,
        Colname.charge_quantity: charge_quantity,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
        Colname.metering_point_type: metering_point_type.value,
        Colname.settlement_method: settlement_method.value,
        Colname.grid_area: grid_area,
        Colname.energy_supplier_id: energy_supplier_id,
    }

    return Row(**row)


def create_subscription_or_fee_charge_period_prices_row(
    charge_type: e.ChargeType,
    charge_code: str = DefaultValues.DEFAULT_CHARGE_CODE,
    charge_owner: str = DefaultValues.DEFAULT_CHARGE_OWNER,
    charge_tax: bool = DefaultValues.DEFAULT_CHARGE_TAX,
    charge_time: datetime = DefaultValues.DEFAULT_CHARGE_TIME_HOUR_0,
    from_date: datetime = datetime(2019, 12, 31, 23),
    to_date: datetime = datetime(2020, 1, 1, 0),
    charge_price: Decimal = DefaultValues.DEFAULT_CHARGE_PRICE,
) -> Row:
    charge_key: str = f"{charge_code}-{charge_owner}-{charge_type.value}"

    row = {
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_type: charge_type.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_tax: charge_tax,
        Colname.resolution: e.ChargeResolution.MONTH.value,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
        Colname.charge_price: charge_price,
        Colname.charge_time: charge_time,
    }
    return Row(**row)


def create_charge_period_prices(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> ChargePeriodPrices:
    if isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, charge_period_prices_schema)
    return ChargePeriodPrices(df)


def create_charge_link_metering_point_periods(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> ChargeLinkMeteringPointPeriods:
    if data is None:
        data = [create_charge_link_metering_point_periods_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, charge_link_metering_point_periods_schema)
    return ChargeLinkMeteringPointPeriods(df)
