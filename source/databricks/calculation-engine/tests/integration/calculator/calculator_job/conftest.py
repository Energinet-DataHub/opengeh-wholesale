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

from datetime import datetime
from pyspark.sql import SparkSession, DataFrame
import pytest
from typing import Callable, Optional

from . import configuration as C
from package.calculator_job import (
    _start_calculator,
)
from package.calculator_args import CalculatorArgs
from package.schemas import time_series_point_schema, metering_point_period_schema
from package.output_writers.calculation_result_writer import (
    DATABASE_NAME,
    RESULT_TABLE_NAME,
)


@pytest.fixture(scope="session")
def test_data_job_parameters(
    data_lake_path: str,
    timestamp_factory: Callable[[str], Optional[datetime]],
) -> CalculatorArgs:
    return C.DictObj(
        {
            "data_storage_account_name": "foo",
            "wholesale_container_path": f"{data_lake_path}",
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
    migrations_executed: None,
) -> None:
    """Execute the calculator job.
    This is the act part of a test in the arrange-act-assert paradigm.
    This act is made as a session-scoped fixture because it is a slow process
    and because lots of assertions can be made and split into seperate tests
    without awaiting the execution in each test."""

    metering_points_df = spark.read.csv(
        f"{test_files_folder_path}/MeteringPointsPeriods.csv",
        header=True,
        schema=metering_point_period_schema,
    )
    metering_points_df.write.format("delta").save(
        f"{data_lake_path}/calculation_input/metering_point_periods",
        mode="overwrite",
    )
    timeseries_points_df = spark.read.csv(
        f"{test_files_folder_path}/TimeSeriesPoints.csv",
        header=True,
        schema=time_series_point_schema,
    )

    timeseries_points_df.write.format("delta").save(
        f"{data_lake_path}/calculation_input/time_series_points",
        mode="overwrite",
    )

    _start_calculator(spark, test_data_job_parameters)


@pytest.fixture(scope="session")
def results_df(
    spark: SparkSession,
    executed_calculation_job: None,
) -> DataFrame:
    return spark.read.table(f"{DATABASE_NAME}.{RESULT_TABLE_NAME}")
