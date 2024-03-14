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

from package.calculation.preparation.data_structures.charge_master_data import (
    charge_master_data_schema,
)
from package.calculation.preparation.data_structures.charge_prices import (
    charge_prices_schema,
)
from package.constants import Colname

from tests.calculation.dataframe_defaults import DataframeDefaults


def test_charge_master_data(charge_master_data_factory):
    df = charge_master_data_factory().df
    result = df.collect()[0]
    assert len(df.columns) == len(charge_master_data_schema.fields)
    assert (
        result[Colname.charge_key]
        == f"{DataframeDefaults.default_charge_code}-{DataframeDefaults.default_charge_owner}-{DataframeDefaults.default_charge_type}"
    )
    assert result[Colname.charge_code] == DataframeDefaults.default_charge_code
    assert result[Colname.charge_type] == DataframeDefaults.default_charge_type
    assert result[Colname.charge_owner] == DataframeDefaults.default_charge_owner
    assert result[Colname.charge_tax] == DataframeDefaults.default_charge_tax
    assert result[Colname.resolution] == DataframeDefaults.default_charge_resolution
    assert result[Colname.from_date] == DataframeDefaults.default_from_date
    assert result[Colname.to_date] == DataframeDefaults.default_to_date


def test_charge_prices(charge_prices_factory):
    df = charge_prices_factory().df
    result = df.collect()[0]
    assert len(df.columns) == len(charge_prices_schema.fields)
    assert (
        result[Colname.charge_key]
        == f"{DataframeDefaults.default_charge_code}-{DataframeDefaults.default_charge_owner}-{DataframeDefaults.default_charge_type}"
    )
    assert result[Colname.charge_code] == DataframeDefaults.default_charge_code
    assert result[Colname.charge_type] == DataframeDefaults.default_charge_type
    assert result[Colname.charge_owner] == DataframeDefaults.default_charge_owner
    assert result[Colname.charge_time] == DataframeDefaults.default_charge_time
    assert result[Colname.charge_price] == DataframeDefaults.default_charge_price
