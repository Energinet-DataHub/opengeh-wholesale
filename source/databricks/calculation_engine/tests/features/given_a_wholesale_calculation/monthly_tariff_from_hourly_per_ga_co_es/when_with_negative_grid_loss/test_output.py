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
  AND 5 hours for consumption metering point (5 rows)
WHEN calculating the monthly negative grid loss for February
THEN the aggregated amount is 37.849900 for negative grid loss (system correction)
```

```text
Negative grid loss is calculated like this:

(((Σ Exchange in - Σ Exchange out) + Σ Production) - (Σ Consumption non-profiled + Σ Consumption flex))) = grid loss per hour
grid loss per hour * hours per day * days per month = total grid loss

(((0 - 0) + 0) - (0 + 10)) = -10 * 5 = -50

tariff * total negative grid loss = amount
0.756998 * -50 = -37.849900
```
"""
from features.utils.base import Base


class TestThen(Base):
    pass
