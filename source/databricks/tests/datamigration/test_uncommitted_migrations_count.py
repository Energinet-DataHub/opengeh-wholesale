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
import subprocess


def test_uncommitted_migrations_count_when_invoked_with_incorrect_parameters_fails(
    databricks_path,
):
    exit_code = subprocess.call(
        [
            "python",
            f"{databricks_path}/package/datamigration/uncommitted_migrations_count.py",
            "--unexpected-arg",
        ]
    )

    assert (
        exit_code != 0
    ), "Expected to return non-zero exit code when invoked with bad arguments"
