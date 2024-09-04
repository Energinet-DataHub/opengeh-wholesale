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
from package.calculation.calculation_output import CalculationOutput
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.wholesale.handlers.calculationstep import BaseCalculationStep
from package.codelists import TimeSeriesType, AggregationLevel

import package.databases.wholesale_results_internal.energy_storage_model_factory as factory

class TotalEnergyConsumptionStep(BaseCalculationStep):

    def __init__(
        self,
        calculator_args: CalculatorArgs,
    ):
        super().__init__()
        self.calculator_args = calculator_args

    def handle(self, output: CalculationOutput) -> CalculationOutput:
        total_consumption = factory.create(
            self.calculator_args,
            grid_loss_aggr.calculate_total_consumption(production, exchange),
            TimeSeriesType.TOTAL_CONSUMPTION,
            AggregationLevel.GRID_AREA,
        )

        output.energy_results_output.total_consumption = total_consumption
        return output


    production: EnergyResults,
    exchange: EnergyResults,
