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

from package.calculation.calculation_results import CalculationResultsContainer
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.wholesale.handlers.decorator import BaseDecorator
from package.calculation.wholesale.handlers.get_metering_point_periods_handler import (
    GetMeteringPointPeriodsDecorator,
    GetGridLossResponsibleDecorator,
    MeteringPointPeriodsWithGridLossDecorator,
    CalculationDecorator,
)
from package.databases.migrations_wholesale import TableReader


def chain():

    # Dependency injection
    mpp_repository = TableReader()
    calculator_args = CalculatorArgs()
    container = CalculationResultsContainer()

    # DI for decorators
    calculation_decorator = CalculationDecorator(calculator_args)
    get_mpp_decorator = GetMeteringPointPeriodsDecorator(
        calculator_args, container, mpp_repository
    )
    grid_loss_responsible_decorator = GetGridLossResponsibleDecorator(
        calculator_args.calculation_grid_areas,
        container.metering_point_periods_df,
        mpp_repository,
    )
    mpp_with_grid_loss_decorator = MeteringPointPeriodsWithGridLossDecorator()
    fallback_decorator = BaseDecorator()

    # Set up the chain
    calculation_decorator.set_next(get_mpp_decorator)
    get_mpp_decorator.set_next(grid_loss_responsible_decorator)
    get_mpp_decorator.handle()
    grid_loss_responsible_decorator.set_next(mpp_with_grid_loss_decorator)
    mpp_with_grid_loss_decorator.set_next(fallback_decorator)

    # Execute calculation
    calculation_decorator.handle()
