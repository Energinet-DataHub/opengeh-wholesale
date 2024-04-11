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

from package.calculation.basis_data.schemas import (
    time_series_point_schema,
)
from package.codelists import QuantityQuality
from package.constants import Colname


class BasisDataTimeSeriesPointsFactory:
    CALCULATION_ID = "c6fe53cb-de76-4d0f-9d5c-cd73e46e4be9"
    METERING_POINT_ID = "123456789012345678"
    QUANTITY = "1.01"
    QUALITY = QuantityQuality.CALCULATED
    OBSERVATION_TIME = datetime(2019, 12, 31, 23, 0, 0)

    def __init__(self, spark: SparkSession):
        self.spark = spark

    @staticmethod
    def create_row(
        calculation_id: str = CALCULATION_ID,
        metering_point_id: str = METERING_POINT_ID,
        quantity: str = QUANTITY,
        quality: QuantityQuality = QUALITY,
        observation_time: datetime = OBSERVATION_TIME,
    ) -> Row:
        row = {
            Colname.calculation_id: calculation_id,
            Colname.metering_point_id: metering_point_id,
            Colname.quantity: quality,
            Colname.quality: quantity,
            Colname.observation_time: observation_time,
        }

        return Row(**row)

    def create_dataframe(self, data: None | Row | list[Row] = None) -> DataFrame:
        if data is None:
            data = [self.create_row()]
        elif isinstance(data, Row):
            data = [data]
        return self.spark.createDataFrame(data, schema=time_series_point_schema)
