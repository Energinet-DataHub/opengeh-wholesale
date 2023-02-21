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

from tests.contract_utils import assert_codelist_matches_contract
from package.codelists import TimeSeriesQuality


def test_quality_codelist_matches_contract(contracts_path):
    contract_path = f"{contracts_path}/internal/time-series-point-quality.json"
    assert_codelist_matches_contract(TimeSeriesQuality, contract_path)
