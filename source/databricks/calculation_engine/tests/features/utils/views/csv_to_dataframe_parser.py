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

from pyspark.sql import SparkSession

from features.utils.expected_output import ExpectedOutput


class CsvToDataframeParser:

    def __init__(self, spark: SparkSession):
        self.spark = spark

    @staticmethod
    def _read_file(
        spark_session: SparkSession,
        file_name: str,
        schema: str,
        file_folder: str,
    ) -> ExpectedOutput | None:

        file_path = f"{file_folder}/{file_name}"
        if not os.path.exists(file_path):
            return None

        df = spark_session.read.csv(file_path, header=True, sep=";", schema=schema)

        # We need to create the dataframe again because nullability and precision
        # are not applied when reading the csv file.
        df = spark_session.createDataFrame(df.rdd, schema)
        name, extension = os.path.splitext(file_name)
        return ExpectedOutput(name=name, df=df)

    def read_files_in_parallel(
        self, path: str, correlations: dict[str, tuple]
    ) -> list[ExpectedOutput]:
        """
        Reads csv files concurrently and converts them to dataframes.
        """
        schemas = [t[0] for t in correlations.values()]

        with concurrent.futures.ThreadPoolExecutor() as executor:
            dataframes = list(
                executor.map(
                    self._read_file,
                    [self.spark] * len(correlations.keys()),
                    correlations.keys(),
                    schemas,
                    [path] * len(correlations.keys()),
                )
            )
        return dataframes
