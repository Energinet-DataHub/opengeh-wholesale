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
from dataclasses import dataclass

from package.calculation.calculation_output import CalculationOutput
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.domain.calculation_steps.energy_total_consumption_step import (
    TotalEnergyConsumptionStep,
)
from package.calculation.wholesale.handlers.calculationstep import BaseCalculationStep


class Chain:

    def __init__(self, calculator_args: CalculatorArgs):
        ,
        metering_point_period_repository: MeteringPointPeriodRepositoryInterface,
        prepared_data_reader: PreparedDataReader,

        bucket = CacheBucket()

        total_energy_consumption_step = TotalEnergyConsumptionStep(calculator_args)
        fallback_step = BaseCalculationStep()

        # Set up the chain
        (
            total_energy_consumption_step(
            ).set_next(fallback_step)
        )

        # Execute calculation chain
        total_energy_cunsumption_step.handle(CalculationOutput())
