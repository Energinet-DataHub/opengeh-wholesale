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

from datetime import datetime
from pyspark.sql import DataFrame, SparkSession
from .metering_point_periods import get_metering_point_periods_df
from .grid_loss_responsible import get_grid_loss_responsible
from .delta_table_reader import DeltaTableReader
from .charges_reader import read_charges


class CalculationInput:
    def __init__(self, spark: SparkSession) -> None:
        self._calculation_input_reader = DeltaTableReader(spark)

    def get_metering_point_periods_df(
        self,
        period_start_datetime: datetime,
        period_end_datetime: datetime,
        grid_areas: list[str],
    ) -> DataFrame:
        return get_metering_point_periods_df(
            self._calculation_input_reader,
            period_start_datetime,
            period_end_datetime,
            grid_areas,
        )

    def get_time_series_points(self) -> DataFrame:
        return self._calculation_input_reader.read_time_series_points()

    def get_grid_loss_responsible(self, grid_areas: list[str]) -> DataFrame:
        return get_grid_loss_responsible(grid_areas)

    def get_charges(self) -> DataFrame:
        return read_charges(self._calculation_input_reader)
