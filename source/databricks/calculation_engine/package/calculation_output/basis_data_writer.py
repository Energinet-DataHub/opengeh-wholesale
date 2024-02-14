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

from package.codelists import BasisDataType
from package.constants import PartitionKeyName
from package.infrastructure import paths, logging_configuration


class BasisDataWriter:
    def __init__(self, container_path: str, calculation_id: str):
        self.__master_basis_data_path = f"{container_path}/{paths.get_basis_data_root_path(BasisDataType.MASTER_BASIS_DATA, calculation_id)}"
        self.__time_series_quarter_path = f"{container_path}/{paths.get_basis_data_root_path(BasisDataType.TIME_SERIES_QUARTER, calculation_id)}"
        self.__time_series_hour_path = f"{container_path}/{paths.get_basis_data_root_path(BasisDataType.TIME_SERIES_HOUR, calculation_id)}"
        self.calculation_id = calculation_id

    def write_basis_data_to_csv(
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
