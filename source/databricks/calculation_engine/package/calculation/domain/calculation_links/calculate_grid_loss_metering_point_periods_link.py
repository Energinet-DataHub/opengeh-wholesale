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
from pyspark.sql import DataFrame
from pyspark.sql.functions import col

from package.calculation.calculation_output import CalculationOutput
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.domain.calculation_links.calculation_link import (
    CalculationLink,
)
from package.calculation.domain.chains.cache_bucket import CacheBucket
from package.codelists import MeteringPointType
from package.constants import Colname
from package.container import Container
from package.databases.wholesale_internal import WholesaleInternalRepository


class CalculateGridLossMeteringPointPeriodsLink(CalculationLink):

    def __init__(
        self,
        calculator_args: CalculatorArgs = Provide[Container.calculator_args],
        wholesale_internal_repository: WholesaleInternalRepository = Provide[
            Container.wholesale_internal_repository
        ],
        cache_bucket: CacheBucket = Provide[Container.cache_bucket],
    ):
        super().__init__()
        self.calculator_args = calculator_args
        self.wholesale_internal_repository = wholesale_internal_repository
        self.cache_bucket = cache_bucket

    def execute(self, calculation_output: CalculationOutput) -> CalculationOutput:

        grid_loss_metering_points = (
            self.wholesale_internal_repository.read_grid_loss_metering_point_ids()
        )

        grid_loss_responsible = grid_loss_metering_points.join(
            self.cache_bucket.metering_point_periods,
            Colname.metering_point_id,
            "inner",
        ).select(
            col(Colname.metering_point_id),
            col(Colname.grid_area_code),
            col(Colname.from_date),
            col(Colname.to_date),
            col(Colname.metering_point_type),
            col(Colname.energy_supplier_id),
            col(Colname.balance_responsible_party_id),
        )

        self._throw_if_no_grid_loss_responsible(
            self.calculator_args.calculation_grid_areas, grid_loss_responsible
        )

        return super().execute(calculation_output)

    @staticmethod
    def _throw_if_no_grid_loss_responsible(
        grid_areas: list[str], grid_loss_responsible_df: DataFrame
    ) -> None:
        for grid_area in grid_areas:
            current_grid_loss_metering_points = grid_loss_responsible_df.filter(
                col(Colname.grid_area_code) == grid_area
            )
            if (
                current_grid_loss_metering_points.filter(
                    col(Colname.metering_point_type)
                    == MeteringPointType.PRODUCTION.value
                ).count()
                == 0
            ):
                raise ValueError(
                    f"No responsible for negative grid loss found for grid area {grid_area}"
                )
            if (
                current_grid_loss_metering_points.filter(
                    col(Colname.metering_point_type)
                    == MeteringPointType.CONSUMPTION.value
                ).count()
                == 0
            ):
                raise ValueError(
                    f"No responsible for positive grid loss found for grid area {grid_area}"
                )
