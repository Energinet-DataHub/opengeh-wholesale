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

import package.infrastructure.environment_variables as env_vars

# Input database and tables
INPUT_DATABASE_NAME = "wholesale_input"
METERING_POINT_PERIODS_TABLE_NAME = "metering_point_periods"
TIME_SERIES_POINTS_TABLE_NAME = "time_series_points_v2"
CHARGE_LINK_PERIODS_TABLE_NAME = "charge_link_periods"
CHARGE_MASTER_DATA_PERIODS_TABLE_NAME = "charge_price_information_periods"
CHARGE_PRICE_POINTS_TABLE_NAME = "charge_price_points"
GRID_LOSS_METERING_POINTS_TABLE_NAME = "grid_loss_metering_points"

# Output database and tables
OUTPUT_DATABASE_NAME = "wholesale_output"
ENERGY_RESULT_TABLE_NAME = "energy_results"
WHOLESALE_RESULT_TABLE_NAME = "wholesale_results"
TOTAL_MONTHLY_AMOUNTS_TABLE_NAME = "total_monthly_amounts"
SUCCEEDED_ENERGY_RESULTS_V1_VIEW_NAME = "succeeded_energy_results_v1"

# Basis data database and tables
BASIS_DATA_DATABASE_NAME = "basis_data"
METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME = "metering_point_periods"
TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME = "time_series_points"
CHARGE_LINK_PERIODS_BASIS_DATA_TABLE_NAME = "charge_link_periods"
CHARGE_MASTER_DATA_PERIODS_BASIS_DATA_TABLE_NAME = "charge_price_information_periods"
CHARGE_PRICE_POINTS_BASIS_DATA_TABLE_NAME = "charge_price_points"
GRID_LOSS_METERING_POINTS_BASIS_DATA_TABLE_NAME = "grid_loss_metering_points"
CALCULATIONS_TABLE_NAME = "calculations"


class EdiResults:
    DATABASE_NAME = "wholesale_edi_results"
    ENERGY_RESULT_POINTS_PER_GA_V1_VIEW_NAME = "energy_result_points_per_ga_v1"
    ENERGY_RESULT_POINTS_PER_BRP_GA_V1_VIEW_NAME = "energy_result_points_per_brp_ga_v1"
    ENERGY_RESULT_POINTS_PER_ES_BRP_GA_V1_VIEW_NAME = (
        "energy_result_points_per_es_brp_ga_v1"
    )


# Settlement report database and views
SETTLEMENT_REPORT_DATABASE_NAME = "settlement_report"
METERING_POINT_PERIODS_SETTLEMENT_REPORT_VIEW_NAME_V1 = "metering_point_periods_v1"
METERING_POINT_TIME_SERIES_SETTLEMENT_REPORT_VIEW_NAME_V1 = (
    "metering_point_time_series_v1"
)
CHARGE_LINK_PERIODS_SETTLEMENT_REPORT_VIEW_NAME_V1 = "charge_link_periods_v1"
CHARGE_PRICES_SETTLEMENT_REPORT_VIEW_NAME_V1 = "charge_prices_v1"
ENERGY_RESULT_POINTS_PER_GA_SETTLEMENT_REPORT_VIEW_NAME_V1 = (
    "energy_result_points_per_ga_v1"
)
ENERGY_RESULT_POINTS_PER_GA_ES_SETTLEMENT_REPORT_VIEW_NAME_V1 = (
    "energy_result_points_per_ga_es_v1"
)
WHOLESALE_RESULTS_SETTLEMENT_REPORT_VIEW_NAME_V1 = "wholesale_results_v1"
CURRENT_CALCULATION_TYPE_VERSIONS_SETTLEMENT_REPORT_VIEW_NAME_V1 = (
    "current_calculation_type_versions_v1"
)
MONTHLY_AMOUNTS_SETTLEMENT_REPORT_VIEW_NAME_V1 = "monthly_amounts_v1"

TEST = ""

# Paths
WHOLESALE_CONTAINER_NAME = "wholesale"
OUTPUT_FOLDER = "calculation-output"
BASIS_DATA_FOLDER = "basis_data"

BASIS_DATA_TABLE_NAMES = [
    CALCULATIONS_TABLE_NAME,
    METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME,
    TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME,
    CHARGE_LINK_PERIODS_BASIS_DATA_TABLE_NAME,
    CHARGE_MASTER_DATA_PERIODS_BASIS_DATA_TABLE_NAME,
    CHARGE_PRICE_POINTS_BASIS_DATA_TABLE_NAME,
    GRID_LOSS_METERING_POINTS_BASIS_DATA_TABLE_NAME,
]


def get_storage_account_url(storage_account_name: str) -> str:
    return f"https://{storage_account_name}.dfs.core.windows.net"


def get_container_root_path(storage_account_name: str) -> str:
    return f"abfss://{WHOLESALE_CONTAINER_NAME}@{storage_account_name}.dfs.core.windows.net/"


def get_spark_sql_migrations_path(storage_account_name: str) -> str:
    return f"{get_container_root_path(storage_account_name)}spark_sql_migrations/"


def get_calculation_input_path(
    storage_account_name: str, input_folder: str | None = None
) -> str:
    input_folder = input_folder or env_vars.get_calculation_input_folder_name()
    return f"{get_container_root_path(storage_account_name)}{input_folder}/"
