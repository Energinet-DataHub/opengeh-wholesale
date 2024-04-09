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
GIVEN two metering points (positive and negative grid loss)
  AND only one charge link
  AND the charge link starts on February 27th
  AND the subscription price is 28.282828 DKK
WHEN calculating subscription amount per charge for February
THEN there is only result rows for 27th and 28th of february
  AND the subscription amount is 1.010101 DKK
```
"""
from features.utils.base import Base


class TestThen(Base):
    pass
