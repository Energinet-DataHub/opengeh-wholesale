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

from enum import Enum
import pytest

import package.codelists as enums

from tests.contract_utils import assert_codelist_matches_contract


@pytest.mark.parametrize(
    "contract_file,code_list",
    [
        ("aggregation-level.json", enums.AggregationLevel),
        ("basis-data-type.json", enums.BasisDataType),
        ("charge-quality.json", enums.ChargeQuality),
        ("charge-resolution.json", enums.ChargeResolution),
        ("charge-type.json", enums.ChargeType),
        ("charge-unit.json", enums.ChargeUnit),
        ("metering-point-resolution.json", enums.MeteringPointResolution),
        ("metering-point-type.json", enums.MeteringPointType),
        ("process-type.json", enums.ProcessType),
        ("quantity-quality.json", enums.QuantityQuality),
        ("settlement-method.json", enums.SettlementMethod),
        ("time-series-type.json", enums.TimeSeriesType),
        ("amount-type.json", enums.AmountType),
    ],
)
def test_codelist_matches_contract(
    contracts_path: str, contract_file: str, code_list: Enum
) -> None:
    contract_path = f"{contracts_path}/enums/{contract_file}"
    assert_codelist_matches_contract(code_list, contract_path)
