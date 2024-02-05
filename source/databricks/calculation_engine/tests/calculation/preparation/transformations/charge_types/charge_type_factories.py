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
from pyspark.sql import Row

from calculation.preparation.transformations.default_test_values import (
    DefaultTestValues,
)
from package.constants import Colname

import package.codelists as e


def create_metering_point_row(
    metering_point_id: str = DefaultTestValues.DEFAULT_METERING_POINT_ID,
    metering_point_type: e.MeteringPointType = DefaultTestValues.DEFAULT_METERING_POINT_TYPE,
    calculation_type: str = DefaultTestValues.DEFAULT_CALCULATION_TYPE,
    settlement_method: e.SettlementMethod = DefaultTestValues.DEFAULT_SETTLEMENT_METHOD,
    grid_area: str = DefaultTestValues.DEFAULT_GRID_AREA,
    resolution: e.MeteringPointResolution = e.MeteringPointResolution.HOUR,
    from_grid_area: str = DefaultTestValues.DEFAULT_TO_GRID_AREA,
    to_grid_area: str = DefaultTestValues.DEFAULT_TO_GRID_AREA,
    parent_metering_point_id: str = DefaultTestValues.DEFAULT_PARENT_METERING_POINT_ID,
    energy_supplier_id: str = DefaultTestValues.DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str = DefaultTestValues.DEFAULT_BALANCE_RESPONSIBLE_ID,
    from_date: datetime = datetime(2019, 12, 31, 23),
    to_date: datetime = datetime(2020, 1, 31, 23),
) -> Row:
    row = {
        Colname.metering_point_id: metering_point_id,
        Colname.metering_point_type: metering_point_type.value,
        Colname.calculation_type: calculation_type,
        Colname.settlement_method: settlement_method.value,
        Colname.grid_area: grid_area,
        Colname.resolution: resolution.value,
        Colname.from_grid_area: from_grid_area,
        Colname.to_grid_area: to_grid_area,
        Colname.parent_metering_point_id: parent_metering_point_id,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.balance_responsible_id: balance_responsible_id,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
    }
    return Row(**row)


def create_time_series_row(
    metering_point_id: str = DefaultTestValues.DEFAULT_METERING_POINT_ID,
    quantity: Decimal = DefaultTestValues.DEFAULT_QUANTITY,
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


def create_tariff_charges_row(
    charge_code: str = DefaultTestValues.DEFAULT_CHARGE_CODE,
    charge_owner: str = DefaultTestValues.DEFAULT_CHARGE_OWNER,
    charge_tax: bool = DefaultTestValues.DEFAULT_CHARGE_TAX,
    resolution: e.ChargeResolution = e.ChargeResolution.HOUR,
    charge_time: datetime = DefaultTestValues.DEFAULT_CHARGE_TIME_HOUR_0,
    from_date: datetime = datetime(2019, 12, 31, 23),
    to_date: datetime = datetime(2020, 1, 1, 0),
    charge_price: Decimal = DefaultTestValues.DEFAULT_CHARGE_PRICE,
    metering_point_id: str = DefaultTestValues.DEFAULT_METERING_POINT_ID,
) -> Row:
    charge_key: str = f"{charge_code}-{charge_owner}-{e.ChargeType.TARIFF.value}"

    row = {
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_type: e.ChargeType.TARIFF.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_tax: charge_tax,
        Colname.resolution: resolution.value,
        Colname.charge_time: charge_time,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
        Colname.charge_price: charge_price,
        Colname.metering_point_id: metering_point_id,
    }
    return Row(**row)


def create_subscription_or_fee_charges_row(
    charge_type: e.ChargeType,
    charge_code: str = DefaultTestValues.DEFAULT_CHARGE_CODE,
    charge_owner: str = DefaultTestValues.DEFAULT_CHARGE_OWNER,
    charge_tax: bool = DefaultTestValues.DEFAULT_CHARGE_TAX,
    charge_time: datetime = DefaultTestValues.DEFAULT_CHARGE_TIME_HOUR_0,
    from_date: datetime = datetime(2019, 12, 31, 23),
    to_date: datetime = datetime(2020, 1, 1, 0),
    charge_price: Decimal = DefaultTestValues.DEFAULT_CHARGE_PRICE,
    metering_point_id: str = DefaultTestValues.DEFAULT_METERING_POINT_ID,
) -> Row:
    charge_key: str = f"{charge_code}-{charge_owner}-{charge_type.value}"

    row = {
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_type: charge_type.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_tax: charge_tax,
        Colname.resolution: e.ChargeResolution.MONTH.value,
        Colname.charge_time: charge_time,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
        Colname.charge_price: charge_price,
        Colname.metering_point_id: metering_point_id,
    }
    return Row(**row)
