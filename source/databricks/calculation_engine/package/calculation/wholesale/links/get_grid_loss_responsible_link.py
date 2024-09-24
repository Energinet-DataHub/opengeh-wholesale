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
from pyspark.sql import DataFrame
from pyspark.sql.functions import col

from package.calculation.calculation_output import CalculationOutput
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.domain.utils.chain.chain import CacheBucket
from package.calculation.wholesale.links.calculation_link import CalculationLink
from package.codelists import MeteringPointType
from package.constants import Colname
from package.databases.wholesale_internal import WholesaleInternalRepository
from package.infrastructure import logging_configuration


class GetGridLossResponsibleLink(CalculationLink):

    @inject
    def __init__(
        self,
        args: CalculatorArgs = Provide[Container.args],
        repository: WholesaleInternalRepository = Provide[
            Container.metering_point_period_repository
        ],
        metering_point_periods: DataFrame = Provide[
            Container.buckets.metering_point_periods
        ],
        cache_bucket: CacheBucket = Provide[Container.bucket],
    ):
        super().__init__()
        self.repository = repository
        self.args = args
        self.metering_point_periods = metering_point_periods
        self.cache_bucket = cache_bucket

    @logging_configuration.use_span("get_metering_point_periods")
    def execute(self, output: CalculationOutput) -> CalculationOutput:

        grid_loss_responsible = (
            self.repository.read_grid_loss_metering_points()
            .join(
                self.metering_point_periods,
                Colname.metering_point_id,
                "inner",
            )
            .select(
                col(Colname.metering_point_id),
                col(Colname.grid_area_code),
                col(Colname.from_date),
                col(Colname.to_date),
                col(Colname.metering_point_type),
                col(Colname.energy_supplier_id),
                col(Colname.balance_responsible_party_id),
            )
        )

        _throw_if_no_grid_loss_responsible(
            self.args.calculation_grid_areas, grid_loss_responsible
        )

        self.cache_bucket.grid_loss_responsible = grid_loss_responsible

        return super().execute(output)


def _throw_if_no_grid_loss_responsible(
    grid_areas: list[str], grid_loss_responsible_df: DataFrame
) -> None:
    for grid_area in grid_areas:
        current_grid_loss_metering_points = grid_loss_responsible_df.filter(
            col(Colname.grid_area_code) == grid_area
        )
        if (
            current_grid_loss_metering_points.filter(
                col(Colname.metering_point_type) == MeteringPointType.PRODUCTION.value
            ).count()
            == 0
        ):
            raise ValueError(
                f"No responsible for negative grid loss found for grid area {grid_area}"
            )
        if (
            current_grid_loss_metering_points.filter(
                col(Colname.metering_point_type) == MeteringPointType.CONSUMPTION.value
            ).count()
            == 0
        ):
            raise ValueError(
                f"No responsible for positive grid loss found for grid area {grid_area}"
            )
