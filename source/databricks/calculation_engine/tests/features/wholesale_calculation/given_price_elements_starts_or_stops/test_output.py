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

from typing import Any

import pytest

from features.utils.assertion import assert_output
from features.utils.scenario_output_files import get_output_names
from testsession_configuration import TestSessionConfiguration
from testcommon.etl import get_then_names, TestCase
from testcommon.dataframes import assert_dataframes_and_schemas, AssertDataframesConfiguration


# IMPORTANT:
# All test files should be identical. This makes changing them cumbersome.
# So in order to make it easier you can modify the utils/templates/calculation-test-template.py file instead,
# and then run the power-shell script "Use-Template.ps1" to update all test_output.py files.
@pytest.mark.parametrize("name", get_then_names())
def test__equals_expected(
    test_cases: TestCase,
    name: str,
    assert_dataframes_configuration: AssertDataframesConfiguration,
) -> None:
    test_case = test_cases[name]

    assert_dataframes_and_schemas(
        actual=test_case.actual,
        expected=test_case.expected,
        configuration=assert_dataframes_configuration,
    )
