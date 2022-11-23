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


def test__entry_point__start_stream__returns_0(installed_package):
    exit_code = subprocess.call(["start_stream", "-h"])
    assert exit_code == 0


def test__entry_point__start_calculator__returns_0(installed_package):
    exit_code = subprocess.call(["start_calculator", "-h"])
    assert exit_code == 0


def test__entry_point__uncommitted_migrations_count__returns_0(installed_package):
    exit_code = subprocess.call(["uncommitted_migrations_count", "-h"])
    assert exit_code == 0


def test__entry_point__lock_storage__returns_0(installed_package):
    exit_code = subprocess.call(["lock_storage", "-h"])
    assert exit_code == 0


def test__entry_point__unlock_storage__returns_0(installed_package):
    exit_code = subprocess.call(["unlock_storage", "-h"])
    assert exit_code == 0


def test__entry_point__migrate_data_lake__returns_0(installed_package):
    exit_code = subprocess.call(["migrate_data_lake", "-h"])
    assert exit_code == 0
