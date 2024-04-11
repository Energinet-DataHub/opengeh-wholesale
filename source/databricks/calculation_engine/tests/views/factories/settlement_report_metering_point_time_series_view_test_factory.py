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

from package.codelists import (
    InputMeteringPointType,
    MeteringPointResolution,
)
from views.factories.metering_point_time_series_colname import (
    MeteringPointTimeSeriesColname,
)
from views.settlement_reports.schemas.metering_point_time_series_schema import (
    metering_point_time_series_schema,
)


class SettlementReportMeteringPointTimeSeriesViewTestFactory:
    CALCULATION_ID = "295b6872-cc24-483c-bf0a-a33f93207c20"
    METERING_POINT_ID = "123456789012345678901234567"
    METERING_POINT_TYPE = InputMeteringPointType.PRODUCTION
    RESOLUTION = MeteringPointResolution.HOUR
    GRID_AREA = "805"
    ENERGY_SUPPLIER_ID = "9999999999999"
    OBSERVATION_DAY = datetime(2019, 12, 31, 23, 0, 0)
    QUANTITIES = ""

    def __init__(self, spark: SparkSession):
        self.spark = spark

    @staticmethod
    def create_row(
        calculation_id: str = CALCULATION_ID,
        metering_point_id: str = METERING_POINT_ID,
        metering_point_type: InputMeteringPointType = METERING_POINT_TYPE,
        resolution: MeteringPointResolution = RESOLUTION,
        grid_area: str = GRID_AREA,
        energy_supplier: str = ENERGY_SUPPLIER_ID,
        observation_day: datetime = OBSERVATION_DAY,
        quantities: str = QUANTITIES,
    ) -> Row:
        row = {
            MeteringPointTimeSeriesColname.calculation_id: calculation_id,
            MeteringPointTimeSeriesColname.metering_point_id: metering_point_id,
            MeteringPointTimeSeriesColname.metering_point_type: metering_point_type,
            MeteringPointTimeSeriesColname.resolution: resolution,
            MeteringPointTimeSeriesColname.grid_area: grid_area,
            MeteringPointTimeSeriesColname.energy_supplier_id: energy_supplier,
            MeteringPointTimeSeriesColname.observation_day: observation_day,
            MeteringPointTimeSeriesColname.quantities: quantities,
        }

        return Row(**row)

    def create_dataframe(self, data: None | Row | list[Row] = None) -> DataFrame:
        if data is None:
            data = [self.create_row()]
        elif isinstance(data, Row):
            data = [data]
        return self.spark.createDataFrame(
            data, schema=metering_point_time_series_schema
        )
