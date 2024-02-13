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
from decimal import Decimal

from pyspark.sql import DataFrame, Row, SparkSession
from pyspark.sql.types import TimestampType

from package.calculation_input.schemas import time_series_point_schema
from package.constants import Colname


class DefaultValues:
    METERING_POINT_ID = "123456789012345678901234567"
    QUANTITY = Decimal("1.123456")
    QUALITY: str = "missing"
    OBSERVATION_TIME = datetime(2019, 12, 31, 23)


def create_row(
    metering_point_id: str = DefaultValues.METERING_POINT_ID,
    quantity: Decimal = DefaultValues.QUANTITY,
    quality: str = DefaultValues.QUALITY,
    observation_time: TimestampType = DefaultValues.OBSERVATION_TIME,
) -> Row:
    row = {
        Colname.metering_point_id: metering_point_id,
        Colname.quantity: quantity,
        Colname.quality: quality,
        Colname.observation_time: observation_time,
    }

    return Row(**row)


def create_dataframe(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> DataFrame:
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    return spark.createDataFrame(data, schema=time_series_point_schema)
