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

from package.calculation.preparation.quarterly_metering_point_time_series import (
    QuarterlyMeteringPointTimeSeries,
    _quarterly_metering_point_time_series_schema,
)
from package.codelists import MeteringPointType, QuantityQuality
from package.constants import Colname

DEFAULT_GRID_AREA = "100"
DEFAULT_NEIGHBOUR_GRID_AREA = "200"
DEFAULT_OBSERVATION_TIME = datetime.datetime.now()
DEFAULT_QUANTITY = Decimal("999.123456")
DEFAULT_QUALITY = QuantityQuality.MEASURED
DEFAULT_METERING_POINT_ID = "1234567890123"
DEFAULT_METERING_POINT_TYPE = MeteringPointType.CONSUMPTION


def create_row(
    grid_area: str = DEFAULT_GRID_AREA,
    to_grid_area: str = None,
    from_grid_area: str = None,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    metering_point_type: MeteringPointType = DEFAULT_METERING_POINT_TYPE,
    observation_time: datetime = DEFAULT_OBSERVATION_TIME,
    quantity: int | Decimal = DEFAULT_QUANTITY,
    quality: QuantityQuality = DEFAULT_QUALITY,
) -> Row:
    if isinstance(quantity, int):
        quantity = Decimal(quantity)

    row = {
        Colname.grid_area: grid_area,
        Colname.to_grid_area: to_grid_area,
        Colname.from_grid_area: from_grid_area,
        Colname.metering_point_id: metering_point_id,
        Colname.metering_point_type: metering_point_type.value,
        Colname.observation_time: observation_time,
        Colname.quantity: quantity,
        Colname.quality: quality.value,
        Colname.energy_supplier_id: None,
        Colname.balance_responsible_id: None,
        Colname.settlement_method: None,
        Colname.time_window: {
            Colname.start: observation_time,
            Colname.end: observation_time + datetime.timedelta(minutes=15),
        },
    }

    return Row(**row)


def create_from_row(
    grid_area: str = DEFAULT_GRID_AREA,
    to_grid_area: str = DEFAULT_NEIGHBOUR_GRID_AREA,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    observation_time: datetime = DEFAULT_OBSERVATION_TIME,
    quantity: int | Decimal = DEFAULT_QUANTITY,
    quality: QuantityQuality = DEFAULT_QUALITY,
) -> Row:
    """Create a row representing exchange leaving the (default) grid area."""
    return create_row(
        grid_area=grid_area,
        to_grid_area=to_grid_area,
        from_grid_area=grid_area,
        metering_point_id=metering_point_id,
        metering_point_type=MeteringPointType.EXCHANGE,
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
    return create_row(
        grid_area=grid_area,
        to_grid_area=grid_area,
        from_grid_area=from_grid_area,
        metering_point_id=metering_point_id,
        metering_point_type=MeteringPointType.EXCHANGE,
        observation_time=observation_time,
        quantity=quantity,
        quality=quality,
    )


def create(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> QuarterlyMeteringPointTimeSeries:
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(
        data, schema=_quarterly_metering_point_time_series_schema
    )
    return QuarterlyMeteringPointTimeSeries(df)
