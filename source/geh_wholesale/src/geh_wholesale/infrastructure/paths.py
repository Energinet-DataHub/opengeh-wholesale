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

# Resource names and variables defined in the infrastructure repository (https://github.com/Energinet-DataHub/dh3-infrastructure)


from typing import ClassVar

from pydantic import Field
from pydantic_settings import BaseSettings

import geh_wholesale.infrastructure.environment_variables as env_vars

# WHOLESALE_RESULTS = "wholesale_results"
# WHOLESALE_BASIS_DATA_INTERNAL = "wholesale_basis_data_internal"
# WHOLESALE_BASIS_DATA = "wholesale_basis_data"
# WHOLESALE_RESULTS_INTERNAL = "wholesale_results_internal"
# WHOLESALE_INTERNAL = "wholesale_internal"
# WHOLESALE_SAP = "wholesale_sap"
# WHOLESALE_MIGRATION = "shared_wholesale_input"


class MigrationsWholesaleDatabase(BaseSettings):
    DATABASE_WHOLESALE_MIGRATION: str = Field(init=False)
    METERING_POINT_PERIODS_TABLE_NAME: ClassVar[str] = "metering_point_periods_view_v1"
    TIME_SERIES_POINTS_TABLE_NAME: ClassVar[str] = "time_series_points_view_v1"
    CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME: ClassVar[str] = "charge_price_information_periods_view_v1"
    CHARGE_LINK_PERIODS_TABLE_NAME: ClassVar[str] = "charge_link_periods_view_v1"
    CHARGE_PRICE_POINTS_TABLE_NAME: ClassVar[str] = "charge_price_points_view_v1"

    TABLE_NAMES: ClassVar[list[str]] = [
        METERING_POINT_PERIODS_TABLE_NAME,
        TIME_SERIES_POINTS_TABLE_NAME,
        CHARGE_LINK_PERIODS_TABLE_NAME,
        CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME,
        CHARGE_PRICE_POINTS_TABLE_NAME,
    ]


class WholesaleInternalDatabase(BaseSettings):
    DATABASE_WHOLESALE_INTERNAL: str = Field(init=False)
    EXECUTED_MIGRATIONS_TABLE_NAME: ClassVar[str] = "executed_migrations"
    CALCULATIONS_TABLE_NAME: ClassVar[str] = "calculations"
    GRID_LOSS_METERING_POINT_IDS_TABLE_NAME: ClassVar[str] = "grid_loss_metering_points"
    CALCULATION_GRID_AREAS_TABLE_NAME: ClassVar[str] = "calculation_grid_areas"

    TABLE_NAMES: ClassVar[list[str]] = [
        EXECUTED_MIGRATIONS_TABLE_NAME,
        CALCULATIONS_TABLE_NAME,
        GRID_LOSS_METERING_POINT_IDS_TABLE_NAME,
        CALCULATION_GRID_AREAS_TABLE_NAME,
    ]

    SUCCEEDED_EXTERNAL_CALCULATIONS_V1_VIEW_NAME: ClassVar[str] = "succeeded_external_calculations_v1"

    VIEW_NAMES: ClassVar[list[str]] = [
        SUCCEEDED_EXTERNAL_CALCULATIONS_V1_VIEW_NAME,
    ]


class WholesaleResultsInternalDatabase(BaseSettings):
    DATABASE_WHOLESALE_RESULTS_INTERNAL: str = Field(init=False)
    ENERGY_TABLE_NAME: ClassVar[str] = "energy"
    ENERGY_PER_ES_TABLE_NAME: ClassVar[str] = "energy_per_es"
    ENERGY_PER_BRP_TABLE_NAME: ClassVar[str] = "energy_per_brp"
    GRID_LOSS_METERING_POINT_TIME_SERIES_TABLE_NAME: ClassVar[str] = "grid_loss_metering_point_time_series"
    EXCHANGE_PER_NEIGHBOR_TABLE_NAME: ClassVar[str] = "exchange_per_neighbor_ga"
    AMOUNTS_PER_CHARGE_TABLE_NAME: ClassVar[str] = "amounts_per_charge"
    TOTAL_MONTHLY_AMOUNTS_TABLE_NAME: ClassVar[str] = "total_monthly_amounts"
    MONTHLY_AMOUNTS_PER_CHARGE_TABLE_NAME: ClassVar[str] = "monthly_amounts_per_charge"

    TABLE_NAMES: ClassVar[list[str]] = [
        ENERGY_TABLE_NAME,
        ENERGY_PER_ES_TABLE_NAME,
        ENERGY_PER_BRP_TABLE_NAME,
        GRID_LOSS_METERING_POINT_TIME_SERIES_TABLE_NAME,
        EXCHANGE_PER_NEIGHBOR_TABLE_NAME,
        AMOUNTS_PER_CHARGE_TABLE_NAME,
        TOTAL_MONTHLY_AMOUNTS_TABLE_NAME,
        MONTHLY_AMOUNTS_PER_CHARGE_TABLE_NAME,
    ]


