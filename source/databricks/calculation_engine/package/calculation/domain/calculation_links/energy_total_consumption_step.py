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
from dependency_injector.wiring import Provide, Container

import package.databases.wholesale_results_internal.energy_storage_model_factory as factory
from package.calculation.calculation_output import CalculationOutput
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.wholesale.links.calculation_step import CalculationLink
from package.calculation.wholesale.links.repository_interfaces import MeteringPointPeriodRepository, IMeteringPointPeriodRepository
from package.codelists import TimeSeriesType, AggregationLevel

from package.calculation.energy.aggregators.grid_loss_aggregators import as grid_loss_aggr

class CalculateTotalEnergyConsumptionStep(CalculationLink):

    def __init__(
        self,
        calculator_args: CalculatorArgs,
        metering_point_periods_repository: IMeteringPointPeriodRepository=Provide[Container.service]
    ):
        super().__init__()
        if calculator_args is None:
            raise ValueError("calculator_args cannot be None")

        self.calculator_args = calculator_args

    def execute(self, output: CalculationOutput) -> CalculationOutput:
        total_consumption = factory.create(
            self.calculator_args,
            grid_loss_aggr.calculate_total_consumption(production, exchange),
            TimeSeriesType.TOTAL_CONSUMPTION,
            AggregationLevel.GRID_AREA,
        )

        output.energy_results_output.total_consumption = total_consumption
        return super().execute(output)
