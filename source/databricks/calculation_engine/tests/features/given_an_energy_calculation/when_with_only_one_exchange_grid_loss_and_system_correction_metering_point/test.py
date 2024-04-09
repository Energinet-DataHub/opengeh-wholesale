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
from features.utils.base import Base

"""
# Tests Description

```gherkin
GIVEN one exchange metering point
AND one grid loss metering point
AND one system correction metering point
AND time series on the exchange MP is 75 kWh per hour
WHEN calculating flex_consumption_per_ga
THEN flex consumption per grid area is 75/4 = 18.75
THEN there are four rows
```

"""


class TestThen(Base):
    pass
