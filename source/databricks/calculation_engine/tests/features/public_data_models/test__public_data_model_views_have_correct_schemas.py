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

from features.utils.views.expected_schemas.charge_link_periods_v1_schema import (
    charge_link_periods_v1_schema,
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
    schemas = get_schemas()
    errors = []

    for view in views:
        df = spark.read.format("delta").table(f"{database_name}.{view.name}")
        try:
            assert_schema(df.schema, schemas[view.name])
        except Exception as e:
            errors.append(f"{view.name}: {e}")

    assert not errors, "\n".join(errors) if errors else "All assertions passed."


def get_schemas() -> dict:
    return {
        "charge_link_periods_v1": charge_link_periods_v1_schema,
    }
