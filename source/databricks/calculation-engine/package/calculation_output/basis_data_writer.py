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

from datetime import timedelta
import time
from pyspark.sql import DataFrame
from pyspark.sql.functions import col

import package.calculation_input.basis_data as basis_data
from package.infrastructure import paths, log
from package.constants import PartitionKeyName, BasisDataColname
from package.codelists import AggregationLevel, BasisDataType


class BasisDataWriter:
    def __init__(self, container_path: str, batch_id: str):
        self.__master_basis_data_path = f"{container_path}/{paths.get_basis_data_root_path(BasisDataType.MASTER_BASIS_DATA, batch_id)}"
        self.__time_series_quarter_path = f"{container_path}/{paths.get_basis_data_root_path(BasisDataType.TIME_SERIES_QUARTER, batch_id)}"
        self.__time_series_hour_path = f"{container_path}/{paths.get_basis_data_root_path(BasisDataType.TIME_SERIES_HOUR, batch_id)}"

    def write(
        self,
        metering_points_periods_df: DataFrame,
        enriched_time_series_point_df: DataFrame,
        time_zone: str,
    ) -> None:

        # TIMER #
        start_time = time.time()
        # TIMER #

        (
            timeseries_quarter_df,
            timeseries_hour_df,
        ) = basis_data.get_time_series_basis_data_dfs(
            enriched_time_series_point_df, time_zone
        )

        master_basis_data_df = basis_data.get_master_basis_data_df(
            metering_points_periods_df
        )

        # TIMER #
        duration = time.time() - start_time
        log(f"Get basis data took: {str(timedelta(seconds=duration))}")        
        # TIMER #

        self._write(master_basis_data_df, timeseries_quarter_df, timeseries_hour_df)

    def _write(
        self,
        master_basis_data_df: DataFrame,
        timeseries_quarter_df: DataFrame,
        timeseries_hour_df: DataFrame,
    ) -> None:

        # TIMER #
        start_time = time.time()
        # TIMER #

        self._write_ga_basis_data(
            master_basis_data_df,
            timeseries_quarter_df,
            timeseries_hour_df,
        )

        # TIMER #
        duration = time.time() - start_time
        log(f"_write_ga_basis_data took: {str(timedelta(seconds=duration))}")
        start_time = time.time()
        # TIMER #

        self._write_es_basis_data(
            master_basis_data_df,
            timeseries_quarter_df,
            timeseries_hour_df,
        )

        # TIMER #
        duration = time.time() - start_time
        log(f"_write_es_basis_data took: {str(timedelta(seconds=duration))}")
        # TIMER #


    def _write_ga_basis_data(
        self,
        master_basis_data_df: DataFrame,
        timeseries_quarter_df: DataFrame,
        timeseries_hour_df: DataFrame,
    ) -> None:
        grouping_folder_name = f"grouping={AggregationLevel.TOTAL_GA.value}"

        partition_keys = [PartitionKeyName.GRID_AREA]
        timeseries_quarter_df = timeseries_quarter_df.drop(
            BasisDataColname.energy_supplier_id
        ).withColumnRenamed(BasisDataColname.grid_area, PartitionKeyName.GRID_AREA)
        timeseries_hour_df = timeseries_hour_df.drop(
            BasisDataColname.energy_supplier_id
        ).withColumnRenamed(BasisDataColname.grid_area, PartitionKeyName.GRID_AREA)
        master_basis_data_df = master_basis_data_df.withColumn(
            PartitionKeyName.GRID_AREA, col(BasisDataColname.grid_area)
        )

        self._write_basis_data_to_csv(
            master_basis_data_df,
            timeseries_quarter_df,
            timeseries_hour_df,
            grouping_folder_name,
            partition_keys,
        )

    def _write_es_basis_data(
        self,
        master_basis_data_df: DataFrame,
        timeseries_quarter_df: DataFrame,
        timeseries_hour_df: DataFrame,
    ) -> None:
        grouping_folder_name = f"grouping={AggregationLevel.ES_PER_GA.value}"

        partition_keys = [
            PartitionKeyName.GRID_AREA,
            PartitionKeyName.ENERGY_SUPPLIER_GLN,
        ]

        timeseries_quarter_df = timeseries_quarter_df.withColumnRenamed(
            BasisDataColname.energy_supplier_id, PartitionKeyName.ENERGY_SUPPLIER_GLN
        ).withColumnRenamed(BasisDataColname.grid_area, PartitionKeyName.GRID_AREA)
        timeseries_hour_df = timeseries_hour_df.withColumnRenamed(
            BasisDataColname.energy_supplier_id, PartitionKeyName.ENERGY_SUPPLIER_GLN
        ).withColumnRenamed(BasisDataColname.grid_area, PartitionKeyName.GRID_AREA)
        master_basis_data_df = master_basis_data_df.withColumnRenamed(
            BasisDataColname.energy_supplier_id, PartitionKeyName.ENERGY_SUPPLIER_GLN
        ).withColumn(PartitionKeyName.GRID_AREA, col(BasisDataColname.grid_area))

        self._write_basis_data_to_csv(
            master_basis_data_df,
            timeseries_quarter_df,
            timeseries_hour_df,
            grouping_folder_name,
            partition_keys,
        )

    def _write_basis_data_to_csv(
        self,
        master_basis_data_df: DataFrame,
        timeseries_quarter_df: DataFrame,
        timeseries_hour_df: DataFrame,
        grouping_folder_name: str,
        partition_keys: list[str],
    ) -> None:
        self._write_df_to_csv(
            f"{self.__time_series_quarter_path}/{grouping_folder_name}",
            timeseries_quarter_df,
            partition_keys,
        )

        self._write_df_to_csv(
            f"{self.__time_series_hour_path}/{grouping_folder_name}",
            timeseries_hour_df,
            partition_keys,
        )
        self._write_df_to_csv(
            f"{self.__master_basis_data_path}/{grouping_folder_name}",
            master_basis_data_df,
            partition_keys,
        )

    def _write_df_to_csv(
        self, path: str, df: DataFrame, partition_keys: list[str]
    ) -> None:
        df.repartition(PartitionKeyName.GRID_AREA).write.mode("overwrite").partitionBy(
            partition_keys
        ).option("header", True).csv(path)
