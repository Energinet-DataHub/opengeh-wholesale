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
    InputMeteringPointType,
    InputSettlementMethod,
    MeteringPointResolution,
)
from package.constants import Colname

DEFAULT_FROM_DATE = datetime(2022, 6, 8, 22, 0, 0)
DEFAULT_TO_DATE = datetime(2022, 6, 9, 22, 0, 0)
DEFAULT_GRID_AREA = "805"
DEFAULT_METERING_POINT_ID = "123456789012345678901234567"
DEFAULT_METERING_POINT_TYPE = InputMeteringPointType.CONSUMPTION
DEFAULT_SETTLEMENT_METHOD = InputSettlementMethod.FLEX
DEFAULT_RESOLUTION = MeteringPointResolution.HOUR
DEFAULT_FROM_GRID_AREA = None
DEFAULT_TO_GRID_AREA = None
DEFAULT_PARENT_METERING_POINT_ID = None
DEFAULT_ENERGY_SUPPLIER_ID = "9999999999999"
DEFAULT_BALANCE_RESPONSIBLE_ID = "1234567890123"


def create_row(
    metering_point_type: InputMeteringPointType = DEFAULT_METERING_POINT_TYPE,
    settlement_method: InputSettlementMethod = DEFAULT_SETTLEMENT_METHOD,
    from_date: datetime = DEFAULT_FROM_DATE,
    to_date: datetime = DEFAULT_TO_DATE,
    grid_area: str = DEFAULT_GRID_AREA,
    from_grid_area: str = DEFAULT_FROM_GRID_AREA,
    to_grid_area: str = DEFAULT_TO_GRID_AREA,
) -> Row:
    row = {
        Colname.metering_point_id: DEFAULT_METERING_POINT_ID,
        Colname.metering_point_type: metering_point_type.value,
        Colname.calculation_type: "foo",
        Colname.settlement_method: settlement_method.value,
        Colname.grid_area: grid_area,
        Colname.resolution: DEFAULT_RESOLUTION.value,
        Colname.from_grid_area: from_grid_area,
        Colname.to_grid_area: to_grid_area,
        Colname.parent_metering_point_id: DEFAULT_PARENT_METERING_POINT_ID,
        Colname.energy_supplier_id: DEFAULT_ENERGY_SUPPLIER_ID,
        Colname.balance_responsible_id: DEFAULT_BALANCE_RESPONSIBLE_ID,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
    }
    return Row(**row)


def create_dataframe(spark: SparkSession, data: None | Row | list[Row] = None) -> DataFrame:
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    return spark.createDataFrame(data, schema=metering_point_period_schema)
