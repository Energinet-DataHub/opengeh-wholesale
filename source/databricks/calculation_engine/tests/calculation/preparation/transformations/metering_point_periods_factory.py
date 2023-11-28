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

from pyspark.sql import DataFrame, Row, SparkSession

from package.calculation_input.schemas import (
    metering_point_period_schema,
)
from package.codelists import (
    MeteringPointType,
    SettlementMethod,
    MeteringPointResolution,
)
from package.constants import Colname

DEFAULT_METERING_POINT_ID = "123456789012345678901234567"
DEFAULT_METERING_POINT_TYPE = MeteringPointType.PRODUCTION
DEFAULT_SETTLEMENT_METHOD = SettlementMethod.FLEX
DEFAULT_GRID_AREA = "805"
DEFAULT_RESOLUTION = MeteringPointResolution.HOUR
DEFAULT_FROM_GRID_AREA = None
DEFAULT_TO_GRID_AREA = None
DEFAULT_PARENT_METERING_POINT_ID = None
DEFAULT_ENERGY_SUPPLIER_ID = "9999999999999"
DEFAULT_BALANCE_RESPONSIBLE_ID = "1234567890123"
DEFAULT_FROM_DATE = datetime(2020, 1, 1, 0, 0)
DEFAULT_TO_DATE = datetime(2020, 2, 1, 0, 0)


def create_row(
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    metering_point_type: MeteringPointType = DEFAULT_METERING_POINT_TYPE,
    settlement_method: SettlementMethod = DEFAULT_SETTLEMENT_METHOD,
    grid_area: str = DEFAULT_GRID_AREA,
    resolution: MeteringPointResolution = DEFAULT_RESOLUTION,
    from_grid_area: str | None = DEFAULT_FROM_GRID_AREA,
    to_grid_area: str | None = DEFAULT_TO_GRID_AREA,
    parent_metering_point_id: str | None = DEFAULT_PARENT_METERING_POINT_ID,
    energy_supplier_id: str | None = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str | None = DEFAULT_BALANCE_RESPONSIBLE_ID,
    from_date: datetime = DEFAULT_FROM_DATE,
    to_date: datetime | None = DEFAULT_TO_DATE,
) -> Row:
    calculation_type = None

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


def create(spark: SparkSession, data: None | Row | list[Row] = None) -> DataFrame:
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    return spark.createDataFrame(data, schema=metering_point_period_schema)
