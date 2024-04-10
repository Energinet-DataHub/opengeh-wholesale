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
