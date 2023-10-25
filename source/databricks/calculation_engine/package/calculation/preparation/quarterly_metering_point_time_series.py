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

from pyspark.sql import DataFrame

from package.calculation.energy.schemas import (
    time_series_quarter_points_schema,
)
from package.common import TimeSeries
from package.constants import Colname


class QuarterlyMeteringPointTimeSeries(TimeSeries):
    """
    Time series points of metering points with resolution quarterly.

    The points are enriched with metering point data required by calculations.

    When points are missing the time series are padded with
    points where quantity=0 and quality=missing.

    Can be either hourly or quarterly.
    """

    def __init__(self, df: DataFrame):
        df = df.select(
            Colname.grid_area,
            Colname.to_grid_area,
            Colname.from_grid_area,
            Colname.metering_point_id,
            Colname.metering_point_type,
            Colname.resolution,
            Colname.observation_time,
            Colname.quantity,
            Colname.quality,
            Colname.energy_supplier_id,
            Colname.balance_responsible_id,
            Colname.time_window,
        )

        # Workaround to enforce quantity nullable=False. This should be safe as quantity in input is nullable=False
        df.schema[Colname.quantity].nullable = False

        super().__init__(df, time_series_quarter_points_schema)
