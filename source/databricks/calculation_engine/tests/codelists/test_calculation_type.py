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

from package.codelists import CalculationType
from package.codelists.calculation_type import is_wholesale_calculation_type


def test__is_wholesale_calculation_type__returns_expected():
    # Arrange
    expected_results = {
        CalculationType.BALANCE_FIXING: False,
        CalculationType.AGGREGATION: False,
        CalculationType.WHOLESALE_FIXING: True,
        CalculationType.FIRST_CORRECTION_SETTLEMENT: True,
        CalculationType.SECOND_CORRECTION_SETTLEMENT: True,
        CalculationType.THIRD_CORRECTION_SETTLEMENT: True,
    }

    for calculation_type in CalculationType:
        # Act
        result = is_wholesale_calculation_type(calculation_type)

        # Assert
        assert calculation_type in expected_results.keys()
        assert result == expected_results[calculation_type]
