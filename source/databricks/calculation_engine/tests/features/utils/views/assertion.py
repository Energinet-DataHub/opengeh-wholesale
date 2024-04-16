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

from pyspark.sql import DataFrame

from features.utils.expected_output import ExpectedOutput
from helpers.data_frame_utils import assert_dataframe_and_schema


def assert_view_output(
    actual_and_expected: tuple[list[ExpectedOutput], list[ExpectedOutput]],
    output_name: str,
) -> None:
    actual_results, expected_results = actual_and_expected

    actual_result = _get_expected(actual_results, output_name)
    expected_result = _get_expected(expected_results, output_name)

    assert_dataframe_and_schema(
        actual_result,
        expected_result,
        ignore_decimal_precision=True,
        ignore_nullability=True,
        ignore_decimal_scale=True,
    )


def _get_expected(
    expected_results: list[ExpectedOutput], output_name: str
) -> DataFrame:
    for expected_result in expected_results:
        if expected_result.name == output_name:
            return expected_result.df

    raise Exception(f"Unknown expected result name: {output_name}")
