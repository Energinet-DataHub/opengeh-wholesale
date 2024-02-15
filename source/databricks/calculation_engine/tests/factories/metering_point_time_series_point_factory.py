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

from pyspark import Row
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import TimestampType

from package.calculation.energy.hour_to_quarter import metering_point_time_series_schema
from package.codelists import (
    InputMeteringPointType,
    QuantityQuality,
    MeteringPointResolution,
    InputSettlementMethod,
)
from package.constants import Colname


class DefaultValues:
    METERING_POINT_ID = "123456789012345678901234567"
    METERING_POINT_TYPE = InputMeteringPointType.PRODUCTION
    GRID_AREA = "805"
    RESOLUTION = MeteringPointResolution.HOUR
    ENERGY_SUPPLIER_ID = "9999999999999"
    BALANCE_RESPONSIBLE_ID = "1234567890123"
    QUANTITY = Decimal("1")
    OBSERVATION_TIME = datetime(2019, 12, 31, 23)
    QUANTITY_QUALITY = QuantityQuality.ESTIMATED
    FROM_GRID_AREA = "805"
    TO_GRID_AREA = "805"
    SETTLEMENT_METHOD = InputSettlementMethod.FLEX


def create_row(
    metering_point_id: str = DefaultValues.METERING_POINT_ID,
    metering_point_type: InputMeteringPointType = DefaultValues.METERING_POINT_TYPE,
    grid_area: str = DefaultValues.GRID_AREA,
    resolution: MeteringPointResolution = DefaultValues.RESOLUTION,
    energy_supplier_id: str = DefaultValues.ENERGY_SUPPLIER_ID,
    balance_responsible_id: str = DefaultValues.BALANCE_RESPONSIBLE_ID,
    quantity: Decimal = DefaultValues.QUANTITY,
    observation_time: TimestampType = DefaultValues.OBSERVATION_TIME,
    quantity_quality: QuantityQuality = DefaultValues.QUANTITY_QUALITY,
    from_grid_area: str = DefaultValues.FROM_GRID_AREA,
    to_grid_area: str = DefaultValues.TO_GRID_AREA,
    settlement_method: InputSettlementMethod = DefaultValues.SETTLEMENT_METHOD,
) -> Row:
    row = {
        Colname.grid_area: grid_area,
        Colname.to_grid_area: to_grid_area,
        Colname.from_grid_area: from_grid_area,
        Colname.metering_point_id: metering_point_id,
        Colname.metering_point_type: metering_point_type.value,
        Colname.resolution: resolution.value,
        Colname.observation_time: observation_time,
        Colname.quantity: quantity,
        Colname.quality: quantity_quality.value,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.balance_responsible_id: balance_responsible_id,
        Colname.settlement_method: settlement_method.value,
    }

    return Row(**row)


def create_dataframe(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> DataFrame:
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    return spark.createDataFrame(data, schema=metering_point_time_series_schema)
