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
from dependency_injector.wiring import Provide
from pyspark.sql import SparkSession, DataFrame, Row

from package.calculation import PreparedDataReader
from package.calculation.calculator_args import CalculatorArgs
from package.databases.table_column_names import TableColumnNames

from package.container import Container
from package.databases.wholesale_internal.schemas.calculations_schema import (
    calculations_schema,
)


def create_calculation(
    args: CalculatorArgs,
    next_version: int,
) -> DataFrame:
    """
    Creates a data frame with a row representing the currently executing calculation.
    The version is the next available version for the given calculation type.
    """
    return _create_calculation(args, next_version)


def _create_calculation(
    args: CalculatorArgs,
    next_version: int,
    spark: SparkSession = Provide[Container.spark],
) -> DataFrame:
    calculation = {
        TableColumnNames.calculation_id: args.calculation_id,
        TableColumnNames.calculation_type: args.calculation_type.value,
        TableColumnNames.calculation_period_start: args.calculation_period_start_datetime,
        TableColumnNames.calculation_period_end: args.calculation_period_end_datetime,
        TableColumnNames.calculation_execution_time_start: args.calculation_execution_time_start,
        TableColumnNames.created_by_user_id: args.created_by_user_id,
        TableColumnNames.calculation_version: next_version,
        TableColumnNames.is_internal_calculation: args.is_internal_calculation,
        TableColumnNames.calculation_succeeded_time: None,
    }

    return spark.createDataFrame(data=[Row(**calculation)], schema=calculations_schema)
