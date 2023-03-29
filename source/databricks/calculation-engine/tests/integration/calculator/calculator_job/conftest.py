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

from os import path
from shutil import rmtree
from pyspark.sql import SparkSession
import pytest
from . import configuration as C
from package.calculator_job import (
    _start_calculator,
)
from package.calculator_args import CalculatorArgs
import package.infrastructure as infra
from package.schemas import time_series_point_schema, metering_point_period_schema
from typing import Callable, Optional
from datetime import datetime


@pytest.fixture(scope="session")
def test_data_job_parameters(
    data_lake_path: str,
    timestamp_factory: Callable[[str], Optional[datetime]],
    worker_id: str,
) -> CalculatorArgs:
    return C.DictObj(
        {
            "data_storage_account_name": "foo",
            "data_storage_account_key": "foo",
            "wholesale_container_path": f"{data_lake_path}/{worker_id}",
            "batch_id": C.executed_batch_id,
            "batch_process_type": "BalanceFixing",
            "batch_grid_areas": [805, 806],
            "batch_period_start_datetime": timestamp_factory(
                "2018-01-01T23:00:00.000Z"
            ),
            "batch_period_end_datetime": timestamp_factory("2018-01-03T23:00:00.000Z"),
            "batch_execution_time_start": timestamp_factory("2018-01-05T23:00:00.000Z"),
            "time_zone": "Europe/Copenhagen",
        }
    )


@pytest.fixture(scope="session")
def executed_calculation_job(
    spark: SparkSession,
    test_data_job_parameters: CalculatorArgs,
    test_files_folder_path: str,
    data_lake_path: str,
    worker_id: str,
) -> None:
    """Execute the calculator job.
    This is the act part of a test in the arrange-act-assert paradigm.
    This act is made as a session-scoped fixture because it is a slow process
    and because lots of assertions can be made and split into seperate tests
    without awaiting the execution in each test."""

    output_path = f"{data_lake_path}/{worker_id}/{infra.OUTPUT_FOLDER}"

    if path.isdir(output_path):
        # Since we are appending the result dataframes we must ensure that the path is removed before executing the tests
        rmtree(output_path)

    metering_points_df = spark.read.csv(
        f"{test_files_folder_path}/MeteringPointsPeriods.csv",
        header=True,
        schema=metering_point_period_schema,
    )
    metering_points_df.write.format("delta").save(
        f"{data_lake_path}/{worker_id}/calculation-input-v2/metering-point-periods",
        mode="overwrite",
    )
    timeseries_points_df = spark.read.csv(
        f"{test_files_folder_path}/TimeSeriesPoints.csv",
        header=True,
        schema=time_series_point_schema,
    )

    timeseries_points_df.write.format("delta").save(
        f"{data_lake_path}/{worker_id}/calculation-input-v2/time-series-points",
        mode="overwrite",
    )

    _start_calculator(spark, test_data_job_parameters)
