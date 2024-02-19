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

from pyspark.sql import SparkSession


def read_file(spark_session: SparkSession, file_path: str):
    return spark_session.read.csv(file_path)


def read_files_in_parallel(spark_session: SparkSession, paths: list):
    with concurrent.futures.ThreadPoolExecutor() as executor:
        dataframes = list(executor.map(read_file, [spark_session] * len(paths), paths))
    return dataframes


# Usage
spark = SparkSession.builder.getOrCreate()
file_paths = ["charge_link_periods.csv", "charge_price_points.csv"]
dataframes = read_files_in_parallel(spark, file_paths)
