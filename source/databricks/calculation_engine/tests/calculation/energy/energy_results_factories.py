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

from package.calculation.energy.energy_results import (
    EnergyResults,
    energy_results_schema,
)
from package.codelists import MeteringPointType, QuantityQuality, SettlementMethod
from package.constants import Colname

DEFAULT_GRID_AREA = "100"
DEFAULT_FROM_GRID_AREA = "200"
DEFAULT_TO_GRID_AREA = "300"
DEFAULT_OBSERVATION_TIME = datetime.datetime.now()
DEFAULT_SUM_QUANTITY = Decimal("999.123456")
DEFAULT_QUALITIES = [QuantityQuality.MEASURED]
DEFAULT_METERING_POINT_TYPE = MeteringPointType.CONSUMPTION
DEFAULT_SETTLEMENT_METHOD = SettlementMethod.NON_PROFILED
DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"
DEFAULT_BALANCE_RESPONSIBLE_ID = "9999999999999"


def create_row(
    grid_area: str = DEFAULT_GRID_AREA,
    from_grid_area: str | None = DEFAULT_FROM_GRID_AREA,
    to_grid_area: str | None = DEFAULT_TO_GRID_AREA,
    observation_time: datetime = DEFAULT_OBSERVATION_TIME,
    sum_quantity: int | Decimal = DEFAULT_SUM_QUANTITY,
    qualities: None | QuantityQuality | list[QuantityQuality] = None,
    energy_supplier_id: str | None = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str | None = DEFAULT_BALANCE_RESPONSIBLE_ID,
    metering_point_id: str | None = None,
) -> Row:
    if isinstance(sum_quantity, int):
        sum_quantity = Decimal(sum_quantity)

    if qualities is None:
        qualities = DEFAULT_QUALITIES
    elif isinstance(qualities, QuantityQuality):
        qualities = [qualities]
    qualities = [q.value for q in qualities]

    row = {
        Colname.grid_area: grid_area,
        Colname.from_grid_area: from_grid_area,
        Colname.to_grid_area: to_grid_area,
        Colname.balance_responsible_id: balance_responsible_id,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.time_window: {
            Colname.start: observation_time,
            Colname.end: observation_time + datetime.timedelta(minutes=15),
        },
        Colname.sum_quantity: sum_quantity,
        Colname.qualities: qualities,
        Colname.metering_point_type: metering_point_id,
    }

    return Row(**row)


def create(spark: SparkSession, data: None | Row | list[Row] = None) -> EnergyResults:
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, schema=energy_results_schema)
    return EnergyResults(df)
