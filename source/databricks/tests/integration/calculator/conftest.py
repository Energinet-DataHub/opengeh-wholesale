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
import glob
import os
from pathlib import Path
from pyspark.sql.types import (
    DecimalType,
    StructType,
    StructField,
    StringType,
    TimestampType,
    IntegerType,
    LongType,
)
from pyspark.sql.functions import col


@pytest.fixture(scope="session")
def json_lines_reader():
    def f(path: str):
        return Path(path).read_text()

    return f


@pytest.fixture(scope="session")
def find_first_file():
    def f(path: str, pattern: str):
        os.chdir(path)
        for filePath in glob.glob(pattern):
            return filePath
        raise Exception("Target test file not found.")

    return f


@pytest.fixture(scope="session")
def test_data(spark, json_test_files, data_lake_path, worker_id):
    print("hej")
    # # Reads integration_events json file into dataframe and writes it to parquet
    # spark.read.json(f"{json_test_files}/integration_events.json").withColumn(
    #     "body", col("body").cast("binary")
    # ).write.mode("overwrite").parquet(
    #     f"{data_lake_path}/parquet_test_files/{worker_id}/integration_events"
    # )

    # # Schema should match published_time_series_points_schema in time series
    # published_time_series_points_schema = StructType(
    #     [
    #         StructField("GsrnNumber", StringType(), True),
    #         StructField("TransactionId", StringType(), True),
    #         StructField("Quantity", DecimalType(18, 3), True),
    #         StructField("Quality", LongType(), True),
    #         StructField("Resolution", LongType(), True),
    #         StructField("RegistrationDateTime", TimestampType(), True),
    #         StructField("storedTime", TimestampType(), False),
    #         StructField("time", TimestampType(), True),
    #         StructField("year", IntegerType(), True),
    #         StructField("month", IntegerType(), True),
    #         StructField("day", IntegerType(), True),
    #     ]
    # )

    # # Reads time_series_points json file into dataframe with published_time_series_points_schema and writes it to parquet
    # spark.read.schema(published_time_series_points_schema).json(
    #     f"{json_test_files}/time_series_points.json"
    # ).write.mode("overwrite").parquet(
    #     f"{data_lake_path}/parquet_test_files/{worker_id}/time_series_points"
    # )


# Code snippet from https://joelmccune.com/python-dictionary-as-object/
class DictObj:
    def __init__(self, in_dict: dict):
        assert isinstance(in_dict, dict)
        for key, val in in_dict.items():
            if isinstance(val, (list, tuple)):
                setattr(
                    self, key, [DictObj(x) if isinstance(x, dict) else x for x in val]
                )
            else:
                setattr(self, key, DictObj(val) if isinstance(val, dict) else val)


@pytest.fixture
def test_data_job_parameters(
    test_data, databricks_path, data_lake_path, timestamp_factory, worker_id
):
    "test_data parameter ensures that the corresponding test data has been created when using these corresponding job parameters"
    return DictObj(
        {
            "data_storage_account_name": "foo",
            "data_storage_account_key": "foo",
            "integration_events_path": f"{data_lake_path}/parquet_test_files/integration_events",
            "time_series_points_path": f"{data_lake_path}/parquet_test_files/time_series_points",
            "process_results_path": f"{data_lake_path}/{worker_id}/results",
            "batch_id": "1",
            "batch_grid_areas": [805, 806],
            "batch_snapshot_datetime": timestamp_factory("2022-09-02T21:59:00.000Z"),
            "batch_period_start_datetime": timestamp_factory(
                "2022-04-01T22:00:00.000Z"
            ),
            "batch_period_end_datetime": timestamp_factory("2022-09-01T22:00:00.000Z"),
            "time_zone": "Europe/Copenhagen",
        }
    )
