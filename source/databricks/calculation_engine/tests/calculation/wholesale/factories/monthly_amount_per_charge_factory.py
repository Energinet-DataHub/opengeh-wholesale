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

import datetime
from decimal import Decimal

from pyspark.sql import Row, SparkSession

from package.calculation.wholesale.data_structures import MonthlyAmountPerCharge
from package.calculation.wholesale.data_structures.monthly_amount_per_charge import (
    monthly_amount_per_charge_schema,
)
from package.codelists import (
    ChargeType,
)
from package.constants import Colname


def create_row(
    grid_area: str = "543",
    energy_supplier_id: str = "1234567890123",
    unit: str = "kWh",
    charge_time: datetime = datetime.datetime.now(),
    total_amount: int | Decimal | None = None,
    charge_tax: bool = True,
    charge_code: str = "4000",
    charge_type: ChargeType = ChargeType.TARIFF,
    charge_owner: str = "001",
) -> Row:

    row = {
        Colname.grid_area_code: grid_area,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.unit: unit,
        Colname.charge_time: charge_time,
        Colname.total_amount: total_amount,
        Colname.charge_tax: charge_tax,
        Colname.charge_code: charge_code,
        Colname.charge_type: charge_type.value,
        Colname.charge_owner: charge_owner,
    }

    return Row(**row)


def create(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> MonthlyAmountPerCharge:
    """If data is None, a single row with default values is created."""
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, schema=monthly_amount_per_charge_schema)
    return MonthlyAmountPerCharge(df)
