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

from calculator_job.scenario_factory import ScenarioFactory


def test_demo(
    spark: SparkSession,
) -> None:
    # Arrange
    factory = ScenarioFactory(spark, os.path.abspath(__file__))

    # Act
    results = factory.execute_scenario()

    # Assert
    assert (
        results.wholesale_results.hourly_tariff_per_ga_co_es.schema
        == factory.get_expected_result().schema
    )
    assert (
        results.wholesale_results.hourly_tariff_per_ga_co_es.count()
        == factory.get_expected_result().count()
    )
    assert (
        results.wholesale_results.hourly_tariff_per_ga_co_es.collect()
        == factory.get_expected_result().collect()
    )
