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

import pyspark.sql.functions as f

from package.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from package.calculation.preparation.data_structures.metering_point_time_series import (
    MeteringPointTimeSeries,
)
from package.codelists import MeteringPointResolution
from package.constants import Colname


def transform_quarter_to_hour(
    metering_point_time_series: PreparedMeteringPointTimeSeries,
) -> MeteringPointTimeSeries:
    df = metering_point_time_series.df
    result = df.withColumn(
        Colname.observation_time, f.date_trunc("hour", Colname.observation_time)
    )
    result = (
        result.groupby(
            Colname.grid_area,
            Colname.to_grid_area,
            Colname.from_grid_area,
            Colname.metering_point_id,
            Colname.metering_point_type,
            Colname.resolution,
            Colname.observation_time,
            Colname.quality,
            Colname.energy_supplier_id,
            Colname.balance_responsible_id,
            Colname.settlement_method,
        )
        .sum(Colname.quantity)
        .withColumnRenamed("sum(quantity)", Colname.quantity)
        .withColumn(Colname.resolution, f.lit(MeteringPointResolution.HOUR.value))
    )

    return MeteringPointTimeSeries(result)
