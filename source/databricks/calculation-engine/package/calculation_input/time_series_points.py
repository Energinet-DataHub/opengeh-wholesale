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
from pyspark.sql import DataFrame, SparkSession
import pyspark.sql.functions as F
from package.constants import Colname


def get_time_series_points_df(
    spark: SparkSession,
    wholesale_container_path: str,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:

    timeseries_points_df = (
        spark.read.option("mode", "FAILFAST")
        .format("delta")
        .load(f"{wholesale_container_path}/calculation_input/time_series_points")
    )

    timeseries_points_df = timeseries_points_df.where(
        F.col(Colname.observation_time) >= period_start_datetime
    ).where(F.col(Colname.observation_time) < period_end_datetime)

    return timeseries_points_df
