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

import pytest
from pyspark.sql import SparkSession, DataFrame
from typing import Callable, Union
from pyspark import RDD

from pyspark.sql.types import (
    StructType,
    StructField,
    StringType,
    TimestampType,
)


time_series_received_schema = StructType(
    [
        StructField("enqueuedTime", TimestampType(), True),
        StructField("body", StringType(), True),
    ]
)


@pytest.fixture(scope="session")
def parquet_reader(
    spark: SparkSession, data_lake_path: str
) -> Callable[[str], Union[DataFrame, RDD]]:
    def f(path: str) -> Union[DataFrame, RDD]:
        data = spark.sparkContext.emptyRDD()
        try:
            return spark.read.format("parquet").load(f"{data_lake_path}/{path}")
        except Exception:
            pass
        return data

    return f
