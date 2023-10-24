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

from package.constants import Colname
from package.codelists import MeteringPointResolution
from package.common import assert_schema
from package.calculation.energy.schemas import basis_data_time_series_points_schema


def transform_hour_to_quarter(basis_data_time_series_points_df: DataFrame) -> DataFrame:
    assert_schema(
        basis_data_time_series_points_df.schema,
        basis_data_time_series_points_schema,
    )

    result = basis_data_time_series_points_df.withColumn(
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
        basis_data_time_series_points_df["*"],
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

    result = result.select(
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
    result.schema[Colname.quantity].nullable = False

    return result
