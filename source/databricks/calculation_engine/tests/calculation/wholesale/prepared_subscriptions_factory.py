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

from package.calculation.preparation.data_structures.prepared_subscriptions import (
    PreparedSubscriptions,
    prepared_subscriptions_schema,
)
from package.codelists import (
    ChargeType,
    MeteringPointType,
    SettlementMethod,
    WholesaleResultResolution,
)
from package.constants import Colname


class DefaultValues:
    GRID_AREA = "543"
    CHARGE_CODE = "4000"
    CHARGE_OWNER = "001"
    CHARGE_TIME_HOUR_0 = datetime(2020, 1, 1, 0)
    CHARGE_PRICE = Decimal("2.000005")
    CHARGE_QUANTITY = 1
    ENERGY_SUPPLIER_ID = "1234567890123"
    METERING_POINT_ID = "123456789012345678901234567"
    METERING_POINT_TYPE = MeteringPointType.CONSUMPTION
    SETTLEMENT_METHOD = SettlementMethod.FLEX
    PERIOD_START_DATETIME = datetime(2019, 12, 31, 23)


def create_row(
    charge_key: str | None = None,
    charge_code: str = DefaultValues.CHARGE_CODE,
    charge_owner: str = DefaultValues.CHARGE_OWNER,
    charge_time: datetime = DefaultValues.CHARGE_TIME_HOUR_0,
    charge_price: Decimal | None = DefaultValues.CHARGE_PRICE,
    charge_quantity: int | None = DefaultValues.CHARGE_QUANTITY,
    energy_supplier_id: str = DefaultValues.ENERGY_SUPPLIER_ID,
    metering_point_type: MeteringPointType = DefaultValues.METERING_POINT_TYPE,
    settlement_method: SettlementMethod = DefaultValues.SETTLEMENT_METHOD,
    metering_point_id: str = DefaultValues.METERING_POINT_ID,
    grid_area: str = DefaultValues.GRID_AREA,
) -> Row:
    charge_type = ChargeType.SUBSCRIPTION.value
    row = {
        Colname.charge_key: charge_key or f"{charge_code}-{charge_type}-{charge_owner}",
        Colname.charge_type: charge_type,
        Colname.charge_owner: charge_owner,
        Colname.charge_code: charge_code,
        Colname.charge_time: charge_time,
        Colname.charge_price: charge_price,
        Colname.charge_tax: False,
        Colname.charge_quantity: charge_quantity,
        Colname.metering_point_type: metering_point_type.value,
        Colname.settlement_method: settlement_method.value,
        Colname.metering_point_id: metering_point_id,
        Colname.grid_area: grid_area,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.resolution: WholesaleResultResolution.DAY.value,
    }

    return Row(**row)


def create(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> PreparedSubscriptions:
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, prepared_subscriptions_schema)
    return PreparedSubscriptions(df)
