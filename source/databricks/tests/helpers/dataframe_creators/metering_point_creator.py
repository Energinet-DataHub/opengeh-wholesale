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
from geh_stream.schemas import metering_point_schema
from tests.helpers import DataframeDefaults
import pytest
import pandas as pd


@pytest.fixture(scope="module")
def metering_point_factory(spark):
    def factory(
        from_date: datetime,
        to_date: datetime,
        metering_point_id=DataframeDefaults.default_metering_point_id,
        metering_point_type=DataframeDefaults.default_metering_point_type,
        settlement_method=DataframeDefaults.default_settlement_method,
        grid_area=DataframeDefaults.default_grid_area,
        connection_state=DataframeDefaults.default_connection_state,
        resolution=DataframeDefaults.default_resolution,
        in_grid_area=DataframeDefaults.default_in_grid_area,
        out_grid_area=DataframeDefaults.default_out_grid_area,
        metering_method=DataframeDefaults.default_metering_method,
        parent_metering_point_id=DataframeDefaults.default_parent_metering_point_id,
        unit=DataframeDefaults.default_unit,
        product=DataframeDefaults.default_product
    ):
        pandas_df = pd.DataFrame().append([{
            Colname.metering_point_id: metering_point_id,
            Colname.metering_point_type: metering_point_type,
            Colname.settlement_method: settlement_method,
            Colname.grid_area: grid_area,
            Colname.connection_state: connection_state,
            Colname.resolution: resolution,
            Colname.in_grid_area: in_grid_area,
            Colname.out_grid_area: out_grid_area,
            Colname.metering_method: metering_method,
            Colname.parent_metering_point_id: parent_metering_point_id,
            Colname.unit: unit,
            Colname.product: product,
            Colname.from_date: from_date,
            Colname.to_date: to_date}],
            ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=metering_point_schema)
    return factory
