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

import pytest
from pyspark.sql import SparkSession

from features.utils.views.expected_schemas import (
    charge_link_periods_v1_schema,
    charge_prices_v1_schema,
    current_balance_fixing_calculation_version_v1_schema,
    energy_result_points_per_es_ga_v1_schema,
    energy_result_points_per_ga_v1_schema,
    metering_point_periods_v1_schema,
    metering_point_time_series_v1_schema,
    monthly_amounts_v1_schema,
    wholesale_results_v1_schema,
    energy_per_brp_ga_v1_schema,
    energy_per_es_brp_ga_v1_schema,
    energy_per_ga_v1_schema,
    exchange_per_neighbor_ga_v1_schema,
)
from package.common import assert_schema


@pytest.mark.parametrize(
    "database_name",
    [
        "settlement_report",
        "wholesale_calculation_results",
    ],
)
def test__public_data_model_views_have_correct_schemas(
    migrations_executed: None,
    spark: SparkSession,
    database_name: str,
) -> None:
    """Verify that all schemas from all views in all public view models match the respective expected schema."""

    spark.catalog.setCurrentDatabase(database_name)
    views = spark.catalog.listTables()
    schemas = get_expected_schemas()
    errors = []

    for view in views:
        try:
            actual_df = spark.read.format("delta").table(f"{database_name}.{view.name}")
            expected_df = spark.createDataFrame([], schemas[view.name])

            assert_schema(
                actual_df.schema,
                expected_df.schema,
                ignore_nullability=True,
            )
        except Exception as e:
            errors.append(f"{view.name}: {e}")

    assert not errors, "\n".join(errors) if errors else "All assertions passed."


def get_expected_schemas() -> dict:
    return {
        # settlement_report
        "charge_link_periods_v1": charge_link_periods_v1_schema,
        "charge_prices_v1": charge_prices_v1_schema,
        "current_balance_fixing_calculation_version_v1": current_balance_fixing_calculation_version_v1_schema,
        "energy_result_points_per_es_ga_v1": energy_result_points_per_es_ga_v1_schema,
        "energy_result_points_per_ga_v1": energy_result_points_per_ga_v1_schema,
        "metering_point_periods_v1": metering_point_periods_v1_schema,
        "metering_point_time_series_v1": metering_point_time_series_v1_schema,
        "monthly_amounts_v1": monthly_amounts_v1_schema,
        "wholesale_results_v1": wholesale_results_v1_schema,
        # Calculation results
        "energy_per_brp_ga_v1": energy_per_brp_ga_v1_schema,
        "energy_per_es_brp_ga_v1": energy_per_es_brp_ga_v1_schema,
        "energy_per_ga_v1": energy_per_ga_v1_schema,
        "exchange_per_neighbor_ga_v1": exchange_per_neighbor_ga_v1_schema,
    }
