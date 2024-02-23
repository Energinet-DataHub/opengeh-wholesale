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
from pyspark.sql import SparkSession

from business_logic_tests.features.wholesale_calculations.hourly_tariff_for_child_metering_point.states.scenario_state import (
    get_expected,
)
from business_logic_tests.scenario_factory import ScenarioFixture
from helpers.data_frame_utils import assert_dataframes


def test_execute__returns_expected(
    spark: SparkSession,
) -> None:
    # Arrange
    scenario_fixture = ScenarioFixture(spark)
    scenario_fixture.setup(get_expected)

    # Act
    results = scenario_fixture.execute()

    # Assert
    assert_dataframes(
        results.wholesale_results.hourly_tariff_per_ga_co_es.drop("metering_point_type")
        .drop("quantity_qualities")
        .drop("price")
        .drop("amount")
        .drop("energy_supplier_id")
        .drop("quantity")
        .drop("calculation_result_id"),
        scenario_fixture.expected.drop("metering_point_type")
        .drop("quantity_qualities")
        .drop("price")
        .drop("amount")
        .drop("energy_supplier_id")
        .drop("quantity")
        .drop("calculation_result_id"),
        ignore_nullability=True,
    )
