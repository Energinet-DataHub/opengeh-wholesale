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

from package.calculation.wholesale.data_structures import (
    TotalMonthlyAmount,
)
from package.calculation.wholesale.data_structures.total_monthly_amount import (
    total_monthly_amount_schema,
)
from package.constants import Colname


class DefaultValues:
    GRID_AREA = "543"
    ENERGY_SUPPLIER_ID = "1234567890123"
    CHARGE_TIME = datetime.datetime.now()
    TOTAL_AMOUNT = None
    CHARGE_OWNER = "001"


def create_row(
    grid_area: str = DefaultValues.GRID_AREA,
    energy_supplier_id: str = DefaultValues.ENERGY_SUPPLIER_ID,
    charge_time: datetime = DefaultValues.CHARGE_TIME,
    total_amount: int | Decimal | None = DefaultValues.TOTAL_AMOUNT,
    charge_owner: str = DefaultValues.CHARGE_OWNER,
) -> Row:

    row = {
        Colname.grid_area_code: grid_area,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.charge_time: charge_time,
        Colname.total_amount: total_amount,
        Colname.charge_owner: charge_owner,
    }

    return Row(**row)


def create(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> TotalMonthlyAmount:
    """If data is None, a single row with default values is created."""
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, schema=total_monthly_amount_schema)
    return TotalMonthlyAmount(df)
