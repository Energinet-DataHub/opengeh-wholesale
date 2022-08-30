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
import subprocess
import pytest
from package import initialize_spark, calculator
from pyspark.sql.functions import col
from tests.contract_utils import assert_contract_matches_schema
from pyspark.sql.types import (
    DecimalType,
    StructType,
    StructField,
    StringType,
    TimestampType,
    IntegerType,
    LongType,
)

spark = initialize_spark("foo", "bar")


def _get_process_manager_parameters(filename):
    """Get the parameters as they are expected to be received from the process manager."""
    with open(filename) as file:
        text = file.read()
        text = text.replace("{batch-id}", "any-guid-id")
        lines = text.splitlines()
        return list(
            filter(lambda line: not line.startswith("#") and len(line) > 0, lines)
        )


def test_calculator_job_when_invoked_with_incorrect_parameters_fails(
    integration_tests_path, databricks_path
):
    exit_code = subprocess.call(
        [
            "python",
            f"{databricks_path}/package/calculator_job_v2_draft.py",
            "--unexpected-arg",
        ]
    )

    assert (
        exit_code != 0
    ), "Calculator job should return non-zero exit when invoked with bad arguments"


def test_calculator_job_accepts_parameters_from_process_manager(
    data_lake_path, source_path, databricks_path
):
    """
    This test works in tandem with a .NET test ensuring that the calculator job accepts
    the arguments that are provided by the calling process manager.
    """

    # Arrange
    process_manager_parameters = _get_process_manager_parameters(
        f"{source_path}/contracts/calculation-job-parameters-reference.txt"
    )

    python_parameters = [
        "python",
        f"{databricks_path}/package/calculator_job_v2_draft.py",
        "--data-storage-account-name",
        "foo",
        "--data-storage-account-key",
        "foo",
        "--integration-events-path",
        "foo",
        "--time-series-points-path",
        "foo",
        "--process-results-path",
        "foo",
        "--only-validate-args",
        "1",
    ]
    python_parameters.extend(process_manager_parameters)

    # Act
    exit_code = subprocess.call(python_parameters)

    # Assert
    assert exit_code == 0, "Calculator job failed to accept provided input arguments"


def test_calculator_job_creates_files_for_each_gridarea(
    json_test_files, databricks_path, data_lake_path, source_path
):
    # Reads integration_events json file into dataframe and writes it to parquet
    spark.read.json(f"{json_test_files}/integration_events.json").withColumn(
        "body", col("body").cast("binary")
    ).write.mode("overwrite").parquet(
        f"{data_lake_path}/parquet_test_files/integration_events"
    )

    # Schema should match published_time_series_points_schema in time series
    published_time_series_points_schema = StructType(
        [
            StructField("GsrnNumber", StringType(), True),
            StructField("TransactionId", StringType(), True),
            StructField("Quantity", DecimalType(18, 3), True),
            StructField("Quality", LongType(), True),
            StructField("Resolution", LongType(), True),
            StructField("RegistrationDateTime", TimestampType(), True),
            StructField("storedTime", TimestampType(), False),
            StructField("time", TimestampType(), True),
            StructField("year", IntegerType(), True),
            StructField("month", IntegerType(), True),
            StructField("day", IntegerType(), True),
        ]
    )

    # Reads time_series_points json file into dataframe with published_time_series_points_schema and writes it to parquet
    spark.read.schema(published_time_series_points_schema).json(
        f"{json_test_files}/time_series_points.json"
    ).write.mode("overwrite").parquet(
        f"{data_lake_path}/parquet_test_files/time_series_points"
    )

    # Arrange
    python_parameters = [
        "python",
        f"{databricks_path}/package/calculator_job_v2_draft.py",
        "--data-storage-account-name",
        "foo",
        "--data-storage-account-key",
        "foo",
        "--integration-events-path",
        f"{data_lake_path}/parquet_test_files/integration_events",
        "--time-series-points-path",
        f"{data_lake_path}/parquet_test_files/time_series_points",
        "--process-results-path",
        f"{data_lake_path}/result",
        "--batch-id",
        "1",
        "--batch-grid-areas",
        "[805, 806]",
        "--batch-snapshot-datetime",
        "2022-09-02T21:59:00Z",
        "--batch-period-start-datetime",
        "2022-04-01T22:00:00Z",
        "--batch-period-end-datetime",
        "2022-09-01T22:00:00Z",
    ]

    # Reads time_series_points from parquet into dataframe
    input_time_series_points = spark.read.parquet(
        f"{data_lake_path}/parquet_test_files/time_series_points"
    )

    # Act
    subprocess.call(python_parameters)

    # Assert
    result_805 = spark.read.json(f"{data_lake_path}/result/batch-id=1/grid-area=805")
    result_806 = spark.read.json(f"{data_lake_path}/result/batch-id=1/grid-area=806")
    assert result_805.count() >= 1, "Calculator job failed to write files"
    assert result_806.count() >= 1, "Calculator job failed to write files"

    result_805.printSchema()
    result_805.show()

    # Asserts that the published-time-series-points contract matches the schema from input_time_series_points.
    # When asserting both that the calculator creates output and it does it with input data that matches
    # the time series points contract from the time-series domain (in the same test), then we can infer that the
    # calculator works with the format of the data published from the time-series domain.
    assert_contract_matches_schema(
        f"{source_path}/contracts/events/published-time-series-points.json",
        input_time_series_points.schema,
    )


def test_calculator_creates_file(
    spark, data_lake_path, find_first_file, json_lines_reader
):
    batchId = 1234
    process_results_path = f"{data_lake_path}/results"

    calculator(spark, process_results_path, batchId)

    # This
    result_path = f"{data_lake_path}/results/batch_id={batchId}/grid_area=805"
    jsonFile = find_first_file(result_path, "part-*.json")

    result = json_lines_reader(jsonFile)
    assert len(result) > 0, "Could not verify created json file."
