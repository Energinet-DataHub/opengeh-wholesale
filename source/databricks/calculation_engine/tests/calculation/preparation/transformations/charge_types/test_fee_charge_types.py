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
from pyspark.sql import SparkSession, Row

from package.calculation.preparation.transformations import get_fee_charges
from package.calculation_input.schemas import metering_point_period_schema
from package.calculation.wholesale.schemas.charges_schema import charges_schema
import package.codelists as e
from package.constants import Colname

DEFAULT_GRID_AREA = "543"
DEFAULT_CHARGE_CODE = "4000"
DEFAULT_CHARGE_OWNER = "001"
DEFAULT_CHARGE_TAX = True
DEFAULT_CHARGE_TIME_HOUR_0 = datetime(2019, 12, 31, 23)
DEFAULT_CHARGE_PRICE = Decimal("2.000005")
DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"
DEFAULT_METERING_POINT_ID = "123456789012345678901234567"
DEFAULT_METERING_POINT_TYPE = e.MeteringPointType.CONSUMPTION
DEFAULT_SETTLEMENT_METHOD = e.SettlementMethod.FLEX


def _create_metering_point_row(
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    metering_point_type: e.MeteringPointType = DEFAULT_METERING_POINT_TYPE,
    calculation_type: str = "calculation_type",
    settlement_method: e.SettlementMethod = DEFAULT_SETTLEMENT_METHOD,
    grid_area: str = DEFAULT_GRID_AREA,
    resolution: e.MeteringPointResolution = e.MeteringPointResolution.HOUR,
    from_grid_area: str = "from_grid_area",
    to_grid_area: str = "to_grid_area",
    parent_metering_point_id: str = "parent_metering_point_id",
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str = "balance_responsible_id",
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


def _create_subscription_or_fee_charges_row(
    charge_type: e.ChargeType,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_tax: bool = DEFAULT_CHARGE_TAX,
    charge_time: datetime = DEFAULT_CHARGE_TIME_HOUR_0,
    from_date: datetime = datetime(2019, 12, 31, 23),
    to_date: datetime = datetime(2020, 1, 1, 0),
    charge_price: Decimal = DEFAULT_CHARGE_PRICE,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
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


def test__get_fee_charges__filters_on_fee_charge_type(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_rows = [_create_metering_point_row()]
    charges_rows = [
        _create_subscription_or_fee_charges_row(
            charge_type=e.ChargeType.FEE,
        ),
        _create_subscription_or_fee_charges_row(
            charge_type=e.ChargeType.SUBSCRIPTION,
        ),
    ]

    metering_point = spark.createDataFrame(
        metering_point_rows, metering_point_period_schema
    )
    charges = spark.createDataFrame(charges_rows, charges_schema)

    # Act
    actual_fee = get_fee_charges(charges, metering_point)

    # Assert
    assert actual_fee.collect()[0][Colname.charge_type] == e.ChargeType.FEE.value
