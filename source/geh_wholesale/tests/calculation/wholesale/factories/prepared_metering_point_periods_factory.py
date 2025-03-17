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

from package.databases.migrations_wholesale.schemas import (
    metering_point_periods_schema,
)
from package.codelists import (
    MeteringPointResolution,
    SettlementMethod,
    MeteringPointType,
    CalculationType,
)
from package.constants import Colname


class DefaultValues:
    FROM_DATE = datetime(2022, 6, 8, 22, 0, 0)
    TO_DATE = datetime(2022, 6, 9, 22, 0, 0)
    GRID_AREA_CODE = "805"
    METERING_POINT_ID = "123456789012345678901234567"
    METERING_POINT_TYPE = MeteringPointType.CONSUMPTION
    CALCULATION_TYPE = CalculationType.WHOLESALE_FIXING
    SETTLEMENT_METHOD = SettlementMethod.FLEX
    RESOLUTION = MeteringPointResolution.HOUR
    FROM_GRID_AREA = None
    TO_GRID_AREA = None
    PARENT_METERING_POINT_ID = None
    ENERGY_SUPPLIER_ID = "9999999999999"
    BALANCE_RESPONSIBLE_ID = "1234567890123"


def create_row(
    metering_point_id: str = DefaultValues.METERING_POINT_ID,
    metering_point_type: MeteringPointType = DefaultValues.METERING_POINT_TYPE,
    calculation_type: CalculationType = DefaultValues.CALCULATION_TYPE,
    settlement_method: SettlementMethod = DefaultValues.SETTLEMENT_METHOD,
    grid_area_code: str = DefaultValues.GRID_AREA_CODE,
    resolution: MeteringPointResolution = DefaultValues.RESOLUTION,
    from_date: datetime = DefaultValues.FROM_DATE,
    to_date: datetime = DefaultValues.TO_DATE,
    from_grid_area_code: str | None = DefaultValues.FROM_GRID_AREA,
    to_grid_area_code: str | None = DefaultValues.TO_GRID_AREA,
) -> Row:
    row = {
        Colname.metering_point_id: metering_point_id,
        Colname.metering_point_type: metering_point_type.value,
        Colname.calculation_type: calculation_type.value,
        Colname.settlement_method: settlement_method.value,
        Colname.grid_area_code: grid_area_code,
        Colname.resolution: resolution.value,
        Colname.from_grid_area_code: from_grid_area_code,
        Colname.to_grid_area_code: to_grid_area_code,
        Colname.parent_metering_point_id: DefaultValues.PARENT_METERING_POINT_ID,
        Colname.energy_supplier_id: DefaultValues.ENERGY_SUPPLIER_ID,
        Colname.balance_responsible_party_id: DefaultValues.BALANCE_RESPONSIBLE_ID,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
    }
    return Row(**row)


def create(spark: SparkSession, data: None | Row | list[Row] = None) -> DataFrame:
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    return spark.createDataFrame(data, schema=metering_point_periods_schema)
