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
from helpers.data_frame_utils import (
    assert_dataframe_and_schema,
)
from .states.scenario_state import (
    get_expected,
)


def test_execute__returns_expected(  # type: ignore
    scenario_fixture,
) -> None:
    # Arrange
    scenario_fixture.setup(get_expected)

    # Act
    results = scenario_fixture.execute()

    results.energy_results.flex_consumption_per_ga_and_brp_and_es.show()
    scenario_fixture.expected.show()

    actual = (
        results.energy_results.flex_consumption_per_ga_and_brp_and_es.drop(
            "aggregation_level"
        )
        .drop("time_series_type")
        .drop("calculation_execution_time_start")
        .drop("from_grid_area")
        .drop("calculation_result_id")
        .drop("metering_point_id")
    )

    exp = (
        scenario_fixture.expected.drop("aggregation_level")
        .drop("time_series_type")
        .drop("calculation_execution_time_start")
        .drop("from_grid_area")
        .drop("calculation_result_id")
        .drop("metering_point_id")
    )

    # Assert
    assert_dataframe_and_schema(
        actual,
        exp,
        ignore_decimal_precision=True,
        ignore_decimal_scale=True,
        ignore_nullability=True,
    )
