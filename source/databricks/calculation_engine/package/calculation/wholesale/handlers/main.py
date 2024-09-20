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
from package.calculation.wholesale.handlers.calculation_step import (
    CacheBucket,
    CalculationLink,
)
from package.calculation.wholesale.handlers.get_metering_point_periods_handler import (
    AddMeteringPointPeriodsToBucketStep,
    CalculateGridLossResponsibleStep,
)

from package.calculation import PreparedDataReader
from package.calculation.calculation_output import CalculationOutput
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.wholesale.handlers.calculate_temporary_flex_consumption_per_es_step import (
    CalculateTemporaryFlexConsumptionPerEsStep,
)
from package.calculation.wholesale.handlers.calculation_parameters_step import (
    CreateCalculationMetaDataOutputStep,
)
from package.calculation.wholesale.handlers.calculation_start_step import (
    CalculationStartStep,
)
from package.calculation.wholesale.handlers.repository_interfaces import (
    IMeteringPointPeriodRepository,
)


# Chain steps
# Pass output to next step automatically in handle method
# Inject needed dependencies in the constructors
def chain(
    calculator_args: CalculatorArgs,
    metering_point_period_repository: IMeteringPointPeriodRepository,
    prepared_data_reader: PreparedDataReader,
) -> None:
    bucket = CacheBucket()

    calculation_start_step = CalculationStartStep()

    calculation_meta_data_step = CreateCalculationMetaDataOutputStep(
        calculator_args, prepared_data_reader
    )

    calculate_exchange_step = AddMeteringPointPeriodsToBucketStep(
        metering_point_period_repository
    )

    calculate_temporary_flex_consumption_per_es_step = (
        CalculateTemporaryFlexConsumptionPerEsStep()
    )
    calculate_grid_loss_responsible_step = CalculateGridLossResponsibleStep(
        metering_point_period_repository
    )
    last_step = CalculationLink()

    # Set up the chain
    (
        calculation_start_step.set_next(calculation_meta_data_step)
        .set_next(calculate_exchange_step)
        .set_next(calculate_grid_loss_responsible_step)
        .set_next(calculate_temporary_flex_consumption_per_es_step)
        .set_next(last_step)
    )

    # Execute chain
    calculation_start_step.execute(CalculationOutput())
