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
from dependency_injector.wiring import Provide, Container, inject

from package.calculation.calculation_output import CalculationOutput
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.domain.calculation_links.calculation_link import (
    CalculationLink,
)
from package.calculation.domain.chains.cache_bucket import CacheBucket
from package.calculation.wholesale.links.metering_point_period_repository import (
    IMeteringPointPeriodRepository,
)
from package.infrastructure import logging_configuration


class GetMeteringPointPeriodsLink(CalculationLink):

    @inject
    def __init__(
        self,
        args: CalculatorArgs = Provide[Container.args],
        metering_point_period_repository: IMeteringPointPeriodRepository = Provide[
            Container.metering_point_period_repository
        ],
        cache_bucket: CacheBucket = Provide[Container.bucket],
    ):
        super().__init__()
        self.metering_point_period_repository = metering_point_period_repository
        self.cache_bucket = cache_bucket
        self.args = args

    @logging_configuration.use_span("get_metering_point_periods")
    def execute(self, output: CalculationOutput) -> CalculationOutput:

        metering_point_periods = self.metering_point_period_repository.get_by(
            self.args.calculation_period_start_datetime,
            self.args.calculation_period_end_datetime,
            self.args.calculation_grid_areas,
        )

        self.cache_bucket.metering_point_periods = metering_point_periods

        return super().execute(output)
