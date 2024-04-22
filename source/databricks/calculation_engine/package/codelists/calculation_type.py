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


class CalculationType(Enum):
    BALANCE_FIXING = "BalanceFixing"
    AGGREGATION = "Aggregation"
    WHOLESALE_FIXING = "WholesaleFixing"
    FIRST_CORRECTION_SETTLEMENT = "FirstCorrectionSettlement"
    SECOND_CORRECTION_SETTLEMENT = "SecondCorrectionSettlement"
    THIRD_CORRECTION_SETTLEMENT = "ThirdCorrectionSettlement"


def is_wholesale_calculation_type(calculation_type: CalculationType) -> bool:
    return (
        calculation_type == CalculationType.WHOLESALE_FIXING
        or calculation_type == CalculationType.FIRST_CORRECTION_SETTLEMENT
        or calculation_type == CalculationType.SECOND_CORRECTION_SETTLEMENT
        or calculation_type == CalculationType.THIRD_CORRECTION_SETTLEMENT
    )
