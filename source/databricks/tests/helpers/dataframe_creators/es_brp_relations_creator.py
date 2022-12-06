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
from geh_stream.schemas import es_brp_relations_schema
from tests.helpers import DataframeDefaults
import pytest
import pandas as pd


@pytest.fixture(scope="module")
def es_brp_relations_factory(spark):
    def factory(
        from_date: datetime,
        to_date: datetime,
        energy_supplier_id=DataframeDefaults.default_energy_supplier_id,
        balance_responsible_id=DataframeDefaults.default_balance_responsible_id,
        grid_area=DataframeDefaults.default_grid_area,
        metering_point_type=DataframeDefaults.default_metering_point_type
    ):
        pandas_df = pd.DataFrame().append([{
            Colname.energy_supplier_id: energy_supplier_id,
            Colname.balance_responsible_id: balance_responsible_id,
            Colname.grid_area: grid_area,
            Colname.metering_point_type: metering_point_type,
            Colname.from_date: from_date,
            Colname.to_date: to_date}],
            ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=es_brp_relations_schema)
    return factory
