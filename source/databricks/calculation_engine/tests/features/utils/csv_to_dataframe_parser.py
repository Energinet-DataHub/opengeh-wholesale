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
import concurrent.futures
import os
from pathlib import Path

from pyspark.sql import SparkSession

from tests.features.utils.views.dataframe_wrapper import DataframeWrapper


class CsvToDataframeWrapperParser:

    def __init__(self, spark: SparkSession):
        self.spark = spark

    @staticmethod
    def _read_file(
        spark_session: SparkSession,
        file_name: str,
        file_folder: str,
    ) -> DataframeWrapper | None:

        file_path = f"{file_folder}/{file_name}"
        if not os.path.exists(file_path):
            return None

        df = spark_session.read.csv(file_path, header=True, sep=";")
        name, extension = os.path.splitext(file_name)
        return DataframeWrapper(key=file_name, name=name, df=df)

    def parse_csv_files_concurrently(self, path: str) -> list[DataframeWrapper]:
        """
        Reads csv files concurrently and converts them to dataframes.
        """
        csv_files = [x.name for x in list(Path(path).rglob("*.csv"))]

        with concurrent.futures.ThreadPoolExecutor() as executor:
            dataframes = list(
                executor.map(
                    self._read_file,
                    [self.spark] * len(csv_files),
                    csv_files,
                    [path] * len(csv_files),
                )
            )

        # Remove None values from the dataframes list
        dataframes = [x for x in dataframes if x is not None]

        return dataframes
