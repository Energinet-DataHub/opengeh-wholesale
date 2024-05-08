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
import pyspark.sql.functions as f

from package.calculation import PreparedDataReader
from package.calculation.basis_data.schemas import calculations_schema
from package.codelists import CalculationType
from package.container import Container
from package.infrastructure import logging_configuration, paths
from package.calculation.calculator_args import CalculatorArgs
from package.constants.calculation_column_names import CalculationColumnNames


@logging_configuration.use_span("calculation.write-succeeded-calculation")
@inject
def write_calculation(
    args: CalculatorArgs,
    prepared_data_reader: PreparedDataReader,
) -> None:
    """Writes the succeeded calculation to the calculations table."""

    next_version = _get_next_version(args.calculation_type, spark)

    calculation = {
        CalculationColumnNames.calculation_id: args.calculation_id,
        CalculationColumnNames.calculation_type: args.calculation_type,
        CalculationColumnNames.period_start: args.calculation_period_start_datetime,
        CalculationColumnNames.period_end: args.calculation_period_end_datetime,
        CalculationColumnNames.execution_time_start: args.calculation_execution_time_start,
        CalculationColumnNames.created_by_user_id: args.created_by_user_id,
        CalculationColumnNames.version: next_version,
    }

    df = spark.createDataFrame(data=[Row(**calculation)], schema=calculations_schema)
    df.write.format("delta").mode("append").option("mergeSchema", "false").insertInto(
        f"{paths.BASIS_DATA_DATABASE_NAME}.{paths.CALCULATIONS_TABLE_NAME}"
    )


def _get_next_version(calculation_type: CalculationType, spark: SparkSession) -> int:
    """Returns the next available version for the selected calculation type."""

    calculations = spark.read.format("delta").table(
        f"{paths.BASIS_DATA_DATABASE_NAME}.{paths.CALCULATIONS_TABLE_NAME}"
    )

    latest_calculation = (
        calculations.where(
            f.col(CalculationColumnNames.calculation_type) == calculation_type
        )
        .agg(f.max(CalculationColumnNames.version))
        .collect()
    )

    current_version = (
        latest_calculation[0][CalculationColumnNames.version]
        if len(latest_calculation) > 0
        else 0
    )

    return current_version + 1
