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
from package.constants.market_role import MarketRole
from pyspark.sql.functions import col, lit


class ProcessStepResultWriter:
    def __init__(
        self,
        output_path: str,
    ):
        self.__output_path = output_path

    def write_per_ga(
        self, result_df: DataFrame, time_series_type: TimeSeriesType
    ) -> None:

        result_df = self._add_gln_without_market_role(result_df)
        result_df = self._prepare_result_for_output(
            result_df,
            time_series_type,
        )
        self._write_result_df(result_df, time_series_type)

    def write_per_ga_per_actor(
        self,
        result_df: DataFrame,
        time_series_type: TimeSeriesType,
        market_role: MarketRole,
    ) -> None:

        result_df = self._add_gln(result_df, market_role)
        result_df = self._prepare_result_for_output(
            result_df,
            time_series_type,
        )
        self._write_result_df(result_df, time_series_type)
        self._write_actors(result_df, market_role)

    def _prepare_result_for_output(
        self, result_df: DataFrame, time_series_type: TimeSeriesType
    ) -> DataFrame:

        result_df = result_df.withColumn(
            Colname.time_series_type, lit(time_series_type.value)
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

    def _add_gln(
        self,
        result_df: DataFrame,
        market_role: MarketRole,
    ) -> DataFrame:

        if market_role is MarketRole.ENERGY_SUPPLIER:
            result_df = result_df.withColumnRenamed(
                Colname.energy_supplier_id, Colname.gln
            )
        else:
            raise NotImplementedError(
                f"Market role, {market_role}, is not supported yet"
            )

        return result_df

    def _add_gln_without_market_role(
        self,
        result_df: DataFrame,
    ) -> DataFrame:
        result_df = result_df.withColumn(Colname.gln, lit("grid_area"))
        return result_df

    def _write_result_df(
        self, result_df: DataFrame, time_series_type: TimeSeriesType
    ) -> None:

        result_data_directory = f"{self.__output_path}/result"

        # First repartition to co-locate all rows for a grid area on a single executor.
        # This ensures that only one file is being written/created for each grid area
        # When writing/creating the files. The partition by creates a folder for each grid area.
        (
            result_df.repartition("grid_area")
            .write.mode("append")
            .partitionBy("grid_area", Colname.gln, Colname.time_series_type)
            .json(result_data_directory)
        )

    def _write_actors(self, result_df: DataFrame, market_role: MarketRole) -> None:

        actors_df = self._get_actors(result_df, market_role)

        actors_directory = f"{self.__output_path}/actors"

        (
            actors_df.repartition("grid_area")
            .write.mode("append")
            .partitionBy("grid_area", Colname.time_series_type, Colname.market_role)
            .json(actors_directory)
        )

    def _get_actors(self, result_df: DataFrame, market_role: MarketRole) -> DataFrame:

        actors_df = result_df.select(
            "grid_area",
            Colname.gln,
            Colname.time_series_type,
        ).distinct()

        actors_df = actors_df.withColumn(Colname.market_role, lit(market_role.value))

        return actors_df
