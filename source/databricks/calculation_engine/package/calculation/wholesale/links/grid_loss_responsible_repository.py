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
from abc import ABC, abstractmethod

from pyspark.sql.functions import col

from package.calculation.preparation.data_structures import GridLossResponsible
from package.calculation.wholesale.links.metering_point_period_repository import (
    IMeteringPointPeriodsRepository,
)
from package.constants import Colname
from package.databases.wholesale_internal import WholesaleInternalRepository


class IGridLossResponsibleRepository(ABC):
    @abstractmethod
    def get_by(self, grid_areas: list[str]) -> GridLossResponsible:
        pass


class GridLossResponsibleRepository(IGridLossResponsibleRepository):

    def __init__(
        self,
        metering_point_periods_repository: IMeteringPointPeriodsRepository,
        repository: WholesaleInternalRepository,
    ):
        self.repository = repository
        self.metering_point_periods_repository = metering_point_periods_repository

    def get_by(self, grid_areas: list[str]) -> GridLossResponsible:

        metering_point_periods = self.metering_point_periods_repository.get_by(
            grid_areas
        )

        grid_loss_responsible = (
            self.repository.read_grid_loss_metering_points()
            .join(
                metering_point_periods,
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

        _throw_if_no_grid_loss_responsible(grid_areas, grid_loss_responsible)

        return GridLossResponsible(grid_loss_responsible)
