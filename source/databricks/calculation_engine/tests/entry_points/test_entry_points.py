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

import importlib.metadata
from typing import Any

import pytest


# IMPORTANT:
# If we add/remove tests here, we also update the "retry logic" in '.docker/entrypoint.sh',
# which depends on the number of "entry point tests".


def assert_entry_point_exists(entry_point_name: str) -> Any:
    # Load the entry point function from the installed wheel
    try:
        entry_point = importlib.metadata.entry_points(
            group="console_scripts", name=entry_point_name
        )
        if not entry_point:
            assert False, f"The {entry_point_name} entry point was not found."
    except importlib.metadata.PackageNotFoundError:
        assert False, f"The {entry_point_name} entry point was not found."


@pytest.mark.parametrize(
    "entry_point_name",
    [
        "start_calculator",
        "lock_storage",
        "unlock_storage",
        "migrate_data_lake",
    ],
)
def test__entry_point__start_calculator__can_load_entry_point(
    installed_package: None,
    entry_point_name: str,
) -> None:
    assert_entry_point_exists(entry_point_name)
