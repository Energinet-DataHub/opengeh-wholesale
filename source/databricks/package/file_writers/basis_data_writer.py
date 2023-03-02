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
from pyspark.sql import DataFrame

import package.basis_data as basis_data
import package.infrastructure as infra
from package.constants import PartitionKeyName
from package.constants import Colname
from package.codelists import Grouping


class BasisDataWriter:
    def __init__(self, container_path: str, batch_id: str):
        self.__output_path = (
            f"{container_path}/{infra.get_batch_relative_path(batch_id)}"
        )

    def write(
        self,
        metering_points_periods_df: DataFrame,
        enriched_time_series_point_df: DataFrame,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
        time_zone: str,
    ) -> None:
        (
            timeseries_quarter_df,
            timeseries_hour_df,
        ) = basis_data.get_time_series_basis_data_dfs(
            enriched_time_series_point_df, time_zone
        )

        master_basis_data_df = basis_data.get_master_basis_data_df(
            metering_points_periods_df, period_start_datetime, period_end_datetime
        )

        self._write(master_basis_data_df, timeseries_quarter_df, timeseries_hour_df)

    def _write(
        self,
        master_basis_data_df: DataFrame,
        timeseries_quarter_df: DataFrame,
        timeseries_hour_df: DataFrame,
    ) -> None:
        basis_data_directory = f"{self.__output_path}/basis_data"

        self._write_ga_basis_data_to_csv(
            master_basis_data_df,
            timeseries_quarter_df,
            timeseries_hour_df,
            basis_data_directory,
        )
        self._write_es_basis_data_to_csv(
            master_basis_data_df,
            timeseries_quarter_df,
            timeseries_hour_df,
            basis_data_directory,
        )

    def _write_ga_basis_data_to_csv(
        self,
        master_basis_data_df: DataFrame,
        timeseries_quarter_df: DataFrame,
        timeseries_hour_df: DataFrame,
        basis_data_directory: str,
    ) -> None:
        partition_keys = [PartitionKeyName.GRID_AREA]
        grouping_folder_name = f"grouping={Grouping.total_ga}"

        timeseries_quarter_df = timeseries_quarter_df.drop(Colname.energy_supplier_id)
        timeseries_hour_df = timeseries_hour_df.drop(Colname.energy_supplier_id)

        self._write_basis_data_to_csv(
            f"{basis_data_directory}/time_series_quarter/{grouping_folder_name}",
            timeseries_quarter_df,
            partition_keys,
        )
        self._write_basis_data_to_csv(
            f"{basis_data_directory}/time_series_hour/{grouping_folder_name}",
            timeseries_hour_df,
            partition_keys,
        )
        self._write_basis_data_to_csv(
            f"{basis_data_directory}/master_basis_data/{grouping_folder_name}",
            master_basis_data_df,
            partition_keys,
        )

    def _write_es_basis_data_to_csv(
        self,
        master_basis_data_df: DataFrame,
        timeseries_quarter_df: DataFrame,
        timeseries_hour_df: DataFrame,
        basis_data_directory: str,
    ) -> None:
        partition_keys = [
            PartitionKeyName.GRID_AREA,
            PartitionKeyName.ENERGY_SUPPLIER_GLN,
        ]
        grouping_folder_name = f"grouping={Grouping.es_per_ga}"

        timeseries_quarter_df = timeseries_quarter_df.withColumnRenamed(
            Colname.energy_supplier_id, PartitionKeyName.ENERGY_SUPPLIER_GLN
        )
        timeseries_hour_df = timeseries_hour_df.withColumnRenamed(
            Colname.energy_supplier_id, PartitionKeyName.ENERGY_SUPPLIER_GLN
        )
        master_basis_data_df = master_basis_data_df.withColumnRenamed(
            Colname.energy_supplier_id, PartitionKeyName.ENERGY_SUPPLIER_GLN
        )

        self._write_basis_data_to_csv(
            f"{basis_data_directory}/time_series_quarter/{grouping_folder_name}",
            timeseries_quarter_df,
            partition_keys,
        )
        self._write_basis_data_to_csv(
            f"{basis_data_directory}/time_series_hour/{grouping_folder_name}",
            timeseries_hour_df,
            partition_keys,
        )
        self._write_basis_data_to_csv(
            f"{basis_data_directory}/master_basis_data/{grouping_folder_name}",
            master_basis_data_df,
            partition_keys,
        )

    def _write_basis_data_to_csv(
        self, path: str, df: DataFrame, partition_keys: list[str]
    ) -> None:
        df = df.withColumnRenamed("GridAreaCode", PartitionKeyName.GRID_AREA)

        df.repartition(PartitionKeyName.GRID_AREA).write.mode("overwrite").partitionBy(
            partition_keys
        ).option("header", True).csv(path)