class WholesaleResultsDatabase(BaseSettings):
    DATABASE_WHOLESALE_RESULTS: str = Field(init=False)
    ENERGY_V1_VIEW_NAME: ClassVar[str] = "energy_v1"
    ENERGY_PER_BRP_V1_VIEW_NAME: ClassVar[str] = "energy_per_brp_v1"
    ENERGY_PER_ES_V1_VIEW_NAME: ClassVar[str] = "energy_per_es_v1"
    GRID_LOSS_METERING_POINT_TIME_SERIES_VIEW_NAME: ClassVar[str] = "grid_loss_metering_point_time_series_v1"
    EXCHANGE_PER_NEIGHBOR_V1_VIEW_NAME: ClassVar[str] = "exchange_per_neighbor_v1"
    AMOUNTS_PER_CHARGE_V1_VIEW_NAME: ClassVar[str] = "amounts_per_charge_v1"
    MONTHLY_AMOUNTS_PER_CHARGE_V1_VIEW_NAME: ClassVar[str] = "monthly_amounts_per_charge_v1"
    TOTAL_MONTHLY_AMOUNTS_V1_VIEW_NAME: ClassVar[str] = "total_monthly_amounts_v1"
    LATEST_CALCULATION_BY_DAY_V1_VIEW_NAME: ClassVar[str] = "latest_calculations_by_day_v1"

    VIEW_NAMES: ClassVar[list[str]] = [
        ENERGY_V1_VIEW_NAME,
        ENERGY_PER_BRP_V1_VIEW_NAME,
        ENERGY_PER_ES_V1_VIEW_NAME,
        GRID_LOSS_METERING_POINT_TIME_SERIES_VIEW_NAME,
        EXCHANGE_PER_NEIGHBOR_V1_VIEW_NAME,
        AMOUNTS_PER_CHARGE_V1_VIEW_NAME,
        MONTHLY_AMOUNTS_PER_CHARGE_V1_VIEW_NAME,
        TOTAL_MONTHLY_AMOUNTS_V1_VIEW_NAME,
        LATEST_CALCULATION_BY_DAY_V1_VIEW_NAME,
    ]


class WholesaleSapDatabase(BaseSettings):
    DATABASE_WHOLESALE_RESULTS: str = Field(init=False)
    LATEST_CALCULATIONS_HISTORY_V1_VIEW_NAME: ClassVar[str] = "latest_calculations_history_v1"
    ENERGY_V1_VIEW_NAME: ClassVar[str] = "energy_v1"
    AMOUNTS_PER_CHARGE_V1_VIEW_NAME: ClassVar[str] = "amounts_per_charge_v1"

    VIEW_NAMES: ClassVar[list[str]] = [
        LATEST_CALCULATIONS_HISTORY_V1_VIEW_NAME,
        ENERGY_V1_VIEW_NAME,
        AMOUNTS_PER_CHARGE_V1_VIEW_NAME,
    ]


class WholesaleBasisDataDatabase(BaseSettings):
    DATABASE_WHOLESALE_BASIS_DATA: str = Field(init=False)

    METERING_POINT_PERIODS_VIEW_NAME: ClassVar[str] = "metering_point_periods_v1"
    TIME_SERIES_POINTS_VIEW_NAME: ClassVar[str] = "time_series_points_v1"
    CHARGE_LINK_PERIODS_VIEW_NAME: ClassVar[str] = "charge_link_periods_v1"
    CHARGE_PRICE_INFORMATION_PERIODS_VIEW_NAME: ClassVar[str] = "charge_price_information_periods_v1"
    CHARGE_PRICE_POINTS_VIEW_NAME: ClassVar[str] = "charge_price_points_v1"
    GRID_LOSS_METERING_POINTS_VIEW_NAME: ClassVar[str] = "grid_loss_metering_points_v1"

    VIEW_NAMES: ClassVar[list[str]] = [
        METERING_POINT_PERIODS_VIEW_NAME,
        TIME_SERIES_POINTS_VIEW_NAME,
        CHARGE_LINK_PERIODS_VIEW_NAME,
        CHARGE_PRICE_INFORMATION_PERIODS_VIEW_NAME,
        CHARGE_PRICE_POINTS_VIEW_NAME,
        GRID_LOSS_METERING_POINTS_VIEW_NAME,
    ]


class WholesaleBasisDataInternalDatabase(BaseSettings):
    DATABASE_WHOLESALE_BASIS_DATA_INTERNAL: str = Field(init=False)
    METERING_POINT_PERIODS_TABLE_NAME: ClassVar[str] = "metering_point_periods"
    TIME_SERIES_POINTS_TABLE_NAME: ClassVar[str] = "time_series_points"
    CHARGE_LINK_PERIODS_TABLE_NAME: ClassVar[str] = "charge_link_periods"
    CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME: ClassVar[str] = "charge_price_information_periods"
    CHARGE_PRICE_POINTS_TABLE_NAME: ClassVar[str] = "charge_price_points"
    GRID_LOSS_METERING_POINT_IDS_TABLE_NAME: ClassVar[str] = "grid_loss_metering_points"

    TABLE_NAMES: ClassVar[list[str]] = [
        METERING_POINT_PERIODS_TABLE_NAME,
        TIME_SERIES_POINTS_TABLE_NAME,
        CHARGE_LINK_PERIODS_TABLE_NAME,
        CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME,
        CHARGE_PRICE_POINTS_TABLE_NAME,
        GRID_LOSS_METERING_POINT_IDS_TABLE_NAME,
    ]


WHOLESALE_CONTAINER_NAME = "wholesale"
"""The name of the container in the storage account"""


def get_container_root_path(storage_account_name: str) -> str:
    return f"abfss://{WHOLESALE_CONTAINER_NAME}@{storage_account_name}.dfs.core.windows.net/"


def get_calculation_input_path(storage_account_name: str, input_folder: str | None = None) -> str:
    input_folder = input_folder or env_vars.get_calculation_input_folder_name()
    return f"{get_container_root_path(storage_account_name)}{input_folder}/"
