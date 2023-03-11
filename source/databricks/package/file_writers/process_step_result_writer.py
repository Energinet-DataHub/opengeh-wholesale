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
from package.codelists import MarketRole, TimeSeriesType, Grouping
from package.constants import Colname, PartitionKeyName
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, lit


class ProcessStepResultWriter:
    def __init__(self, container_path: str, batch_id: str):
        self.__table_name = "result_table"
        self.__delta_table_path = f"{container_path}/{infra.get_calculation_output_folder()}/{self.__table_name}"
        self.__batch_id = batch_id
        self.__output_path = (
            f"{container_path}/{infra.get_batch_relative_path(batch_id)}"
        )

    def write_per_ga(
        self,
        result_df: DataFrame,
        time_series_type: TimeSeriesType,
        grouping: Grouping,
    ) -> None:
        result_df = self._prepare_result_for_output(
            result_df,
        )

        # start: write to delta table (before dropping columns)
        self._write_result_to_table(result_df, time_series_type, grouping)
        # end: write to delta table

        result_df.drop(Colname.energy_supplier_id).drop(Colname.balance_responsible_id)
        partition_by = [PartitionKeyName.GRID_AREA]
        self._write_result_df(result_df, partition_by, time_series_type, grouping)

    def write_per_ga_per_actor(
        self,
        result_df: DataFrame,
        time_series_type: TimeSeriesType,
        market_role: MarketRole,
        grouping: Grouping,
    ) -> None:
        result_df = self._prepare_result_for_output(
            result_df,
        )
        # start: write to delta table (before dropping columns)
        self._write_result_to_table(result_df, time_series_type, grouping)
        # end: write to delta table

        result_df = self._add_gln(result_df, market_role)
        result_df.drop(Colname.energy_supplier_id).drop(Colname.balance_responsible_id)
        partition_by = [PartitionKeyName.GRID_AREA, PartitionKeyName.GLN]
        self._write_result_df(result_df, partition_by, time_series_type, grouping)

    def write_per_ga_per_brp_per_es(
        self,
        result_df: DataFrame,
        time_series_type: TimeSeriesType,
        grouping: Grouping,
    ) -> None:
        result_df = self._prepare_result_for_output(
            result_df,
        )
        # start: write to delta table (before dropping columns)
        self._write_result_to_table(result_df, time_series_type, grouping)
        # end: write to delta table

        result_df = result_df.withColumnRenamed(
            Colname.balance_responsible_id,
            PartitionKeyName.BALANCE_RESPONSIBLE_PARTY_GLN,
        ).withColumnRenamed(
            Colname.energy_supplier_id, PartitionKeyName.ENERGY_SUPPLIER_GLN
        )

        partition_by = [
            PartitionKeyName.GRID_AREA,
            PartitionKeyName.BALANCE_RESPONSIBLE_PARTY_GLN,
            PartitionKeyName.ENERGY_SUPPLIER_GLN,
        ]
        self._write_result_df(result_df, partition_by, time_series_type, grouping)

    def _prepare_result_for_output(self, result_df: DataFrame) -> DataFrame:
        result_df = result_df.select(
            col(Colname.grid_area).alias(PartitionKeyName.GRID_AREA),
            Colname.energy_supplier_id,
            Colname.balance_responsible_id,
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
        elif market_role is MarketRole.BALANCE_RESPONSIBLE_PARTY:
            result_df = result_df.withColumnRenamed(
                Colname.balance_responsible_id, Colname.gln
            )
        else:
            raise NotImplementedError(
                f"Market role, {market_role}, is not supported yet"
            )

        return result_df

    def _write_result_df(
        self,
        result_df: DataFrame,
        partition_by: list[str],
        time_series_type: TimeSeriesType,
        grouping: Grouping,
    ) -> None:
        result_data_directory = f"{self.__output_path}/result/grouping={grouping.value}/time_series_type={time_series_type.value}"

        # First repartition to co-locate all rows for a grid area on a single executor.
        # This ensures that only one file is being written/created for each grid area
        # When writing/creating the files. The partition by creates a folder for each grid area.
        (
            result_df.repartition(PartitionKeyName.GRID_AREA)
            .write.mode("errorifexists")
            .partitionBy(partition_by)
            .json(result_data_directory)
        )

    def _write_result_to_table(
        self,
        df: DataFrame,
        time_series_type: TimeSeriesType,
        grouping: Grouping,
    ) -> None:
        df = (
            df.withColumn(Colname.time_series_type, lit(time_series_type.value))
            .withColumn("grouping", lit(grouping.value))
            .withColumn(Colname.batch_id, lit(self.__batch_id))
        )

        if grouping == Grouping.total_ga:
            df = df.withColumn(
                Colname.balance_responsible_id, lit(None).cast("string")
            ).withColumn(Colname.energy_supplier_id, lit(None).cast("string"))
        elif grouping == Grouping.es_per_ga:
            df = df.withColumn(Colname.balance_responsible_id, lit(None).cast("string"))
        elif grouping == Grouping.brp_per_ga:
            df = df.withColumn(Colname.energy_supplier_id, lit(None).cast("string"))
        elif grouping != Grouping.es_per_brp_per_ga:
            raise ValueError(f"Unsupported grouping, {grouping.value}")

        df.write.format("delta").mode("append").option(
            "path", self.__delta_table_path
        ).saveAsTable(self.__table_name)
