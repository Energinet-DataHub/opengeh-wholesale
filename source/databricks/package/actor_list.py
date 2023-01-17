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
from package.constants import Colname
from package.constants.actor_type import ActorType
from package.constants.time_series_type import TimeSeriesType
from pyspark.sql.functions import lit


def write_actor_list_to_file(
    path: str,
    result_df: DataFrame,
    time_series_type: TimeSeriesType,
    actor_type: ActorType,
) -> None:

    actors_df = _create_actors_df(result_df, actor_type, time_series_type)

    (
        actors_df.repartition("grid_area")
        .write.mode("append")
        .partitionBy("grid_area", Colname.time_series_type, Colname.actor_type)
        .json(path)
    )


def _create_actors_df(
    result_df: DataFrame,
    actor_type: ActorType,
    time_series_type: TimeSeriesType,
) -> DataFrame:

    result_df.withColumn(Colname.actor_type, lit(actor_type.value))
    result_df.withColumn(Colname.time_series_type, lit(time_series_type.value))

    actors_df = result_df.select(
        "grid_area",
        Colname.gln,
        Colname.time_series_type,
    ).distinct()

    return actors_df
