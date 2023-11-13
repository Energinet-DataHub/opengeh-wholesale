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

from pyspark.sql import Row, SparkSession

from package.calculation.preparation.grid_loss_responsible import (
    grid_loss_responsible_schema,
    GridLossResponsible,
)
from package.codelists import MeteringPointType
from package.constants import Colname

DEFAULT_METERING_POINT_ID = "1234567890123"
DEFAULT_GRID_AREA = "100"
DEFAULT_FROM_DATE = datetime.strptime("2020-01-01T00:00:00+0000", "%Y-%m-%dT%H:%M:%S%z")
DEFAULT_TO_DATE = datetime.strptime("2020-01-01T01:00:00+0000", "%Y-%m-%dT%H:%M:%S%z")
DEFAULT_METERING_POINT_TYPE = MeteringPointType.CONSUMPTION
DEFAULT_ENERGY_SUPPLIER = "S1"


def create_row(
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    grid_area: str = DEFAULT_GRID_AREA,
    from_date: datetime = DEFAULT_FROM_DATE,
    to_date: datetime = DEFAULT_TO_DATE,
    metering_point_type: MeteringPointType = DEFAULT_METERING_POINT_TYPE,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER,
    is_negative_grid_loss_responsible: bool = False,
    is_positive_grid_loss_responsible: bool = False,
) -> Row:
    row = {
        Colname.metering_point_id: metering_point_id,
        Colname.grid_area: grid_area,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
        Colname.metering_point_type: metering_point_type.value,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.is_negative_grid_loss_responsible: is_negative_grid_loss_responsible,
        Colname.is_positive_grid_loss_responsible: is_positive_grid_loss_responsible,
    }

    return Row(**row)


def create(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> GridLossResponsible:
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, schema=grid_loss_responsible_schema)
    return GridLossResponsible(df)
