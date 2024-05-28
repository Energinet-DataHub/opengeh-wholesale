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
from pyspark.sql import SparkSession, DataFrame, Row

from package.calculation import PreparedDataReader
from package.calculation.basis_data.schemas import executing_calculation_schema
from package.calculation.calculator_args import CalculatorArgs
from package.constants.calculation_column_names import CalculationColumnNames


def create_executing_calculation(
    args: CalculatorArgs,
    prepared_data_reader: PreparedDataReader,
    spark: SparkSession,
) -> DataFrame:
    """
    Creates a data frame with a row representing the currently executing calculation.
    The version is the next available version for the given calculation type.
    """

    latest_version = prepared_data_reader.get_latest_calculation_version(
        args.calculation_type
    )

    # Next version begins with 1 and increments by 1
    next_version = (latest_version or 0) + 1

    calculation = {
        CalculationColumnNames.calculation_id: args.calculation_id,
        CalculationColumnNames.calculation_type: args.calculation_type.value,
        CalculationColumnNames.period_start: args.calculation_period_start_datetime,
        CalculationColumnNames.period_end: args.calculation_period_end_datetime,
        CalculationColumnNames.execution_time_start: args.calculation_execution_time_start,
        CalculationColumnNames.created_by_user_id: args.created_by_user_id,
        CalculationColumnNames.version: next_version,
    }

    return spark.createDataFrame(
        data=[Row(**calculation)], schema=executing_calculation_schema
    )
