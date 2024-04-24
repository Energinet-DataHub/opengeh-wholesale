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

from package.calculation.energy.aggregators.transformations import (
    aggregate_quantity_and_quality,
)
from package.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
)
from package.calculation.preparation.data_structures.metering_point_time_series import (
    MeteringPointTimeSeries,
)
from package.codelists import QuantityQuality
from package.constants import Colname


def transform_quarter_to_hour(
    metering_point_time_series: PreparedMeteringPointTimeSeries,
) -> MeteringPointTimeSeries:
    df = metering_point_time_series.df
    result = df.withColumn(
        Colname.observation_time, f.date_trunc("hour", Colname.observation_time)
    )
    group_by = [
        col for col in df.columns if col != Colname.quantity and col != Colname.quality
    ]
    result = aggregate_quantity_and_quality(result, group_by)

    result = result.withColumn(
        Colname.quality,
        f.when(
            f.array_contains(
                f.col(Colname.qualities), QuantityQuality.CALCULATED.value
            ),
            QuantityQuality.CALCULATED.value,
        )
        .when(
            f.array_contains(f.col(Colname.qualities), QuantityQuality.ESTIMATED.value),
            QuantityQuality.ESTIMATED.value,
        )
        .when(
            (
                f.array_contains(
                    f.col(Colname.qualities),
                    QuantityQuality.MEASURED.value,
                )
                & f.array_contains(
                    f.col(Colname.qualities), QuantityQuality.MISSING.value
                )
            ),
            QuantityQuality.ESTIMATED.value,
        )
        .when(
            f.array_contains(f.col(Colname.qualities), QuantityQuality.MEASURED.value),
            QuantityQuality.MEASURED.value,
        )
        .otherwise(QuantityQuality.MISSING.value),
    )

    return MeteringPointTimeSeries(result)
