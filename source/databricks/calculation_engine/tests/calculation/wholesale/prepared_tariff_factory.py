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
from package.calculation.preparation.prepared_tariffs import (
    PreparedTariffs,
    prepared_tariffs_schema,
)
from package.codelists import (
    ChargeType,
    ChargeResolution,
    MeteringPointType,
    SettlementMethod,
    ChargeQuality,
)
from package.constants import Colname

DEFAULT_GRID_AREA = "543"
DEFAULT_CHARGE_CODE = "4000"
DEFAULT_CHARGE_OWNER = "001"
DEFAULT_CHARGE_TAX = True
DEFAULT_CHARGE_TIME_HOUR_0 = datetime(2020, 1, 1, 0)
DEFAULT_CHARGE_PRICE = Decimal("2.000005")
DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"
DEFAULT_METERING_POINT_ID = "123456789012345678901234567"
DEFAULT_METERING_POINT_TYPE = MeteringPointType.CONSUMPTION
DEFAULT_SETTLEMENT_METHOD = SettlementMethod.FLEX
DEFAULT_QUANTITY = Decimal("1.005")
DEFAULT_QUALITY = ChargeQuality.CALCULATED
DEFAULT_PERIOD_START_DATETIME = datetime(2019, 12, 31, 23)


def create_prepared_tariffs_row(
    charge_key: str | None = None,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    resolution: ChargeResolution = ChargeResolution.HOUR,
    charge_time: datetime = DEFAULT_CHARGE_TIME_HOUR_0,
    charge_price: Decimal | None = DEFAULT_CHARGE_PRICE,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    metering_point_type: MeteringPointType = DEFAULT_METERING_POINT_TYPE,
    settlement_method: SettlementMethod | None = DEFAULT_SETTLEMENT_METHOD,
    grid_area: str = DEFAULT_GRID_AREA,
    quantity: Decimal = DEFAULT_QUANTITY,
    quality: ChargeQuality = DEFAULT_QUALITY,
) -> Row:
    row = {
        Colname.charge_key: charge_key
        or f"{charge_code}-{ChargeType.TARIFF.value}-{charge_owner}",
        Colname.charge_code: charge_code,
        Colname.charge_type: ChargeType.TARIFF.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_tax: DEFAULT_CHARGE_TAX,
        Colname.resolution: resolution.value,
        Colname.charge_time: charge_time,
        Colname.charge_price: charge_price,
        Colname.metering_point_id: metering_point_id,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.metering_point_type: metering_point_type.value,
        Colname.settlement_method: (
            settlement_method.value if settlement_method else None
        ),
        Colname.grid_area: grid_area,
        Colname.quantity: quantity,
        Colname.qualities: [quality.value],
    }

    return Row(**row)


def create_prepared_tariffs(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> PreparedTariffs:
    if data is None:
        data = [create_prepared_tariffs_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, prepared_tariffs_schema)
    return PreparedTariffs(df)
