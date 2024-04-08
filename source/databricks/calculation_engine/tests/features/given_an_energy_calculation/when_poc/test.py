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
from features.utils.base import Base

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
