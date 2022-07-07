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

import subprocess


def test_calculator(databricks_path):
    exit_code = subprocess.call(
        [
            "python",
            f"{databricks_path}/package/calculator.py",
            "--data-storage-account-name",
            "data-storage-account-name",
            "--data-storage-account-key",
            "data-storage-account-key",
            "--integration-events-path",
            "foo",
            "--time-series-points-path",
            "foo",
            "--process-results-path",
            "foo",
        ]
    )

    # Assert
    assert exit_code == 0, "Calculator must return exit code 0"
