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

import os
import shutil
import pytest

from package import timeseries_persister
from pyspark.sql.types import (
    StructType,
    StructField,
    StringType,
    DecimalType,
    IntegerType,
    TimestampType,
    BooleanType,
    BinaryType,
    LongType,
)

time_series_received_schema = StructType(
    [
        StructField("enqueuedTime", TimestampType(), True),
        StructField("body", StringType(), True),
    ]
)


@pytest.fixture(scope="session")
def time_series_persister(spark, delta_lake_path, integration_tests_path):
    checkpoint_path = f"{delta_lake_path}/unprocessed_time_series/checkpoint"
    time_series_unprocessed_path = f"{delta_lake_path}/unprocessed_time_series"
    if(os.path.exists(time_series_unprocessed_path)):
        shutil.rmtree(time_series_unprocessed_path)
    streamingDf = spark.readStream.schema(time_series_received_schema).json(
        f"{integration_tests_path}/time_series_persister/time_series_received*.json"
    )
    job = timeseries_persister(
        streamingDf, checkpoint_path, time_series_unprocessed_path
    )

    return job
