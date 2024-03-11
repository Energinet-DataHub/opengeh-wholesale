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

from pyspark.sql import Row
from pyspark.sql.types import (
    TimestampType,
)

from package.codelists import MeteringPointResolution
from package.codelists import QuantityQuality
from package.constants import Colname


def basis_data_time_series_points_row(
    grid_area: str = "805",
    to_grid_area: str = "805",
    from_grid_area: str = "806",
    metering_point_id: str = "the_metering_point_id",
    metering_point_type: str = "the_metering_point_type",
    resolution: MeteringPointResolution = MeteringPointResolution.HOUR,
    observation_time: TimestampType() = datetime(2020, 1, 1, 0, 0),
    quantity: Decimal = Decimal("4.444444"),
    quality: QuantityQuality = QuantityQuality.ESTIMATED,
    energy_supplier_id: str = "the_energy_supplier_id",
    balance_responsible_id: str = "the_balance_responsible_id",
    settlement_method: str = "the_settlement_method",
) -> Row:
    row = {
        Colname.grid_area: grid_area,
        Colname.to_grid_area: to_grid_area,
        Colname.from_grid_area: from_grid_area,
        Colname.metering_point_id: metering_point_id,
        Colname.metering_point_type: metering_point_type,
        Colname.resolution: resolution.value,
        Colname.observation_time: observation_time,
        Colname.quantity: quantity,
        Colname.quality: quality.value,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.balance_responsible_id: balance_responsible_id,
        Colname.settlement_method: settlement_method,
    }

    return Row(**row)
