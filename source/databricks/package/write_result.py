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
from package.constants.result_grouping import ResultGrouping
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
        result_grouping: ResultGrouping,
    ) -> None:

        result_df = self._prepare_result_for_output(
            result_df, time_series_type, result_grouping
        )

        self._write_result_df(result_df)

        actors_df = self._get_actors_df(result_df, result_grouping)
        self._write_actors(actors_df)

    def _prepare_result_for_output(
        self,
        result_df: DataFrame,
        time_series_type: TimeSeriesType,
        result_grouping: ResultGrouping,
    ) -> DataFrame:

        result_df = self._add_gln_and_time_series_type(
            result_df,
            result_grouping,
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
        result_grouping: ResultGrouping,
        time_series_type: TimeSeriesType,
    ) -> DataFrame:

        result_df = result_df.withColumn(
            Colname.time_series_type, lit(time_series_type.value)
        )

        if result_grouping is ResultGrouping.PER_GRID_AREA:
            result_df = result_df.withColumn(Colname.gln, lit("grid_area"))
        elif result_grouping is ResultGrouping.PER_ENERGY_SUPPLIER:
            result_df = result_df.withColumnRenamed(
                Colname.energy_supplier_id, Colname.gln
            )
        else:
            raise NotImplementedError(
                f"Result grouping, {result_grouping}, is not supported yet"
            )

        return result_df

    def _get_actors_df(
        self, result_df: DataFrame, result_grouping: ResultGrouping
    ) -> DataFrame:

        actors_df = result_df.select(
            "grid_area",
            Colname.gln,
            Colname.time_series_type,
        ).distinct()

        actor_type = self._map(result_grouping)

        actors_df.withColumn(Colname.actor_type, lit(actor_type.value))

        return actors_df

    def _write_actors(self, actors_df: DataFrame) -> None:

        actors_directory = f"{self.output_path}/actors"
        (
            actors_df.repartition("grid_area")
            .write.mode("append")
            .partitionBy("grid_area", Colname.time_series_type, Colname.actor_type)
            .json(actors_directory)
        )

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

    def _map(self, result_grouping: ResultGrouping) -> ActorType:

        if result_grouping == ResultGrouping.PER_ENERGY_SUPPLIER:
            return ActorType.ENERGY_SUPPLIER
        else:
            raise ValueError(f"Cannot map from {result_grouping} to an actor_type")
