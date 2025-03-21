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
import uuid
from datetime import datetime
from unittest.mock import patch

import pytest
from pyspark import Row
from pyspark.sql import SparkSession

from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.calculator_job import start_with_deps
from geh_wholesale.databases.wholesale_internal.schemas import calculations_schema
from geh_wholesale.infrastructure.infrastructure_settings import InfrastructureSettings
from geh_wholesale.infrastructure.paths import WholesaleInternalDatabase


@pytest.mark.parametrize(
    "calculation_id_already_used",
    [True, False],
)
@patch("geh_wholesale.calculator_job.calculation.execute")
def test_start_with_deps__throws_exception_when_calculation_id_already_used(
    calculation_executor_mock,
    spark: SparkSession,
    any_calculator_args: CalculatorArgs,
    infrastructure_settings: InfrastructureSettings,
    calculation_id_already_used: bool,
    migrations_executed: bool,
) -> None:
    # Arrange
    calculation_id = str(uuid.uuid4())
    any_calculator_args.calculation_id = calculation_id

    # If true add calculation row to fake that the calculation id is already used
    if calculation_id_already_used:
        add_calculation_row(calculation_id, infrastructure_settings, spark)

    # Act
    try:
        start_with_deps(
            args=any_calculator_args,
            infrastructure_settings=infrastructure_settings,
        )
    except SystemExit as e:
        assert e.code == 4  # noqa: PT017

    # Assert
    if calculation_id_already_used:
        calculation_executor_mock.assert_not_called()
    else:
        calculation_executor_mock.assert_called()


def add_calculation_row(
    calculation_id: str,
    infrastructure_settings: InfrastructureSettings,
    spark: SparkSession,
) -> None:
    data = [
        Row(
            calculation_id=calculation_id,
            calculation_type="balance_fixing",
            calculation_period_start=datetime.now(),
            calculation_period_end=datetime.now(),
            calculation_version=1,
            calculation_execution_time_start=datetime.now(),
            calculation_succeeded_time=datetime.now(),
            is_internal_calculation=True,
        )
    ]

    calculations_df = spark.createDataFrame(data, calculations_schema)
    calculations_df.write.format("delta").mode("append").saveAsTable(
        f"{infrastructure_settings.catalog_name}.{WholesaleInternalDatabase().DATABASE_WHOLESALE_INTERNAL}.{WholesaleInternalDatabase().CALCULATIONS_TABLE_NAME}"
    )
