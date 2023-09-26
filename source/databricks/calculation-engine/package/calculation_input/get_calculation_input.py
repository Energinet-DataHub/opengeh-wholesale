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

from typing import Tuple
from datetime import datetime
from pyspark.sql import DataFrame
from .metering_point_periods import get_metering_point_periods_df
from .grid_loss_responsible import get_grid_loss_responsible
from .calculation_input_reader import CalculationInputReader


def get_calculation_input(
    calculation_input_reader: CalculationInputReader,
    batch_period_start_datetime: datetime,
    batch_period_end_datetime: datetime,
    batch_grid_areas: list[str],
) -> Tuple[DataFrame, DataFrame, DataFrame]:
    metering_point_periods_df = get_metering_point_periods_df(
        calculation_input_reader,
        batch_period_start_datetime,
        batch_period_end_datetime,
        batch_grid_areas,
    )

    time_series_points_df = calculation_input_reader.read_time_series_points()

    grid_loss_responsible_df = get_grid_loss_responsible(batch_grid_areas)

    return metering_point_periods_df, time_series_points_df, grid_loss_responsible_df
