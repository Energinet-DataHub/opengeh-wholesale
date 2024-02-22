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
from package.calculation_input import TableReader
from package.calculation_input.schemas import (
    metering_point_period_schema,
    time_series_point_schema,
    grid_loss_metering_points_schema,
    charge_master_data_periods_schema,
    charge_link_periods_schema,
    charge_price_points_schema,
)


def get_correlations(table_reader: TableReader) -> dict[str, tuple]:
    return {
        "metering_point_periods.csv": (
            metering_point_period_schema,
            table_reader.read_metering_point_periods,
        ),
        "time_series_points.csv": (
            time_series_point_schema,
            table_reader.read_time_series_points,
        ),
        "grid_loss_metering_points.csv": (
            grid_loss_metering_points_schema,
            table_reader.read_grid_loss_metering_points,
        ),
        "charge_master_data_periods.csv": (
            charge_master_data_periods_schema,
            table_reader.read_charge_master_data_periods,
        ),
        "charge_link_periods.csv": (
            charge_link_periods_schema,
            table_reader.read_charge_links_periods,
        ),
        "charge_price_points.csv": (
            charge_price_points_schema,
            table_reader.read_charge_price_points,
        ),
        "expected_results.csv": (None, None),
    }
