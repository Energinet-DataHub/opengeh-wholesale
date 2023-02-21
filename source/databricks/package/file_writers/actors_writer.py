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

import package.infrastructure as infra
from package.codelists.time_series_type import TimeSeriesType
from package.constants import Colname
from pyspark.sql import DataFrame
from pyspark.sql.functions import lit


class ActorsWriter:
    def __init__(self, container_path: str, batch_id: str):
        self.__output_path = (
            f"{container_path}/{infra.get_batch_relative_path(batch_id)}/actors/"
        )

    def write(self, result_df: DataFrame, time_series_type: TimeSeriesType) -> None:
        result_df = result_df.withColumnRenamed(Colname.grid_area, "grid_area")

        actors_df = result_df.select("grid_area", Colname.energy_supplier_id).distinct()
        actors_df = actors_df.withColumnRenamed(
            Colname.energy_supplier_id, "energy_supplier_gln"
        )
        actors_df = actors_df.withColumn(
            Colname.time_series_type, lit(time_series_type.value)
        )

        (
            actors_df.repartition("grid_area")
            .write.mode("overwrite")
            .partitionBy(Colname.time_series_type, "grid_area")
            .json(self.__output_path)
        )
