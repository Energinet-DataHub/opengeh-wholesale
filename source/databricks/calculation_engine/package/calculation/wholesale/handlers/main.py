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

from package.calculation.wholesale.handlers.decorator import BaseDecorator
from pyspark.sql import DataFrame

from package.calculation.calculation_results import CalculationResultsContainer
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.wholesale.handlers.calculation_parameters_step import (
    CalculationParametersStep,
)
from package.calculation.wholesale.handlers.calculationstep import Bucket
from package.calculation.wholesale.handlers.get_metering_point_periods_handler import (
    AddMeteringPointPeriodsToBucket,
    CalculateGridLossResponsibleStep,
    MeteringPointPeriodsWithGridLossDecorator,
)


def chain(calculator_args: CalculatorArgs, mpp_repository: DataFrame) -> None:

    output = CalculationResultsContainer()
    bucket = Bucket()

    calculation_parameters_step = CalculationParametersStep(
        calculator_args,
        output,
    )

    # Chain steps
    # Pass output to next step automatically in handle method
    # Inject needed dependencies in the constructors

    add_metering_point_periods_to_bucket = AddMeteringPointPeriodsToBucket(
        calculator_args, output, mpp_repository
    )

    calculate_grid_loss_responsible_step = CalculateGridLossResponsibleStep(
        bucket, output, mpp_repository
    )
    mpp_with_grid_loss_decorator = MeteringPointPeriodsWithGridLossDecorator()
    fallback_decorator = BaseDecorator()

    # Set up the chain
    (
        calculation_parameters_step.set_next(add_metering_point_periods_to_bucket)
        .set_next(calculate_grid_loss_responsible_step)
        .set_next(mpp_with_grid_loss_decorator)
        .set_next(fallback_decorator)
    )

    # Execute calculation
    calculation_parameters_step.handle(bucket)
