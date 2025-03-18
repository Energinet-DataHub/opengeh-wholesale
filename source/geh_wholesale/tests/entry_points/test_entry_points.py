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


# IMPORTANT:
# If we add/remove tests here, we also update the "retry logic" in '.docker/entrypoint.sh',
# which depends on the number of "entry point tests".
import tomllib

import pytest

from tests import PROJECT_PATH


@pytest.mark.parametrize(
    "entry_point_name",
    [
        "start_calculator",
        "migrate_data_lake",
        "optimize_delta_tables",
    ],
)
def test__entry_point_exists(entry_point_name: str) -> None:
    with open(PROJECT_PATH / "pyproject.toml", "rb") as file:
        pyproject = tomllib.load(file)
        project = pyproject.get("project", {})
    scripts = project.get("scripts", {})
    assert entry_point_name in scripts, "`execute_electrical_heating` not found in scripts"
