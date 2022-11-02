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


def test__entry_point_uncommitted_migrations_count__returns_0(installed_package):
    exit_code = subprocess.call(["uncommitted_migrations_count"])
    assert exit_code == 0


def test__entry_point_stop_jobs_exists(installed_package):
    exit_code = subprocess.call(
        [
            "stop_db_jobs",
            "--databricks-host",
            "foo",
            "--databricks-token",
            "foo",
            "--only-validate-args",
            "1",
        ]
    )
    assert exit_code == 0


def test__entry_point_start_jobs_exists(installed_package):
    exit_code = subprocess.call(
        [
            "start_db_jobs",
            "--databricks-host",
            "foo",
            "--databricks-token",
            "foo",
            "--only-validate-args",
            "1",
        ]
    )
    assert exit_code == 0


def test__entry_point_lock_storage_exists(installed_package):
    exit_code = subprocess.call(["lock_storage"])
    assert exit_code == 0


def test__entry_point_unlock_storage_exists(installed_package):
    exit_code = subprocess.call(["unlock_storage"])
    assert exit_code == 0


def test__entry_point_migrate_data_lake_exists(installed_package):
    exit_code = subprocess.call(["migrate_data_lake"])
    assert exit_code == 0
