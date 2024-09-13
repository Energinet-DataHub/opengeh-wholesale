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

from package.calculation import PreparedDataReader
from package.calculation.calculation_output import CalculationOutput
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.domain.calculation_steps.calculate_not_profiled_consumption_step import (
    CalculateNonProfiledConsumptionStep,
)
from package.calculation.domain.calculation_steps.create_calculation_meta_data_step import (
    CreateCalculationMetaDataStep,
    SaveCalculationMetaDataStep,
)
from package.calculation.domain.calculation_steps.energy_total_consumption_step import (
    CalculatieTotalEnergyConsumptionStep,
)
from package.calculation.domain.calculation_steps.start_step import StartCalculationStep
from package.calculation.wholesale.handlers.calculation_step import (
    BaseCalculationStep,
    CacheBucket,
)
from package.calculation.wholesale.handlers.repository_interfaces import (
    MeteringPointPeriodRepositoryInterface,
)


class Chain:

    def __init__(self, calculator_args: CalculatorArgs):

        metering_point_period_repository: MeteringPointPeriodRepositoryInterface
        prepared_data_reader: PreparedDataReader
        bucket = CacheBucket()

        start_calculation_step = StartCalculationStep()
        create_calculation_meta_data_step = CreateCalculationMetaDataStep()
        save_calculation_meta_data_step = SaveCalculationMetaDataStep()
        calculate_total_energy_consumption_step = CalculatieTotalEnergyConsumptionStep(
            calculator_args
        )
        calculate_non_profiled_consumptions_step = CalculateNonProfiledConsumptionStep(
            calculator_args
        )
        fallback_step = BaseCalculationStep()

        # Set up the chain
        (
            start_calculation_step.set_next(create_calculation_meta_data_step)
            .set_next(save_calculation_meta_data_step)
            .set_next(calculate_total_energy_consumption_step)
            .set_next(calculate_non_profiled_consumptions_step)
            .set_next(fallback_step)
        )

        # Execute calculation chain
        start_calculation_step.handle(CalculationOutput())
