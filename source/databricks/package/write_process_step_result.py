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
from package.constants.time_series_type import TimeSeriesType
from package.constants.actor_type import ActorType
from pyspark.sql.functions import col, lit


class ProcessStepResultWriter:
    def __init__(
        self,
        output_path: str,
    ):
        self.output_path = output_path

    def write(
        self,
        result_df: DataFrame,
        time_series_type: TimeSeriesType,
        actor_type: ActorType,
    ) -> None:

        result_df = self._prepare_result_for_output(
            result_df, time_series_type, actor_type
        )

        self._write_result_df(result_df)

        if actor_type is not ActorType.NONE:
            self._write_actors(result_df, actor_type)

    def _prepare_result_for_output(
        self,
        result_df: DataFrame,
        time_series_type: TimeSeriesType,
        actor_type: ActorType,
    ) -> DataFrame:

        result_df = self._add_gln_and_time_series_type(
            result_df,
            actor_type,
            time_series_type,
        )

        result_df = result_df.select(
            col(Colname.grid_area).alias("grid_area"),
            Colname.gln,
            Colname.time_series_type,
            col(Colname.sum_quantity).alias("quantity").cast("string"),
            col(Colname.quality).alias("quality"),
            col(Colname.time_window_start).alias("quarter_time"),
        )

        return result_df

    def _add_gln_and_time_series_type(
        self,
        result_df: DataFrame,
        actor_type: ActorType,
        time_series_type: TimeSeriesType,
    ) -> DataFrame:

        result_df = result_df.withColumn(
            Colname.time_series_type, lit(time_series_type.value)
        )

        if actor_type is ActorType.NONE:
            result_df = result_df.withColumn(Colname.gln, lit("grid_area"))
        elif actor_type is ActorType.ENERGY_SUPPLIER:
            result_df = result_df.withColumnRenamed(
                Colname.energy_supplier_id, Colname.gln
            )
        else:
            raise NotImplementedError(f"Actor type, {actor_type}, is not supported yet")

        return result_df

    def _write_result_df(self, result_df: DataFrame) -> None:

        result_data_directory = f"{self.output_path}/result"

        # First repartition to co-locate all rows for a grid area on a single executor.
        # This ensures that only one file is being written/created for each grid area
        # When writing/creating the files. The partition by creates a folder for each grid area.
        (
            result_df.repartition("grid_area")
            .write.mode("append")
            .partitionBy("grid_area", Colname.gln, Colname.time_series_type)
            .json(result_data_directory)
        )

    def _write_actors(self, result_df: DataFrame, actor_type: ActorType) -> None:

        actors_df = self._get_actors_df(result_df, actor_type)

        actors_directory = f"{self.output_path}/actors"

        (
            actors_df.repartition("grid_area")
            .write.mode("append")
            .partitionBy("grid_area", Colname.time_series_type, Colname.actor_type)
            .json(actors_directory)
        )

    def _get_actors_df(self, result_df: DataFrame, actor_type: ActorType) -> DataFrame:

        actors_df = result_df.select(
            "grid_area",
            Colname.gln,
            Colname.time_series_type,
        ).distinct()

        actors_df = actors_df.withColumn(Colname.actor_type, lit(actor_type.value))

        return actors_df
