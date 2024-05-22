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

from features.utils.views.dataframe_wrapper import DataframeWrapper
from helpers.data_frame_utils import assert_dataframe_and_schema


def assert_output(
    actual_and_expected: tuple[list[DataframeWrapper], list[DataframeWrapper]],
    output_name: str,
    skip_columns_when_actual_and_expected_are_equal: bool = False,
) -> None:
    actual_results, expected_results = actual_and_expected

    actual_result = _get_expected_for_output(actual_results, output_name)
    expected_result = _get_expected_for_output(expected_results, output_name)

    actual_result.show()

    assert_dataframe_and_schema(
        actual_result,
        expected_result,
        ignore_decimal_precision=True,
        ignore_nullability=True,
        ignore_decimal_scale=True,
        drop_columns_when_actual_and_expected_are_equal=skip_columns_when_actual_and_expected_are_equal,
    )


def _get_expected_for_output(
    expected_results: list[DataframeWrapper], output_name: str
) -> DataFrame:
    for expected_result in expected_results:
        if expected_result.name == output_name:
            return expected_result.df

    raise Exception(f"Unknown expected name: {output_name}")
