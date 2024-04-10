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

"""
# Test Description

```gherkin
GIVEN grid loss and system correction metering points (positive and negative grid loss)
  AND one consumption metering point
  AND one exchange metering point
  AND one production metering point
  AND one hourly time series point for each metering point
WHEN calculating negative grid loss for 1st of February from 12pm to 1pm
THEN the calculated negative grid loss is 8.750 kWh per quarter for the 1 hour period
THEN there are 4 rows in the result
```

```text
The grid loss is calculated like this:
(((Σ Exchange in - Σ Exchange out) + Σ Production) - (Σ Consumption non-profiled + Σ Consumption flex))) / 4 = grid loss
(((5 - 0) + 10) + (0 + 50)) / 4 = -8.750

When grid loss < 0 then grid loss is negative

The number of rows is calculated by multiplying the number quarters by the number of hours times number of days.
4 * 1 * 1 = 4
```
"""

from typing import Any

import pytest

from features.utils import assert_output, get_output_names


@pytest.mark.parametrize("output_name", get_output_names())
def test__equals_expected(
    actual_and_expected: Any,
    output_name: str,
) -> None:
    assert_output(actual_and_expected, output_name)
