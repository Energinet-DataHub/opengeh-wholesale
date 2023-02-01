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
import package.infrastructure as infra


class BasisDataWriter:
    def __init__(self, container_path: str, batch_id: str):
        self.__output_path = (
            f"{container_path}/{infra.get_batch_relative_path(batch_id)}"
        )

    def write(
        self,
        master_basis_data_df: DataFrame,
        timeseries_quarter_df: DataFrame,
        timeseries_hour_df: DataFrame,
    ) -> None:

        basis_data_directory = f"{self.__output_path}/basis_data"

        self._write_basis_data_to_csv(
            f"{basis_data_directory}/time_series_quarter", timeseries_quarter_df
        )
        self._write_basis_data_to_csv(
            f"{basis_data_directory}/time_series_hour", timeseries_hour_df
        )
        self._write_basis_data_to_csv(
            f"{basis_data_directory}/master_basis_data", master_basis_data_df
        )

    def _write_basis_data_to_csv(self, path: str, df: DataFrame) -> None:
        df = df.withColumnRenamed("GridAreaCode", "grid_area")

        df.repartition("grid_area").write.mode("overwrite").partitionBy(
            "grid_area", Colname.gln
        ).option("header", True).csv(path)
