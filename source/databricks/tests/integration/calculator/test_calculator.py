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
from package import initialize_spark
from pyspark.sql.functions import col

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
    delta_lake_path, source_path, databricks_path
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


def test_calculator_job_creates_file(
    json_test_files, databricks_path, delta_lake_path, source_path
):
    spark.read.json(f"{json_test_files}/integration_events.json").withColumn(
        "body", col("body").cast("binary")
    ).write.mode("overwrite").parquet(
        f"{delta_lake_path}/parquet_test_files/integration_events"
    )
    spark.read.json(f"{json_test_files}/time_series_points.json").write.mode(
        "overwrite"
    ).parquet(f"{delta_lake_path}/parquet_test_files/time_series_points")
    # Arrange
    python_parameters = [
        "python",
        f"{databricks_path}/package/calculator_job_v2_draft.py",
        "--data-storage-account-name",
        "foo",
        "--data-storage-account-key",
        "foo",
        "--integration-events-path",
        f"{delta_lake_path}/parquet_test_files/integration_events",
        "--time-series-points-path",
        f"{delta_lake_path}/parquet_test_files/time_series_points",
        "--process-results-path",
        f"{delta_lake_path}/result",
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

    # Act
    subprocess.call(python_parameters)

    # Assert
    result_805 = spark.read.json(f"{delta_lake_path}/result/batch-id=1/grid-area=805")
    result_806 = spark.read.json(f"{delta_lake_path}/result/batch-id=1/grid-area=806")
    assert result_805.count() >= 1, "Calculator job failed to write files"
    assert result_806.count() >= 1, "Calculator job failed to write files"
