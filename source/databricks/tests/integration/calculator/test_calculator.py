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
import sys
import pytest
from package import calculator_job

sys.path.append(r"/workspaces/opengeh-wholesale/source/databricks")


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
            f"{databricks_path}/package/calculator_job.py",
            "--unexpected-arg",
        ]
    )

    assert (
        exit_code != 0
    ), "Calculator job should return non-zero exit when invoked with bad arguments"


# TODO BJARKE: Should we provide empty data sources instead of using `--only-validate-args`?
def test_calculator_job_accepts_parameters_from_process_manager(
    delta_lake_path, integration_tests_path, databricks_path
):
    """
    This test works in tandem with a .NET test ensuring that the calculator job accepts
    the arguments that are provided by the calling process manager.
    """

    # Arrange
    process_manager_parameters = _get_process_manager_parameters(
        f"{integration_tests_path}/calculator/test_files/calculation-job-parameters-reference.txt"
    )

    python_parameters = [
        "python",
        f"{databricks_path}/package/calculator_job.py",
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


"""
def test_calculator_job_creates_file(
    spark, delta_lake_path, find_first_file, json_lines_reader
):
    batchId = 1234
    process_results_path = f"{delta_lake_path}/results"
    integration_events_path = f"{delta_lake_path}/../calculator/test_files"

    raw_integration_events_df = spark.read.format("json").load(integration_events_path)

    calculator_job(spark, raw_integration_events_df, process_results_path, batchId)

    jsonFile = find_first_file(
        f"{delta_lake_path}/results/batch_id={batchId}/grid_area=805", "part-*.json"
    )

    result = json_lines_reader(jsonFile)
    assert len(result) > 0, "Could not verify created json file."
"""
