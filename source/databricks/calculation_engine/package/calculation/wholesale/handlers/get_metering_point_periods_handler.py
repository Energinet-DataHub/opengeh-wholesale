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
from typing import Optional

from pyspark.sql import DataFrame
from pyspark.sql.functions import col

from package.calculation.calculation_results import CalculationResultsContainer
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.preparation.data_structures import GridLossResponsible
from package.calculation.preparation.transformations.grid_loss_responsible import (
    _throw_if_no_grid_loss_responsible,
)
from package.calculation.wholesale.handlers.calculationstep import (
    BaseCalculationStep,
    ResponseType,
    Bucket,
)
from package.calculation.wholesale.handlers.interface_repository import (
    IMeteringPointPeriodRepository,
)

from package.calculation.wholesale.handlers.repository_interfaces import MeteringPointPeriodRepositoryInterface
from package.constants import Colname
from package.databases.migrations_wholesale import TableReader


class MeteringPointPeriodsWithGridLossDecorator(
    BaseCalculationStep[DataFrame, DataFrame]
):

    def handle(self, df: DataFrame) -> Optional[ResponseType]:
        pass


class CalculateGridLossResponsibleStep(
    BaseCalculationStep[DataFrame, GridLossResponsible]
):

    def __init__(
        self,
        output: CalculationResultsContainer,
        wholesale_internal_table_reader: TableReader,
    ):
        super().__init__(output)
        self.wholesale_internal_table_reader = wholesale_internal_table_reader
        self.output = output

    def handle(self, df: DataFrame) -> None:

        grid_loss_responsible = (
            self.wholesale_internal_table_reader.read_grid_loss_metering_points()
            .join(
                self.metering_point_periods_df,
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
                col(Colname.balance_responsible_id),
            )
        )

        _throw_if_no_grid_loss_responsible(self.grid_areas, grid_loss_responsible)

        self.output = GridLossResponsible(grid_loss_responsible)


class AddMeteringPointPeriodsToBucket(BaseCalculationStep):

    def __init__(self, bucket: Bucket, metering_point_period_repository: MeteringPointPeriodRepositoryInterface, )
        super().__init__(bucket)
        self.metering_point_period_repository = metering_point_period_repository

    def handle(self, output: CalculationResultsContainer) -> CalculationResultsContainer:
        metering_point_periods = (
            self.metering_point_period_repository.get_by(
                self.bucket.calculator_args.calculation_grid_areas
            )
        )

        metering_point_periods.where(
                col(Colname.grid_area_code).isin(calculator_args.calculation_grid_areas)
                | col(Colname.from_grid_area_code).isin(
                    calculator_args.calculation_grid_areas
                )
                | col(Colname.to_grid_area_code).isin(
                    calculator_args.calculation_grid_areas
                )
            )
            .where(
                col(Colname.from_date) < calculator_args.calculation_period_end_datetime
            )
            .where(
                col(Colname.to_date).isNull()
                | (
                    col(Colname.to_date)
                    > calculator_args.calculation_period_start_datetime
                )
            )
        )

        metering_point_periods = metering_point_periods.select(
            Colname.metering_point_id,
            Colname.metering_point_type,
            Colname.calculation_type,
            Colname.settlement_method,
            Colname.grid_area_code,
            Colname.resolution,
            Colname.from_grid_area_code,
            Colname.to_grid_area_code,
            Colname.parent_metering_point_id,
            Colname.energy_supplier_id,
            Colname.balance_responsible_id,
            Colname.from_date,
            Colname.to_date,
        )

        return super().handle(metering_point_periods)
