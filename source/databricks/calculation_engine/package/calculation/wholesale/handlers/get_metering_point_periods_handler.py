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

from package.calculation.wholesale.handlers.handler import BaseDecorator, ResponseType
from pyspark.sql import DataFrame
from pyspark.sql.functions import col

from package.calculation.calculation_results import CalculationResultsContainer
from package.calculation.calculator_args import CalculatorArgs
from package.constants import Colname
from package.databases.migrations_wholesale import TableReader


class MeteringPointPeriodsWithGridLossDecorator(BaseDecorator[DataFrame, DataFrame]):

    def handle(self, df: DataFrame) -> Optional[ResponseType]:
        pass


class MeteringPointPeriodsWithoutGridLossDecorator(BaseDecorator[DataFrame, DataFrame]):

    def handle(self, df: DataFrame) -> Optional[ResponseType]:
        pass


class CalculationDecorator(BaseDecorator[DataFrame, None]):

    def __init__(
        self,
        calculator_args: CalculatorArgs,
    ):
        self.calculator_args = calculator_args


class MeteringPointPeriodsDecorator(BaseDecorator[CalculatorArgs, DataFrame]):

    def __init__(
        self,
        container: CalculationResultsContainer,
        calculation_input_reader: TableReader,
    ):
        self.calculation_input_reader = calculation_input_reader

    def handle(
        self,
        calculator_args: CalculatorArgs,
    ) -> DataFrame:
        metering_point_periods = (
            self.calculation_input_reader.read_metering_point_periods()
            .where(
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
