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
from pyspark.sql import DataFrame

import package.databases.wholesale_results_internal.energy_storage_model_factory as factory
from package.calculation.calculation_output import CalculationOutput
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.wholesale.handlers.calculationstep import BaseCalculationStep
from package.codelists import TimeSeriesType, AggregationLevel


class CalculateNonProfiledConsumptionStep(BaseCalculationStep):

    def __init__(
        self, args: CalculatorArgs, non_profiled_consumption_per_es: DataFrame
    ):
        super().__init__()
        self.args = args
        self.non_profiled_consumption_per_es = non_profiled_consumption_per_es

    def handle(self, output: CalculationOutput) -> CalculationOutput:
        non_profiled_consumption_per_es = factory.create(
            self.args,
            self.non_profiled_consumption_per_es,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.ENERGY_SUPPLIER,
        )
        output.energy_results_output.non_profiled_consumption_per_es = (
            non_profiled_consumption_per_es
        )
        return super().handle(output)
