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

from package.calculation.basis_data.settlement_views.schemas.metering_point_period_schema import (
    metering_point_period_schema,
)
from package.codelists import (
    InputMeteringPointType,
    InputSettlementMethod,
)
from package.constants import MeteringPointPeriodColname


class SettlementReportMeteringPointPeriodsViewTestFactory:
    CALCULATION_ID = "295b6872-cc24-483c-bf0a-a33f93207c20"
    METERING_POINT_ID = "123456789012345678901234567"
    FROM_DATE = datetime(2019, 12, 31, 23, 0, 0)
    TO_DATE = None
    GRID_AREA = "805"
    FROM_GRID_AREA = None
    TO_GRID_AREA = None
    METERING_POINT_TYPE = InputMeteringPointType.PRODUCTION
    SETTLEMENT_METHOD = InputSettlementMethod.FLEX
    ENERGY_SUPPLIER_ID = "9999999999999"

    def __init__(self, spark: SparkSession):
        self.spark = spark

    @staticmethod
    def create_row(
        calculation_id: str = CALCULATION_ID,
        metering_point_id: str = METERING_POINT_ID,
        from_date: datetime = FROM_DATE,
        to_date: datetime | None = TO_DATE,
        grid_area: str = GRID_AREA,
        from_grid_area: str | None = FROM_GRID_AREA,
        to_grid_area: str | None = TO_GRID_AREA,
        metering_point_type: InputMeteringPointType = METERING_POINT_TYPE,
        settlement_method: InputSettlementMethod | None = SETTLEMENT_METHOD,
        energy_supplier: str = ENERGY_SUPPLIER_ID,
    ) -> Row:
        row = {
            MeteringPointPeriodColname.calculation_id: calculation_id,
            MeteringPointPeriodColname.metering_point_id: metering_point_id,
            MeteringPointPeriodColname.from_date: from_date,
            MeteringPointPeriodColname.to_date: to_date,
            MeteringPointPeriodColname.grid_area: grid_area,
            MeteringPointPeriodColname.from_grid_area: from_grid_area,
            MeteringPointPeriodColname.to_grid_area: to_grid_area,
            MeteringPointPeriodColname.metering_point_type: metering_point_type,
            MeteringPointPeriodColname.settlement_method: settlement_method,
            MeteringPointPeriodColname.energy_supplier_id: energy_supplier,
        }

        return Row(**row)

    def create_dataframe(self, data: None | Row | list[Row] = None) -> DataFrame:
        if data is None:
            data = [self.create_row()]
        elif isinstance(data, Row):
            data = [data]
        return self.spark.createDataFrame(data, schema=metering_point_period_schema)
