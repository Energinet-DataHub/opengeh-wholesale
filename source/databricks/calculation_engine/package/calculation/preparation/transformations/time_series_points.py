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

from pyspark.sql import DataFrame
from pyspark.sql.functions import col

from package.calculation_input import TableReader
from package.constants import Colname
from package.common import assert_schema
from package.calculation_input.schemas import time_series_point_schema


def get_time_series_points(
    calculation_input_reader: TableReader,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:
    time_series_points_df = (
        calculation_input_reader.read_time_series_points()
        .where(col(Colname.observation_time) >= period_start_datetime)
        .where(col(Colname.observation_time) < period_end_datetime)
    )

    # Remove time series of grid loss metering points
    grid_loss_metering_points = (
        calculation_input_reader.read_grid_loss_metering_points()
    )
    time_series_points_df = time_series_points_df.join(
        grid_loss_metering_points,
        Colname.metering_point_id,
        "left_anti",
    )

    if "observation_year" in time_series_points_df.columns:
        time_series_points_df = time_series_points_df.drop(
            "observation_year"
        )  # Drop year partition column

    if "observation_month" in time_series_points_df.columns:
        time_series_points_df = time_series_points_df.drop(
            "observation_month"
        )  # Drop month partition column

    return time_series_points_df