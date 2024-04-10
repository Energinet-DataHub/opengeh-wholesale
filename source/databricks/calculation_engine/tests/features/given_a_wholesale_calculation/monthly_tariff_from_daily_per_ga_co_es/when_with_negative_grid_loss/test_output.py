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
# Testing monthly tariff per grid area, charge owner and energy supplier with negative grid loss from daily

```gherkin
GIVEN grid loss and system correction metering points (positive and negative grid loss)
  AND one consumption metering point
  AND one exchange metering point
  AND one production metering point
  AND one monthly time series point for consumption (24 hours * 28 days = 672 rows)
  AND one monthly time series point for production  (24 hours * 28 days = 672 rows)
WHEN calculating the monthly negative grid loss for February
THEN the aggregated amount is 12015.58792 for positive grid loss (grid loss)
THEN the aggregated amount is 3795.11568 for negative grid loss (system correction)
```

```text
Negative grid loss is calculated like this:

(((Σ Exchange in - Σ Exchange out) + Σ Production) - (Σ Consumption non-profiled + Σ Consumption flex))) = grid loss
(((0 - 0) + 90) - (0 + 75)) = 3.75 * 24 * 19 = 6840
(((0 - 0) + 80) - (0 + 90)) = -2.5 * 24 * 9 = -2160

tariff * positive grid loss = amount
1.756998 * 6840 = 12017.86632

tariff * negative grid loss = amount
1.756998 * -2160 = -3795.11568
```

"""

from features.utils.base import Base


class TestThen(Base):
    pass
