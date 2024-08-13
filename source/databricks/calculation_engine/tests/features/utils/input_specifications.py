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
from package.databases import wholesale_internal
from package.databases.migrations_wholesale import MigrationsWholesaleRepository
from package.databases.migrations_wholesale.schemas import (
    metering_point_periods_schema,
    time_series_points_schema,
    charge_price_information_periods_schema,
    charge_link_periods_schema,
    charge_price_points_schema,
)
from package.databases.wholesale_internal.schemas import (
    grid_loss_metering_points_schema,
    calculations_schema,
)


def get_data_input_specifications(
    table_reader: MigrationsWholesaleRepository,
    wholesale_internal_table_reader: wholesale_internal.WholesaleInternalRepository,
) -> dict[str, tuple]:
    """
    Contains the mapping between the csv file name, the schema name and the function
    to be mocked.
    """
    return {
        "calculations.csv": (
            calculations_schema,
            wholesale_internal_table_reader.read_calculations,
        ),
        "metering_point_periods.csv": (
            metering_point_periods_schema,
            table_reader.read_metering_point_periods,
        ),
        "time_series_points.csv": (
            time_series_points_schema,
            table_reader.read_time_series_points,
        ),
        "grid_loss_metering_points.csv": (
            grid_loss_metering_points_schema,
            wholesale_internal_table_reader.read_grid_loss_metering_points,
        ),
        "charge_price_information_periods.csv": (
            charge_price_information_periods_schema,
            table_reader.read_charge_price_information_periods,
        ),
        "charge_link_periods.csv": (
            charge_link_periods_schema,
            table_reader.read_charge_link_periods,
        ),
        "charge_price_points.csv": (
            charge_price_points_schema,
            table_reader.read_charge_price_points,
        ),
    }
