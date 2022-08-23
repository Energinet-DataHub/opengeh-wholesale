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

import pytest
from package.codelists import ConnectionState, MeteringPointType, Resolution
from tests.contract_utils import assert_contract_matches_literal


def test_connection_state(source_path):
    assert_contract_matches_literal(
        f"{source_path}/contracts/literals/connection-state.json", ConnectionState
    )


def test_metering_point_type(source_path):
    assert_contract_matches_literal(
        f"{source_path}/contracts/literals/metering-point-type.json", MeteringPointType
    )


def test_resolution(source_path):
    assert_contract_matches_literal(
        f"{source_path}/contracts/literals/time-series-resolution.json", Resolution
    )
