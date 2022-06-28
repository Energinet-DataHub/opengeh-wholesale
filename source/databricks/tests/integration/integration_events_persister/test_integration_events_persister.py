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

import sys
import os
from pyspark.sql import SparkSession
sys.path.append(r"/workspaces/geh-timeseries/source/databricks")

import asyncio
import pytest
from package import timeseries_persister
from tests.integration.utils import streaming_job_asserter


@pytest.mark.asyncio
async def test_stores_received_time_series_in_delta_table(delta_reader, time_series_persister):
    def verification_function():
        data = delta_reader("/unprocessed_time_series")
        return data.count() > 0

    succeeded = streaming_job_asserter(time_series_persister, verification_function)
    assert succeeded, "No data was stored in Delta table"
