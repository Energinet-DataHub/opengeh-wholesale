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
from pyspark.sql import DataFrame
from pyspark.sql.types import DecimalType

from package.calculation.preparation.quarterly_metering_point_time_series import (
    QuarterlyMeteringPointTimeSeries,
)
from package.constants import Colname
from package.codelists import MeteringPointResolution
from package.common import assert_schema
from package.calculation.energy.schemas import basis_data_time_series_points_schema


def transform_hour_to_quarter(
    metering_point_time_series: DataFrame,
) -> QuarterlyMeteringPointTimeSeries:
    assert_schema(
        metering_point_time_series.schema,
        basis_data_time_series_points_schema,
    )

    result = metering_point_time_series.withColumn(
        "quarter_times",
        f.when(
            f.col(Colname.resolution) == MeteringPointResolution.HOUR.value,
            f.array(
                f.col(Colname.observation_time),
                f.col(Colname.observation_time) + f.expr("INTERVAL 15 minutes"),
                f.col(Colname.observation_time) + f.expr("INTERVAL 30 minutes"),
                f.col(Colname.observation_time) + f.expr("INTERVAL 45 minutes"),
            ),
        ).when(
            f.col(Colname.resolution) == MeteringPointResolution.QUARTER.value,
            f.array(f.col(Colname.observation_time)),
        ),
    ).select(
        metering_point_time_series["*"],
        f.explode("quarter_times").alias("quarter_time"),
    )
    result = result.withColumn(
        Colname.time_window, f.window(f.col("quarter_time"), "15 minutes")
    )
    result = result.withColumn(
        Colname.quantity,
        f.when(
            f.col(Colname.resolution) == MeteringPointResolution.HOUR.value,
            f.col(Colname.quantity) / 4,
        )
        .when(
            f.col(Colname.resolution) == MeteringPointResolution.QUARTER.value,
            f.col(Colname.quantity),
        )
        .cast(DecimalType(18, 6)),
    )

    return QuarterlyMeteringPointTimeSeries(result)
