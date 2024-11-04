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

from dependency_injector.wiring import Provide, inject

from package.calculation.calculation_output import CalculationOutput
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.domain.calculation_links.calculation_link import (
    CalculationLink,
)
from package.calculation.domain.chains.cache_bucket import CacheBucket
from package.calculation.preparation.transformations.clamp_period import clamp_period
from package.calculation.wholesale.links.metering_point_period_repository import (
    IMeteringPointPeriodsRepository,
)
from package.constants import Colname
from package.container import Container
from package.infrastructure import logging_configuration


class CacheMeteringPointPeriodsLink(CalculationLink):

    @inject
    def __init__(
        self,
        args: CalculatorArgs = Provide[Container.calculator_args],
        metering_point_periods_repository: IMeteringPointPeriodsRepository = Provide[
            Container.metering_point_periods_repository
        ],
        cache_bucket: CacheBucket = Provide[Container.cache_bucket],
    ) -> None:
        super().__init__()
        self.metering_point_periods_repository = metering_point_periods_repository
        self.cache_bucket = cache_bucket
        self.args = args

    @logging_configuration.use_span("cache_metering_point_periods")
    def execute(self, output: CalculationOutput) -> CalculationOutput:

        # Should return a typed value
        metering_point_periods = self.metering_point_periods_repository.get_by(
            self.args.calculation_period_start_datetime,
            self.args.calculation_period_end_datetime,
            self.args.calculation_grid_areas,
        )

        metering_point_periods = clamp_period(
            metering_point_periods,
            self.args.calculation_period_start_datetime,
            self.args.calculation_period_end_datetime,
            Colname.from_date,
            Colname.to_date,
        )

        self.cache_bucket.metering_point_periods = metering_point_periods

        return super().execute(output)
