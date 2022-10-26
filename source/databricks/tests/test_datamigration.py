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

from datamigration.start_migration import _stop_jobs, _start_jobs


def test__stop_jobs(spark):
    # create sparksession to trigger download of spark dependencies. This is nessesary before
    # runing pytest in parralel with xdist. Else each work will conflict which each other when
    # trying to download dependencies to the same folder
    _stop_all_jobs()
    assert 1 == 1


def test__start_jobs(spark):
    # create sparksession to trigger download of spark dependencies. This is nessesary before
    # runing pytest in parralel with xdist. Else each work will conflict which each other when
    # trying to download dependencies to the same folder
    _start_jobs(["persister_streaming_job"])
    assert 1 == 1
