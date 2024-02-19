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
import os

from pyspark.sql import SparkSession

from business_logic_tests.scenario_factory import ScenarioFactory
from helpers.data_frame_utils import assert_dataframes_equal


def test_execute_scenario__returns_expected_wholesale_results_hourly_tariff_per_ga_co_es(
    spark: SparkSession,
) -> None:
    # Arrange
    factory = ScenarioFactory(spark, os.path.abspath(__file__))

    # Act
    results = factory.execute_scenario()

    # Assert
    expected_results = factory.get_expected_result()

    assert (
        results.wholesale_results.hourly_tariff_per_ga_co_es.schema
        == expected_results.schema
    )

    results = (
        results.wholesale_results.hourly_tariff_per_ga_co_es.drop("metering_point_type")
        .drop("quantity_qualities")
        .drop("price")
        .drop("amount")
        .drop("energy_supplier_id")
        .drop("quantity")
        .drop("calculation_result_id")
    )
    expected_results = (
        expected_results.drop("metering_point_type")
        .drop("quantity_qualities")
        .drop("price")
        .drop("amount")
        .drop("energy_supplier_id")
        .drop("quantity")
        .drop("calculation_result_id")
    )

    assert_dataframes_equal(results, expected_results)
