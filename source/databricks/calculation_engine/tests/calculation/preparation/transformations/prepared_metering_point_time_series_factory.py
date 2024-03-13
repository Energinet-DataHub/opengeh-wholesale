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

from pyspark.sql import Row, SparkSession

from package.calculation.preparation.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
    prepared_metering_point_time_series_schema,
)
from package.codelists import (
    MeteringPointType,
    SettlementMethod,
    MeteringPointResolution,
    QuantityQuality,
)
from package.constants import Colname

DEFAULT_METERING_POINT_ID = "123456789012345678901234567"
DEFAULT_METERING_POINT_TYPE = MeteringPointType.PRODUCTION
DEFAULT_SETTLEMENT_METHOD = SettlementMethod.FLEX
DEFAULT_GRID_AREA = "805"
DEFAULT_RESOLUTION = MeteringPointResolution.HOUR
DEFAULT_ENERGY_SUPPLIER_ID = "9999999999999"
DEFAULT_BALANCE_RESPONSIBLE_ID = "1234567890123"
DEFAULT_OBSERVATION_TIME = datetime(2020, 1, 1, 0, 0)
DEFAULT_QUANTITY = Decimal(1.0)


def create_row(
    grid_area: str = DEFAULT_GRID_AREA,
    to_grid_area: str | None = None,
    from_grid_area: str | None = None,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    metering_point_type: MeteringPointType = DEFAULT_METERING_POINT_TYPE,
    resolution: MeteringPointResolution = DEFAULT_RESOLUTION,
    observation_time: datetime = DEFAULT_OBSERVATION_TIME,
    quantity: Decimal = DEFAULT_QUANTITY,
    quality: QuantityQuality = QuantityQuality.MEASURED,
    energy_supplier_id: str | None = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str | None = DEFAULT_BALANCE_RESPONSIBLE_ID,
    settlement_method: SettlementMethod | None = DEFAULT_SETTLEMENT_METHOD,
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
        Colname.quality: quality.value,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.balance_responsible_id: balance_responsible_id,
        Colname.settlement_method: (
            settlement_method.value if settlement_method else None
        ),
    }

    return Row(**row)


def create(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> PreparedMeteringPointTimeSeries:
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]

    df = spark.createDataFrame(data, schema=prepared_metering_point_time_series_schema)
    return PreparedMeteringPointTimeSeries(df)
