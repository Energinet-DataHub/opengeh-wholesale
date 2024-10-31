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
from pyspark.sql.types import StructType

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
    grid_loss_metering_point_ids_schema,
    calculations_schema,
)
from package.databases.wholesale_results import WholesaleResultsInternalRepository
from package.databases.wholesale_results_internal.schemas import (
    energy_schema,
    energy_per_es_schema,
    energy_per_brp_schema,
    amounts_per_charge_schema,
)
from package.infrastructure.paths import WholesaleResultsInternalDatabase


def get_data_input_specifications(
    migrations_wholesale_repository: MigrationsWholesaleRepository,
    wholesale_internal_repository: wholesale_internal.WholesaleInternalRepository,
    wholesale_results_internal_repository: WholesaleResultsInternalRepository,
) -> dict[str, tuple[StructType, callable]]:
    """
    Contains the mapping between the csv file name, the schema name and the function
    to be mocked.
    """
    return {
        "calculations.csv": (
            calculations_schema,
            wholesale_internal_repository.read_calculations,
        ),
        "metering_point_periods.csv": (
            metering_point_periods_schema,
            migrations_wholesale_repository.read_metering_point_periods,
        ),
        "time_series_points.csv": (
            time_series_points_schema,
            migrations_wholesale_repository.read_time_series_points,
        ),
        "grid_loss_metering_points.csv": (
            grid_loss_metering_point_ids_schema,
            wholesale_internal_repository.read_grid_loss_metering_point_ids,
        ),
        "charge_price_information_periods.csv": (
            charge_price_information_periods_schema,
            migrations_wholesale_repository.read_charge_price_information_periods,
        ),
        "charge_link_periods.csv": (
            charge_link_periods_schema,
            migrations_wholesale_repository.read_charge_link_periods,
        ),
        "charge_price_points.csv": (
            charge_price_points_schema,
            migrations_wholesale_repository.read_charge_price_points,
        ),
        f"{WholesaleResultsInternalDatabase.DATABASE_NAME}/{WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME}.csv": (
            energy_schema,
            wholesale_results_internal_repository.read_energy,
        ),
        f"{WholesaleResultsInternalDatabase.DATABASE_NAME}/{WholesaleResultsInternalDatabase.ENERGY_PER_ES_TABLE_NAME}.csv": (
            energy_per_es_schema,
            wholesale_results_internal_repository.read_energy_per_es,
        ),
        f"{WholesaleResultsInternalDatabase.DATABASE_NAME}/{WholesaleResultsInternalDatabase.ENERGY_PER_BRP_TABLE_NAME}.csv": (
            energy_per_brp_schema,
            wholesale_results_internal_repository.read_energy_per_brp,
        ),
        f"{WholesaleResultsInternalDatabase.DATABASE_NAME}/{WholesaleResultsInternalDatabase.AMOUNTS_PER_CHARGE_TABLE_NAME}.csv": (
            amounts_per_charge_schema,
            wholesale_results_internal_repository.read_amount_per_charge,
        ),
    }
