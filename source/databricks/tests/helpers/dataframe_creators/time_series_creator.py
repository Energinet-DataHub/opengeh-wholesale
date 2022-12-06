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
from datetime import datetime
from geh_stream.codelists import Colname
from geh_stream.schemas import time_series_points_schema
from tests.helpers import DataframeDefaults
import pytest
import pandas as pd


@pytest.fixture(scope="module")
def time_series_factory(spark):
    def factory(
        time: datetime,
        metering_point_id=DataframeDefaults.default_metering_point_id,
        quantity=DataframeDefaults.default_quantity,
        ts_quality=DataframeDefaults.default_quality,
        registration_date_time=DataframeDefaults.default_registration_date_time
    ):
        pandas_df = pd.DataFrame().append([{
            Colname.metering_point_id: metering_point_id,
            Colname.quantity: quantity,
            Colname.quality: ts_quality,
            Colname.time: time,
            Colname.year: time.year,
            Colname.month: time.month,
            Colname.day: time.day,
            Colname.registration_date_time: registration_date_time}],
            ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=time_series_points_schema)
    return factory
