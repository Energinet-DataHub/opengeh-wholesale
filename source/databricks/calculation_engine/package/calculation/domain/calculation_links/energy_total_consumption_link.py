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
from dependency_injector.wiring import Provide

from package.calculation.calculation_output import CalculationOutput
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.domain.calculation_links.calculation_link import (
    CalculationLink,
)
from package.calculation.wholesale.links.metering_point_period_repository import (
    IMeteringPointPeriodsRepository,
)
from package.container import Container


class CalculateTotalEnergyConsumptionLink(CalculationLink):

    def __init__(
        self,
        calculator_args: CalculatorArgs = Provide[Container.calculator_args],
        metering_point_periods_repository: IMeteringPointPeriodsRepository = Provide[
            Container.metering_point_periods_repository
        ],
    ):
        super().__init__()

        if calculator_args is None:
            raise ValueError("calculator_args cannot be None.")

        if metering_point_periods_repository is None:
            raise ValueError("metering_point_periods_repository cannot be None.")

        self.calculator_args = calculator_args
        self.metering_point_periods_repository = metering_point_periods_repository

    def execute(self, output: CalculationOutput) -> CalculationOutput:

        self.metering_point_periods_repository.get_by()

        return super().execute(output)
