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
from datetime import datetime
from typing import List

from pyspark.sql import DataFrame
from pyspark.sql.functions import col

from package.calculation.calculator_args import CalculatorArgs
from package.constants import Colname
from package.databases import read_table
from package.databases.migrations_wholesale.schemas import metering_point_periods_schema
from package.infrastructure.paths import MigrationsWholesaleDatabase


class MeteringPointPeriodRepositoryInterface(ABC):
    @abstractmethod
    def get_by(
        self, period_start: datetime, period_end: datetime, grid_area_codes: List[str]
    ) -> DataFrame:
        pass


class MeteringPointPeriodRepository(MeteringPointPeriodRepositoryInterface):

    def get_by(
        self, period_start: datetime, period_end: datetime, grid_area_codes: List[str]
    ) -> DataFrame:
        metering_point_periods_df = read_table(
            self._spark,
            self._catalog_name,
            self._calculation_input_database_name,
            MigrationsWholesaleDatabase.METERING_POINT_PERIODS_TABLE_NAME,
            metering_point_periods_schema,
        )

        metering_point_periods_df = (
            metering_point_periods_df.where(
                col(Colname.grid_area_code).isin(grid_area_codes)
                | col(Colname.from_grid_area_code).isin(grid_area_codes)
                | col(Colname.to_grid_area_code).isin(grid_area_codes)
            )
            .where(col(Colname.from_date) < period_end)
            .where(
                col(Colname.to_date).isNull() | (col(Colname.to_date) > period_start)
            )
        )

        return metering_point_periods_df


@dataclass
class CalculationMetaData:

    calculation_id: str
    calculation_type: str

    def __init__(self, args: CalculatorArgs):
        self.calculation_id = args.calculation_id
        self.calculation_type = args.calculation_type.value
