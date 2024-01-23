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
from pyspark.sql.functions import col

import package.calculation.preparation.transformations.basis_data as basis_data
from package.codelists import AggregationLevel, BasisDataType
from package.constants import PartitionKeyName, BasisDataColname
from package.infrastructure import paths, logging_configuration
from package.infrastructure.paths import OUTPUT_DATABASE_NAME


class BasisDataWriter:
    def __init__(self, container_path: str, batch_id: str):
        self.__master_basis_data_path = f"{container_path}/{paths.get_basis_data_root_path(BasisDataType.MASTER_BASIS_DATA, batch_id)}"
        self.__time_series_quarter_path = f"{container_path}/{paths.get_basis_data_root_path(BasisDataType.TIME_SERIES_QUARTER, batch_id)}"
        self.__time_series_hour_path = f"{container_path}/{paths.get_basis_data_root_path(BasisDataType.TIME_SERIES_HOUR, batch_id)}"
        self.calculation_id = batch_id

    @logging_configuration.use_span("calculation.basis_data")
    def write(
        self,
        metering_points_periods_df: DataFrame,
        metering_point_time_series: DataFrame,
        time_zone: str,
    ) -> None:
        with logging_configuration.start_span("prepare"):
            (
                timeseries_quarter_df,
                timeseries_hour_df,
            ) = basis_data.get_metering_point_time_series_basis_data_dfs(
                metering_point_time_series, time_zone
            )

        master_basis_data_df = basis_data.get_master_basis_data_df(
            metering_points_periods_df
        )

        self._write(master_basis_data_df, timeseries_quarter_df, timeseries_hour_df)

    def _write(
        self,
        master_basis_data_df: DataFrame,
        timeseries_quarter_df: DataFrame,
        timeseries_hour_df: DataFrame,
    ) -> None:
        self._write_ga_basis_data(
            master_basis_data_df,
            timeseries_quarter_df,
            timeseries_hour_df,
        )

        self._write_es_basis_data(
            master_basis_data_df,
            timeseries_quarter_df,
            timeseries_hour_df,
        )

        _write_basis_data_to_delta_table(
            master_basis_data_df,
            timeseries_quarter_df,
            timeseries_hour_df,
        )

    @logging_configuration.use_span("per_grid_area")
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

    @logging_configuration.use_span("per_energy_supplier")
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
        with logging_configuration.start_span("timeseries_quarter_to_csv"):
            _write_df_to_csv(
                f"{self.__time_series_quarter_path}/{grouping_folder_name}",
                timeseries_quarter_df,
                partition_keys,
            )

        with logging_configuration.start_span("timeseries_hour_to_csv"):
            _write_df_to_csv(
                f"{self.__time_series_hour_path}/{grouping_folder_name}",
                timeseries_hour_df,
                partition_keys,
            )

        with logging_configuration.start_span("metering_point_to_csv"):
            _write_df_to_csv(
                f"{self.__master_basis_data_path}/{grouping_folder_name}",
                master_basis_data_df,
                partition_keys,
            )


def _write_df_to_csv(path: str, df: DataFrame, partition_keys: list[str]) -> None:
    df.repartition(PartitionKeyName.GRID_AREA).write.mode("overwrite").partitionBy(
        partition_keys
    ).option("header", True).csv(path)


def _write_basis_data_to_delta_table(
    master_basis_data_df: DataFrame,
    timeseries_quarter_df: DataFrame,
    timeseries_hour_df: DataFrame,
) -> None:
    with logging_configuration.start_span("basis_data_to_delta_table"):
        _write_to_storage(master_basis_data_df, paths.MASTER_BASIS_DATA_TABLE_NAME)
        _write_to_storage(timeseries_quarter_df, paths.TIME_SERIES_QUARTER_TABLE_NAME)
        _write_to_storage(timeseries_hour_df, paths.TIME_SERIES_HOUR_TABLE_NAME)


def _write_to_storage(results: DataFrame, table_name: str) -> None:
    results.write.format("delta").mode("append").option(
        "mergeSchema", "false"
    ).insertInto(f"{OUTPUT_DATABASE_NAME}.{table_name}")
