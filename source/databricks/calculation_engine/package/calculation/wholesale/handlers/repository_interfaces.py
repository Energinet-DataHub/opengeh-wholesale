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
from dataclasses import dataclass
from typing import List

from pyspark.sql import DataFrame

from package.calculation.calculator_args import CalculatorArgs

import package.databases.repository_helper as repository_helper
from package.databases import read_table


class MeteringPointPeriodRepositoryInterface(ABC):
    @abstractmethod
    def get_by(self, grid_area_codes: List[str]) -> DataFrame:
        pass


class MeteringPointPeriodRepository(MeteringPointPeriodRepositoryInterface):

    def __init__(self):
        self.reader = read_table()


    def get_by(self, grid_area_codes: List[str]) -> DataFrame:
        read_table(). metering_point_periods.where(
            col(Colname.grid_area_code).isin(bucket.calculator_args.calculation_grid_areas)
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


@dataclass
class CalculationMetaData:

    calculation_id: str
    calculation_type : str

    def __init__(self, args: CalculatorArgs):
        self.calculation_id = args.calculation_id
        self.calculation_type = args.calculation_type.value
