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
from pyspark.sql.functions import lit
from package.constants.market_role import MarketRole


def write(output_path: str, result_df: DataFrame, market_role: MarketRole) -> None:

    actors_df = _get_actors(result_df, market_role)

    actors_directory = f"{output_path}/actors"

    (
        actors_df.repartition("grid_area")
        .write.mode("append")
        .partitionBy("grid_area", Colname.time_series_type, Colname.market_role)
        .json(actors_directory)
    )


def _get_actors(result_df: DataFrame, market_role: MarketRole) -> DataFrame:

    actors_df = result_df.select(
        "grid_area",
        Colname.gln,
        Colname.time_series_type,
    ).distinct()

    actors_df = actors_df.withColumn(Colname.market_role, lit(market_role.value))

    return actors_df
