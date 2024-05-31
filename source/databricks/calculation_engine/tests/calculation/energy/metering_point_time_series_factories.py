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

import datetime
from decimal import Decimal

from pyspark.sql import Row, SparkSession

from package.calculation.preparation.data_structures.metering_point_time_series import (
    MeteringPointTimeSeries,
    metering_point_time_series_schema,
)
from package.codelists import MeteringPointType, QuantityQuality, SettlementMethod
from package.constants import Colname

DEFAULT_GRID_AREA = "100"
DEFAULT_NEIGHBOUR_GRID_AREA = "200"
DEFAULT_OBSERVATION_TIME = datetime.datetime.now()
DEFAULT_QUANTITY = Decimal("999.123456")
DEFAULT_QUALITY = QuantityQuality.MEASURED
DEFAULT_METERING_POINT_ID = "1234567890123"
DEFAULT_METERING_POINT_TYPE = MeteringPointType.CONSUMPTION
DEFAULT_SETTLEMENT_METHOD = SettlementMethod.NON_PROFILED
DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"
DEFAULT_BALANCE_RESPONSIBLE_ID = "9999999999999"


def create_row(
    grid_area: str = DEFAULT_GRID_AREA,
    to_grid_area: str | None = None,
    from_grid_area: str | None = None,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    metering_point_type: MeteringPointType = DEFAULT_METERING_POINT_TYPE,
    observation_time: datetime.datetime = DEFAULT_OBSERVATION_TIME,
    quantity: int | Decimal = DEFAULT_QUANTITY,
    quality: QuantityQuality = DEFAULT_QUALITY,
    energy_supplier_id: str | None = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str | None = DEFAULT_BALANCE_RESPONSIBLE_ID,
    settlement_method: SettlementMethod | None = DEFAULT_SETTLEMENT_METHOD,
) -> Row:
    if isinstance(quantity, int):
        quantity = Decimal(quantity)
    if settlement_method is not None:
        settlement_method = settlement_method.value

    row = {
        Colname.grid_area_code: grid_area,
        Colname.to_grid_area_code: to_grid_area,
        Colname.from_grid_area_code: from_grid_area,
        Colname.metering_point_id: metering_point_id,
        Colname.metering_point_type: metering_point_type.value,
        Colname.quantity: quantity,
        Colname.quality: quality.value,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.balance_responsible_id: balance_responsible_id,
        Colname.settlement_method: settlement_method,
        Colname.observation_time: observation_time,
    }

    return Row(**row)


def create_exchange_row(
    grid_area: str = DEFAULT_GRID_AREA,
    to_grid_area: str | None = None,
    from_grid_area: str | None = None,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    observation_time: datetime.datetime = DEFAULT_OBSERVATION_TIME,
    quantity: int | Decimal = DEFAULT_QUANTITY,
    quality: QuantityQuality = DEFAULT_QUALITY,
) -> Row:
    return create_row(
        grid_area=grid_area,
        to_grid_area=to_grid_area,
        from_grid_area=from_grid_area,
        metering_point_id=metering_point_id,
        metering_point_type=MeteringPointType.EXCHANGE,
        observation_time=observation_time,
        quantity=quantity,
        quality=quality,
    )


def create_from_row(
    grid_area: str = DEFAULT_GRID_AREA,
    to_grid_area: str = DEFAULT_NEIGHBOUR_GRID_AREA,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    observation_time: datetime.datetime = DEFAULT_OBSERVATION_TIME,
    quantity: int | Decimal = DEFAULT_QUANTITY,
    quality: QuantityQuality = DEFAULT_QUALITY,
) -> Row:
    """Create a row representing exchange leaving the (default) grid area."""
    return create_exchange_row(
        grid_area=grid_area,
        to_grid_area=to_grid_area,
        from_grid_area=grid_area,
        metering_point_id=metering_point_id,
        observation_time=observation_time,
        quantity=quantity,
        quality=quality,
    )


def create_to_row(
    grid_area: str = DEFAULT_GRID_AREA,
    from_grid_area: str = DEFAULT_NEIGHBOUR_GRID_AREA,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    observation_time: datetime = DEFAULT_OBSERVATION_TIME,
    quantity: int | Decimal = DEFAULT_QUANTITY,
    quality: QuantityQuality = DEFAULT_QUALITY,
) -> Row:
    """Create a row representing exchange entering the grid area."""
    return create_exchange_row(
        grid_area=grid_area,
        to_grid_area=grid_area,
        from_grid_area=from_grid_area,
        metering_point_id=metering_point_id,
        observation_time=observation_time,
        quantity=quantity,
        quality=quality,
    )


def create(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> MeteringPointTimeSeries:
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, schema=metering_point_time_series_schema)
    return MeteringPointTimeSeries(df)
