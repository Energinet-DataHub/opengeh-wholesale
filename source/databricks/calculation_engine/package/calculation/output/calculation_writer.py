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
from dependency_injector.wiring import inject, Provide
from pyspark.sql import SparkSession, Row

from package.container import Container
from package.infrastructure import logging_configuration, paths
from package.calculation.calculator_args import CalculatorArgs
from package.constants.calculation_column_names import CalculationColumnNames


@logging_configuration.use_span("calculation.write-succeeded-calculation")
@inject
def write_calculation(
    args: CalculatorArgs,
    spark: SparkSession = Provide[Container.spark],
) -> None:
    calculation = {
        CalculationColumnNames.calculation_id: args.calculation_id,
        CalculationColumnNames.calculation_type: args.calculation_type,
        CalculationColumnNames.period_start: args.calculation_period_start_datetime,
        CalculationColumnNames.period_end: args.calculation_period_end_datetime,
        CalculationColumnNames.execution_time_start: args.calculation_execution_time_start,
        CalculationColumnNames.created_by_user_id: args.created_by_user_id,
    }

    calculation_schema = """
calculation_id STRING NOT NULL,
calculation_type STRING NOT NULL,
period_start TIMESTAMP NOT NULL,
period_end TIMESTAMP NOT NULL,
execution_time_start TIMESTAMP NOT NULL,
created_by_user_id STRING NOT NULL
"""

    df = spark.createDataFrame(data=[Row(**calculation)], schema=calculation_schema)
    df.write.format("delta").mode("append").option("mergeSchema", "false").insertInto(
        f"{paths.BASIS_DATA_DATABASE_NAME}.{paths.CALCULATIONS_TABLE_NAME}"
    )
