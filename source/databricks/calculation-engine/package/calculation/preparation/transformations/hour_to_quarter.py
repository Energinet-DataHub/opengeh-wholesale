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
import pyspark.sql.functions as F

from package.constants import Colname
from package.codelists import MeteringPointResolution


def transform_hour_to_quarter(df: DataFrame) -> DataFrame:
    result = df.withColumn(
        "quarter_times",
        F.when(
            F.col(Colname.resolution) == MeteringPointResolution.HOUR.value,
            F.array(
                F.col(Colname.observation_time),
                F.col(Colname.observation_time) + F.expr("INTERVAL 15 minutes"),
                F.col(Colname.observation_time) + F.expr("INTERVAL 30 minutes"),
                F.col(Colname.observation_time) + F.expr("INTERVAL 45 minutes"),
            ),
        ).when(
            F.col(Colname.resolution) == MeteringPointResolution.QUARTER.value,
            F.array(F.col(Colname.observation_time)),
        ),
    ).select(
        df["*"],
        F.explode("quarter_times").alias("quarter_time"),
    )
    result = result.withColumn(
        Colname.time_window, F.window(F.col("quarter_time"), "15 minutes")
    )
    result = result.withColumn(
        "quarter_quantity",
        F.when(
            F.col(Colname.resolution) == MeteringPointResolution.HOUR.value,
            F.col(Colname.quantity) / 4,
        ).when(
            F.col(Colname.resolution) == MeteringPointResolution.QUARTER.value,
            F.col(Colname.quantity),
        ),
    )
    return result
