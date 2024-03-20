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

from pyspark.sql import Row, SparkSession, DataFrame

from package.calculation.input.schemas import metering_point_period_schema
from package.codelists import (
    InputMeteringPointType,
    InputSettlementMethod,
    MeteringPointResolution,
    CalculationType,
)
from package.constants import Colname


class InputMeteringPointPeriodsTestFactory:
    METERING_POINT_ID = "123456789012345678901234567"
    METERING_POINT_TYPE = InputMeteringPointType.PRODUCTION
    CALCULATION_TYPE = None
    FROM_DATE = datetime(2019, 12, 31, 23, 0, 0)
    TO_DATE = None
    GRID_AREA = "805"
    SETTLEMENT_METHOD = InputSettlementMethod.FLEX
    RESOLUTION = MeteringPointResolution.HOUR
    FROM_GRID_AREA = None
    TO_GRID_AREA = None
    PARENT_METERING_POINT_ID = None
    ENERGY_SUPPLIER_ID = "9999999999999"
    BALANCE_RESPONSIBLE_ID = "1234567890123"

    def __init__(self, spark: SparkSession):
        self.spark = spark

    @staticmethod
    def create_row(
        metering_point_id: str = METERING_POINT_ID,
        metering_point_type: InputMeteringPointType = METERING_POINT_TYPE,
        calculation_type: CalculationType | None = CALCULATION_TYPE,
        settlement_method: InputSettlementMethod = SETTLEMENT_METHOD,
        grid_area: str = GRID_AREA,
        resolution: MeteringPointResolution = RESOLUTION,
        from_grid_area: str | None = FROM_GRID_AREA,
        to_grid_area: str | None = TO_GRID_AREA,
        parent_metering_point_id: str | None = PARENT_METERING_POINT_ID,
        energy_supplier: str = ENERGY_SUPPLIER_ID,
        balance_responsible_id: str = BALANCE_RESPONSIBLE_ID,
        from_date: datetime = FROM_DATE,
        to_date: datetime | None = TO_DATE,
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
            Colname.energy_supplier_id: energy_supplier,
            Colname.balance_responsible_id: balance_responsible_id,
            Colname.from_date: from_date,
            Colname.to_date: to_date,
        }

        return Row(**row)

    def create_dataframe(self, data: None | Row | list[Row] = None) -> DataFrame:
        if data is None:
            data = [self.create_row()]
        elif isinstance(data, Row):
            data = [data]
        return self.spark.createDataFrame(data, schema=metering_point_period_schema)
