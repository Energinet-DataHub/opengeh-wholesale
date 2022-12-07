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

from geh_stream.codelists import Colname
from geh_stream.schemas.output import aggregation_result_schema
from tests.helpers import DataframeDefaults
import pytest
import pandas as pd


@pytest.fixture(scope="module")
def aggregation_result_factory(spark):
    def factory(
        job_id=DataframeDefaults.default_job_id,
        snapshot_id=DataframeDefaults.default_snapshot_id,
        result_id=DataframeDefaults.default_result_id,
        result_name=DataframeDefaults.default_result_name,
        result_path=DataframeDefaults.default_result_path,
        grid_area=DataframeDefaults.default_grid_area,
        in_grid_area=None,
        out_grid_area=None,
        balance_responsible_id=None,
        energy_supplier_id=None,
        time_window_start=DataframeDefaults.default_time_window_start,
        time_window_end=DataframeDefaults.default_time_window_end,
        resolution=DataframeDefaults.default_resolution,
        sum_quantity=DataframeDefaults.default_sum_quantity,
        quality=DataframeDefaults.default_quality,
        metering_point_type=DataframeDefaults.default_metering_point_type,
        settlement_method=None,
        added_grid_loss=None,
        added_system_correction=None
    ):
        pandas_df = pd.DataFrame().append([{
            Colname.job_id: job_id,
            Colname.snapshot_id: snapshot_id,
            Colname.result_id: result_id,
            Colname.result_name: result_name,
            Colname.result_path: result_path,
            Colname.grid_area: grid_area,
            Colname.in_grid_area: in_grid_area,
            Colname.out_grid_area: out_grid_area,
            Colname.balance_responsible_id: balance_responsible_id,
            Colname.energy_supplier_id: energy_supplier_id,
            Colname.time_window: {
                Colname.start: time_window_start,
                Colname.end: time_window_end},
            Colname.resolution: resolution,
            Colname.sum_quantity: sum_quantity,
            Colname.quality: quality,
            Colname.metering_point_type: metering_point_type,
            Colname.settlement_method: settlement_method,
            Colname.added_grid_loss: added_grid_loss,
            Colname.added_system_correction: added_system_correction
            }],
            ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=aggregation_result_schema)
    return factory
