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
from typing import Optional

from pyspark import Row
from pyspark.sql import DataFrame

from package.calculation import PreparedDataReader
from package.calculation.calculation_results import CalculationResultsContainer
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.wholesale.handlers.calculationstep import (
    BaseCalculationStep,
    ResponseType,
)
from package.databases.wholesale_internal.calculation_column_names import (
    CalculationColumnNames,
)
from package.databases.wholesale_internal.schemas import hive_calculations_schema


class CalculationParametersStep(BaseCalculationStep[DataFrame, None]):

    def __init__(
        self,
        calculator_args: CalculatorArgs,
        output: CalculationResultsContainer,
        prepared_data_reader: PreparedDataReader,
    ):
        super().__init__(output)
        self.calculator_args = calculator_args
        self.prepared_data_reader = prepared_data_reader

    def handle(self) -> Optional[ResponseType]:

        latest_version = self.prepared_data_reader.get_latest_calculation_version(
            self.calculator_args.calculation_type
        )

        # Next version begins with 1 and increments by 1
        next_version = (latest_version or 0) + 1

        calculation = {
            CalculationColumnNames.calculation_id: self.calculator_args.calculation_id,
            CalculationColumnNames.calculation_type: self.calculator_args.calculation_type.value,
            CalculationColumnNames.period_start: self.calculator_args.calculation_period_start_datetime,
            CalculationColumnNames.period_end: self.calculator_args.calculation_period_end_datetime,
            CalculationColumnNames.execution_time_start: self.calculator_args.calculation_execution_time_start,
            CalculationColumnNames.created_by_user_id: self.calculator_args.created_by_user_id,
            CalculationColumnNames.version: next_version,
        }

        self.output.calculation = spark.createDataFrame(
            data=[Row(**calculation)], schema=hive_calculations_schema
        )

        if self._next_handler:
            return self._next_handler.handle()
        return None
