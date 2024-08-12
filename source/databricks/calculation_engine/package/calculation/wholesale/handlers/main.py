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
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.wholesale.handlers.get_metering_point_periods_handler import (
    MeteringPointPeriodsHandler,
    MeteringPointPeriodsWithoutGridLoss,
    MeteringPointPeriodsWithGridLoss,
)
from package.calculation.wholesale.handlers.handler import BaseHandler
from package.databases.migrations_wholesale import TableReader


def chain():

    table_reader = TableReader()
    calculator_args = CalculatorArgs()

    mpp_handler = MeteringPointPeriodsHandler(table_reader)
    mpp_without_grid_loss_handler = MeteringPointPeriodsWithoutGridLoss()
    mpp_with_grid_loss_handler = MeteringPointPeriodsWithGridLoss()
    fallback_handler = BaseHandler()

    # Set up the chain: MeteringPointPeriodsHandler -> MeteringPointPeriodsWithoutGridLoss -> FallbackHandler
    mpp_handler.set_next(mpp_without_grid_loss_handler)
    mpp_without_grid_loss_handler.set_next(mpp_with_grid_loss_handler)
    mpp_with_grid_loss_handler.set_next(fallback_handler)

    mpp_handler.handle(calculator_args)
