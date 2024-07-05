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


class InputDatabase:
    DATABASE_NAME = "wholesale_input"
    METERING_POINT_PERIODS_TABLE_NAME = "metering_point_periods"
    TIME_SERIES_POINTS_TABLE_NAME = "time_series_points_v2"
    CHARGE_LINK_PERIODS_TABLE_NAME = "charge_link_periods"
    CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME = "charge_price_information_periods"
    CHARGE_PRICE_POINTS_TABLE_NAME = "charge_price_points"
    GRID_LOSS_METERING_POINTS_TABLE_NAME = "grid_loss_metering_points"


class WholesaleInternalDatabase:
    DATABASE_NAME = "wholesale_internal"
    EXECUTED_MIGRATIONS_TABLE_NAME = "executed_migrations"


class WholesaleResultsInternalDatabase:
    DATABASE_NAME = "wholesale_results_internal"  # Defined in dh3infrastructure
    TOTAL_MONTHLY_AMOUNTS_TABLE_NAME = "total_monthly_amounts"
    MONTHLY_AMOUNTS_PER_CHARGE_TABLE_NAME = "monthly_amounts_per_charge"


class HiveOutputDatabase:
    FOLDER_NAME = "calculation-output"
    """The folder in the storage account container"""

    DATABASE_NAME = "wholesale_output"
    ENERGY_RESULT_TABLE_NAME = "energy_results"
    WHOLESALE_RESULT_TABLE_NAME = "wholesale_results"
    MONTHLY_AMOUNTS_TABLE_NAME = "monthly_amounts"
    TOTAL_MONTHLY_AMOUNTS_TABLE_NAME = "total_monthly_amounts"
    SUCCEEDED_ENERGY_RESULTS_V1_VIEW_NAME = "succeeded_energy_results_v1"


class HiveBasisDataDatabase:
    FOLDER_NAME = "basis_data"
    """The folder in the storage account container"""

    DATABASE_NAME = "basis_data"
    METERING_POINT_PERIODS_TABLE_NAME = "metering_point_periods"
    TIME_SERIES_POINTS_TABLE_NAME = "time_series_points"
    CHARGE_LINK_PERIODS_TABLE_NAME = "charge_link_periods"
    CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME = "charge_price_information_periods"
    CHARGE_PRICE_POINTS_TABLE_NAME = "charge_price_points"
    GRID_LOSS_METERING_POINTS_TABLE_NAME = "grid_loss_metering_points"
    CALCULATIONS_TABLE_NAME = "calculations"

    TABLE_NAMES = [
        CALCULATIONS_TABLE_NAME,
        METERING_POINT_PERIODS_TABLE_NAME,
        TIME_SERIES_POINTS_TABLE_NAME,
        CHARGE_LINK_PERIODS_TABLE_NAME,
        CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME,
        CHARGE_PRICE_POINTS_TABLE_NAME,
        GRID_LOSS_METERING_POINTS_TABLE_NAME,
    ]


class CalculationResultsPublicDataModel:
    DATABASE_NAME = "wholesale_calculation_results"
    ENERGY_RESULT_POINTS_PER_GA_V1_VIEW_NAME = "energy_per_ga_v1"
    ENERGY_RESULT_POINTS_PER_BRP_GA_V1_VIEW_NAME = "energy_per_brp_ga_v1"
    ENERGY_RESULT_POINTS_PER_ES_BRP_GA_V1_VIEW_NAME = "energy_per_es_brp_ga_v1"
    GRID_LOSS_METERING_POINT_TIME_SERIES_VIEW_NAME = (
        "grid_loss_metering_point_time_series_v1"
    )
    AMOUNTS_PER_CHARGE_VIEW_NAME = "amounts_per_charge_v1"
    MONTHLY_AMOUNTS_PER_CHARGE_VIEW_NAME = "monthly_amounts_per_charge_v1"
    TOTAL_MONTHLY_AMOUNTS_VIEW_NAME = "total_monthly_amounts_v1"


class SettlementReportPublicDataModel:
    DATABASE_NAME = "settlement_report"
    METERING_POINT_PERIODS_VIEW_NAME_V1 = "metering_point_periods_v1"
    METERING_POINT_TIME_SERIES_VIEW_NAME_V1 = "metering_point_time_series_v1"
    CHARGE_LINK_PERIODS_PER_ES_VIEW_NAME_V1 = "charge_link_periods_per_es_v1"
    CHARGE_PRICES_VIEW_NAME_V1 = "charge_prices_v1"
    ENERGY_RESULT_POINTS_PER_GA_VIEW_NAME_V1 = "energy_result_points_per_ga_v1"
    ENERGY_RESULT_POINTS_PER_ES_GA_SETTLEMENT_REPORT_VIEW_NAME_V1 = (
        "energy_result_points_per_es_ga_v1"
    )
    WHOLESALE_RESULTS_VIEW_NAME_V1 = "wholesale_results_v1"
    CURRENT_BALANCE_FIXING_CALCULATION_VERSION_VIEW_NAME_V1 = (
        "current_balance_fixing_calculation_version_v1"
    )
    MONTHLY_AMOUNTS_VIEW_NAME_V1 = "monthly_amounts_v1"


TEST = ""
"""Used to differentiate behavior of SQL migrations in tests from behavior in production"""

WHOLESALE_CONTAINER_NAME = "wholesale"
"""The name of the container in the storage account"""


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
