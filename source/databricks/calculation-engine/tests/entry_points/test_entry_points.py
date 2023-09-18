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

import pkg_resources
import subprocess
from typing import Any

# IMPORTANT:
# If we add/remove tests here, we also update the "retry logic" in '.docker/entrypoint.sh',
# which dependens on the number of "entry point tests".


def _load_entry_point(entry_point_name: str) -> Any:
    # Load the entry point function from the installed wheel
    try:
        return pkg_resources.load_entry_point(
            "package", "console_scripts", entry_point_name
        )
    except pkg_resources.DistributionNotFound:
        assert False, f"The {entry_point_name} entry point was not found."


def test__entry_point__start_calculator__can_load_entry_point(
    installed_package: None,
) -> None:
    # Act
    entry_point = _load_entry_point("start_calculator")

    # Assert
    assert entry_point is not None


def test__entry_point__uncommitted_migrations_count__can_load_entry_point(
    installed_package: None,
) -> None:
    # Act
    entry_point = _load_entry_point("uncommitted_migrations_count")

    # Assert
    assert entry_point is not None


def test__entry_point__list_migrations_in_package__prints_some(
    installed_package: None,
) -> None:
    # Act
    output = subprocess.check_output(
        ["list_migrations_in_package"], universal_newlines=True
    )

    # Assert: This test will fail if the selected migration is being deleted
    migrations = output.replace("\r\n", "\n").split("\n")
    assert any("202304191400_Add_result_table" in m for m in migrations)


def test__entry_point__list_migrations_in_package__can_load_entry_point(
    installed_package: None,
) -> None:
    # Act
    entry_point = _load_entry_point("list_migrations_in_package")

    # Assert
    assert entry_point is not None


def test__entry_point__lock_storage__can_load_entry_point(
    installed_package: None,
) -> None:
    # Act
    entry_point = _load_entry_point("lock_storage")

    # Assert
    assert entry_point is not None


def test__entry_point__unlock_storage__can_load_entry_point(
    installed_package: None,
) -> None:
    # Act
    entry_point = _load_entry_point("unlock_storage")

    # Assert
    assert entry_point is not None


def test__entry_point__migrate_data_lake__can_load_entry_point(
    installed_package: None,
) -> None:
    # Act
    entry_point = _load_entry_point("migrate_data_lake")

    # Assert
    assert entry_point is not None
