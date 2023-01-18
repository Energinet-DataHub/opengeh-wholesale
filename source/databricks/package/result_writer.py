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

from pyspark.sql import DataFrame, SparkSession
from package.constants import Colname


class ResultWriter:
    def __init__(
        self,
        batch_id: str,
        results_path: str,
    ):
        self.batch_directory = f"{results_path}/batch_id={batch_id}"

    def write_basis_data(
        self,
        master_basis_data_df: DataFrame,
        timeseries_quarter_df: DataFrame,
        timeseries_hour_df: DataFrame,
    ) -> None:

        basis_data_directory = f"{self.batch_directory}/basis_data"

        self._write_basis_data_to_csv(
            f"{basis_data_directory}/time_series_quarter", timeseries_quarter_df
        )
        self._write_basis_data_to_csv(
            f"{basis_data_directory}/time_series_hour", timeseries_hour_df
        )
        self._write_basis_data_to_csv(
            f"{basis_data_directory}/master_basis_data", master_basis_data_df
        )

    def write_result(self, result_df: DataFrame) -> None:
        result_data_directory = f"{self.batch_directory}/result"

        # First repartition to co-locate all rows for a grid area on a single executor.
        # This ensures that only one file is being written/created for each grid area
        # When writing/creating the files. The partition by creates a folder for each grid area.
        (
            result_df.repartition("grid_area")
            .write.mode("append")
            .partitionBy("grid_area", Colname.gln, Colname.time_series_type)
            .json(result_data_directory)
        )

    def _write_basis_data_to_csv(self, path: str, df: DataFrame) -> None:
        df = df.withColumnRenamed("GridAreaCode", "grid_area")

        df.repartition("grid_area").write.mode("overwrite").partitionBy(
            "grid_area", Colname.gln
        ).option("header", True).csv(path)
