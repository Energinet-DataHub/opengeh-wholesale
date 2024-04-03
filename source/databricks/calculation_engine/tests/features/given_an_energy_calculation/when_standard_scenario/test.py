#  Copyright 2020 Energinet DataHub A/S
#
#  Licensed under the Apache License, Version 2.0 (the "License2");
#  you may not use this file except in compliance with the License.
#  You may obtain a copy of the License at
#
#      http://www.apache.org/licenses/LICENSE-2.0
#
#  Unless required by applicable law or agreed to in writing, software
#  distributed under the License is distributed on an "AS IS" BASIS,
#  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#  See the License for the specific language governing permissions and
#  limitations under the License.
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
from pyspark.sql import DataFrame

from helpers.data_frame_utils import assert_dataframe_and_schema


# TODO: Move to location shared by all tests
def builder():  # type: ignore
    data = [
        (None, None, "basis_data/metering_point_periods_per_ga"),
        (None, None, "basis_data/metering_point_periods_per_ga_es"),
        (None, None, "energy_results/flex_consumption_per_ga"),
        (None, None, "energy_results/flex_consumption_per_ga_es"),
    ]
    print(repr(data))
    return data


# TODO: Move to location shared by all tests
# builder generates a list of tuples based on existing files in folders "basis_data", "energy_results", and "wholesale_results"
# Example of a tuple:
#   (<actual flex consumption per ga dataframe>, <expected flex consumption per ga dataframe>, "flex_consumption_per_ga")
# The builder will fail if an unexpected file is found in the folders
class Base:
    @pytest.mark.parametrize("actual, expected, output", builder())
    def test_then_actual_equals_expected(
        self, actual: DataFrame, expected: DataFrame, output: str
    ) -> None:
        assert_dataframe_and_schema(
            actual,
            expected,
        )


"""
# Tests Description

A standard scenario is a scenario that is defined as follows:

```gherkin
Given an energy calculation
When there is a production metering point
    And a consumption metering point
    And a grid loss metering point
    And an exchange metering point
    And a time series of energy values of the production and consumption metering points
Then the actual output equals the expected output
```

What has to be asserted in the "then-clause" is inferred from the files in the folders
`basis_data` and `energy_results`.
"""


class TestThen(Base):
    pass
