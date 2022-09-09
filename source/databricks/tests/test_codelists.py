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
from package.codelists import (
    ConnectionState,
    MeteringPointType,
    Quality,
    Resolution,
    SettlementMethod,
    TimeSeriesQuality,
)
from tests.contract_utils import assert_codelist_matches_contract


def test_connection_state_is_subset_of_contract(source_path):
    assert_codelist_matches_contract(
        ConnectionState, f"{source_path}/contracts/enums/connection-state.json"
    )


def test_metering_point_type_is_subset_of_contract(source_path):
    assert_codelist_matches_contract(
        MeteringPointType, f"{source_path}/contracts/enums/metering-point-type.json"
    )


def test_settlement_method_is_subset_of_contract(source_path):
    assert_codelist_matches_contract(
        SettlementMethod, f"{source_path}/contracts/enums/settlement-method.json"
    )


def test_quality_is_subset_of_contract(source_path):
    assert_codelist_matches_contract(
        Quality, f"{source_path}/contracts/enums/quality.json"
    )


def test_resolution_is_subset_of_contract(source_path):
    assert_codelist_matches_contract(
        Resolution, f"{source_path}/contracts/enums/time-series-resolution.json"
    )


def test_timeseries_quality_enum_equals_timeseries_contract(source_path):
    assert_codelist_matches_contract(
        TimeSeriesQuality, f"{source_path}/contracts/enums/timeseries-quality.json"
    )
