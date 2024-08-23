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
from package.calculation.wholesale.handlers.calculation_parameters_step import (
    CreateCalculationMetaDataOutputStep,
)
from package.calculation.wholesale.handlers.calculationstep import (
    Bucket,
    BaseCalculationStep,
)
from package.calculation.wholesale.handlers.get_metering_point_periods_handler import (
    AddMeteringPointPeriodsToBucketStep,
    CalculateGridLossResponsibleStep,
)
from package.calculation.wholesale.handlers.repository_interfaces import (
    MeteringPointPeriodRepositoryInterface,
)


# Chain steps
# Pass output to next step automatically in handle method
# Inject needed dependencies in the constructors
def chain(
    calculator_args: CalculatorArgs,
    metering_point_period_repository: MeteringPointPeriodRepositoryInterface,
    prepared_data_reader: PreparedDataReader,
) -> None:

    calculation_meta_data_step = CreateCalculationMetaDataOutputStep(
        calculator_args, prepared_data_reader
    )

    add_metering_point_periods_to_bucket_step = AddMeteringPointPeriodsToBucketStep(
        metering_point_period_repository
    )

    calculate_grid_loss_responsible_step = CalculateGridLossResponsibleStep(
        metering_point_period_repository
    )
    fallback_step = BaseCalculationStep()

    # Set up the chain
    (
        calculation_meta_data_step.set_next(add_metering_point_periods_to_bucket_step)
        .set_next(calculate_grid_loss_responsible_step)
        .set_next(fallback_step)
    )

    # Execute calculation chain
    calculation_meta_data_step.handle(Bucket(), CalculationOutput())
