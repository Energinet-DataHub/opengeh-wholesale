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


class UnityCatalogDatabaseNames:
    """Unity Catalog database names are defined in the dh3infrastructure repository"""

    WHOLESALE_RESULTS = "wholesale_results"
    WHOLESALE_BASIS_DATA_INTERNAL = "wholesale_basis_data_internal"
    WHOLESALE_BASIS_DATA = "wholesale_basis_data"
    WHOLESALE_SETTLEMENT_REPORTS = "wholesale_settlement_reports"
    WHOLESALE_RESULTS_INTERNAL = "wholesale_results_internal"
    WHOLESALE_INTERNAL = "wholesale_internal"
    WHOLESALE_SAP = "wholesale_sap"

    @classmethod
    def get_names(cls) -> list[str]:
        values = []
        for attr in dir(cls):
            value = getattr(cls, attr)
            if not attr.startswith("__") and isinstance(value, str):
                values.append(value)
        return values


# Hive
class InputDatabase:
    DATABASE_NAME = "wholesale_input"
    METERING_POINT_PERIODS_TABLE_NAME = "metering_point_periods"
    TIME_SERIES_POINTS_TABLE_NAME = "time_series_points_v2"
    CHARGE_LINK_PERIODS_TABLE_NAME = "charge_link_periods"
    CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME = "charge_price_information_periods"
    CHARGE_PRICE_POINTS_TABLE_NAME = "charge_price_points"
    GRID_LOSS_METERING_POINT_IDS_TABLE_NAME = "grid_loss_metering_points"


class MigrationsWholesaleDatabase:
    DATABASE_NAME = "shared_wholesale_input"
    METERING_POINT_PERIODS_TABLE_NAME = "metering_point_periods_view_v1"
    TIME_SERIES_POINTS_TABLE_NAME = "time_series_points_view_v1"
    CHARGE_LINK_PERIODS_TABLE_NAME = "charge_link_periods_view_v1"
    CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME = (
        "charge_price_information_periods_view_v1"
    )
    CHARGE_PRICE_POINTS_TABLE_NAME = "charge_price_points_view_v1"


class WholesaleInternalDatabase:
    DATABASE_NAME = UnityCatalogDatabaseNames.WHOLESALE_INTERNAL
    EXECUTED_MIGRATIONS_TABLE_NAME = "executed_migrations"
    CALCULATIONS_TABLE_NAME = "calculations"
    GRID_LOSS_METERING_POINT_IDS_TABLE_NAME = "grid_loss_metering_points"
    CALCULATION_GRID_AREAS_TABLE_NAME = "calculation_grid_areas"

    TABLE_NAMES = [
        EXECUTED_MIGRATIONS_TABLE_NAME,
        CALCULATIONS_TABLE_NAME,
        GRID_LOSS_METERING_POINT_IDS_TABLE_NAME,
        CALCULATION_GRID_AREAS_TABLE_NAME,
    ]

    SUCCEEDED_EXTERNAL_CALCULATIONS_V1_VIEW_NAME = "succeeded_external_calculations_v1"


class WholesaleResultsInternalDatabase:
    DATABASE_NAME = UnityCatalogDatabaseNames.WHOLESALE_RESULTS_INTERNAL
    ENERGY_TABLE_NAME = "energy"
    ENERGY_PER_ES_TABLE_NAME = "energy_per_es"
    ENERGY_PER_BRP_TABLE_NAME = "energy_per_brp"
    GRID_LOSS_METERING_POINT_TIME_SERIES_TABLE_NAME = (
        "grid_loss_metering_point_time_series"
    )
    EXCHANGE_PER_NEIGHBOR_TABLE_NAME = "exchange_per_neighbor_ga"
    AMOUNTS_PER_CHARGE_TABLE_NAME = "amounts_per_charge"
    TOTAL_MONTHLY_AMOUNTS_TABLE_NAME = "total_monthly_amounts"
    MONTHLY_AMOUNTS_PER_CHARGE_TABLE_NAME = "monthly_amounts_per_charge"

    TABLE_NAMES = [
        ENERGY_TABLE_NAME,
        ENERGY_PER_ES_TABLE_NAME,
        ENERGY_PER_BRP_TABLE_NAME,
        GRID_LOSS_METERING_POINT_TIME_SERIES_TABLE_NAME,
        EXCHANGE_PER_NEIGHBOR_TABLE_NAME,
        AMOUNTS_PER_CHARGE_TABLE_NAME,
        TOTAL_MONTHLY_AMOUNTS_TABLE_NAME,
        MONTHLY_AMOUNTS_PER_CHARGE_TABLE_NAME,
    ]


class WholesaleResultsDatabase:
    DATABASE_NAME = UnityCatalogDatabaseNames.WHOLESALE_RESULTS
    ENERGY_V1_VIEW_NAME = "energy_v1"
    ENERGY_PER_BRP_V1_VIEW_NAME = "energy_per_brp_v1"
    ENERGY_PER_ES_V1_VIEW_NAME = "energy_per_es_v1"
    GRID_LOSS_METERING_POINT_TIME_SERIES_VIEW_NAME = (
        "grid_loss_metering_point_time_series_v1"
    )
    EXCHANGE_PER_NEIGHBOR_V1_VIEW_NAME = "exchange_per_neighbor_v1"
    AMOUNTS_PER_CHARGE_V1_VIEW_NAME = "amounts_per_charge_v1"
    MONTHLY_AMOUNTS_PER_CHARGE_V1_VIEW_NAME = "monthly_amounts_per_charge_v1"
    TOTAL_MONTHLY_AMOUNTS_V1_VIEW_NAME = "total_monthly_amounts_v1"


class WholesaleSettlementReportsDatabase:
    DATABASE_NAME = UnityCatalogDatabaseNames.WHOLESALE_SETTLEMENT_REPORTS
    METERING_POINT_PERIODS_VIEW_NAME_V1 = "metering_point_periods_v1"
    METERING_POINT_TIME_SERIES_VIEW_NAME_V1 = "metering_point_time_series_v1"
    CHARGE_LINK_PERIODS_VIEW_NAME_V1 = "charge_link_periods_v1"
    CHARGE_PRICES_VIEW_NAME_V1 = "charge_prices_v1"
    ENERGY_VIEW_NAME = "energy_v1"
    ENERGY_PER_ES_VIEW_NAME = "energy_per_es_v1"
    AMOUNTS_PER_CHARGE_V1_VIEW_NAME = "amounts_per_charge_v1"
    MONTHLY_AMOUNTS_PER_CHARGE_V1_VIEW_NAME = "monthly_amounts_per_charge_v1"
    TOTAL_MONTHLY_AMOUNTS_V1_VIEW_NAME = "total_monthly_amounts_v1"
    MONTHLY_AMOUNTS_V1_VIEW_NAME = "monthly_amounts_v1"


class WholesaleSapDatabase:
    DATABASE_NAME = UnityCatalogDatabaseNames.WHOLESALE_SAP
    LATEST_CALCULATIONS_HISTORY_V1_VIEW_NAME = "latest_calculations_history_v1"
    ENERGY_V1_VIEW_NAME = "energy_v1"
    AMOUNTS_PER_CHARGE_V1_VIEW_NAME = "amounts_per_charge_v1"


class HiveOutputDatabase:
    FOLDER_NAME = "calculation-output"
    """The folder in the storage account container"""

    DATABASE_NAME = "wholesale_output"
    #ENERGY_RESULT_TABLE_NAME = "energy_results"
    #WHOLESALE_RESULT_TABLE_NAME = "wholesale_results"
    #MONTHLY_AMOUNTS_TABLE_NAME = "monthly_amounts"
    #TOTAL_MONTHLY_AMOUNTS_TABLE_NAME = "total_monthly_amounts"
    #SUCCEEDED_ENERGY_RESULTS_V1_VIEW_NAME = "succeeded_energy_results_v1"


class WholesaleBasisDataDatabase:
    DATABASE_NAME = UnityCatalogDatabaseNames.WHOLESALE_BASIS_DATA
    METERING_POINT_PERIODS_VIEW_NAME = "metering_point_periods_v1"
    TIME_SERIES_POINTS_VIEW_NAME = "time_series_points_v1"
    CHARGE_LINK_PERIODS_VIEW_NAME = "charge_link_periods_v1"
    CHARGE_PRICE_INFORMATION_PERIODS_VIEW_NAME = "charge_price_information_periods_v1"
    CHARGE_PRICE_POINTS_VIEW_NAME = "charge_price_points_v1"
    GRID_LOSS_METERING_POINTS_VIEW_NAME = "grid_loss_metering_points_v1"


class WholesaleBasisDataInternalDatabase:
    DATABASE_NAME = UnityCatalogDatabaseNames.WHOLESALE_BASIS_DATA_INTERNAL
    METERING_POINT_PERIODS_TABLE_NAME = "metering_point_periods"
    TIME_SERIES_POINTS_TABLE_NAME = "time_series_points"
    CHARGE_LINK_PERIODS_TABLE_NAME = "charge_link_periods"
    CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME = "charge_price_information_periods"
    CHARGE_PRICE_POINTS_TABLE_NAME = "charge_price_points"
    GRID_LOSS_METERING_POINT_IDS_TABLE_NAME = "grid_loss_metering_points"
    TABLE_NAMES = [
        METERING_POINT_PERIODS_TABLE_NAME,
        TIME_SERIES_POINTS_TABLE_NAME,
        CHARGE_LINK_PERIODS_TABLE_NAME,
        CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME,
        CHARGE_PRICE_POINTS_TABLE_NAME,
        GRID_LOSS_METERING_POINT_IDS_TABLE_NAME,
    ]


class HiveBasisDataDatabase:
    FOLDER_NAME = "basis_data"
    """The folder in the storage account container"""

    DATABASE_NAME = "basis_data"
    # To be removed when tables are deleted
    # METERING_POINT_PERIODS_TABLE_NAME = "metering_point_periods"
    # TIME_SERIES_POINTS_TABLE_NAME = "time_series_points"
    # CHARGE_LINK_PERIODS_TABLE_NAME = "charge_link_periods"
    # CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME = "charge_price_information_periods"
    # CHARGE_PRICE_POINTS_TABLE_NAME = "charge_price_points"
    # GRID_LOSS_METERING_POINTS_TABLE_NAME = "grid_loss_metering_points"
    # CALCULATIONS_TABLE_NAME = "calculations"
    #
    # TABLE_NAMES = [
    #     CALCULATIONS_TABLE_NAME,
    #     METERING_POINT_PERIODS_TABLE_NAME,
    #     TIME_SERIES_POINTS_TABLE_NAME,
    #     CHARGE_LINK_PERIODS_TABLE_NAME,
    #     CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME,
    #     CHARGE_PRICE_POINTS_TABLE_NAME,
    #     GRID_LOSS_METERING_POINTS_TABLE_NAME,
    # ]


# Hive
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


class HiveSettlementReportPublicDataModel:
    DATABASE_NAME = "settlement_report"
    METERING_POINT_PERIODS_VIEW_NAME_V1 = "metering_point_periods_v1"
    METERING_POINT_TIME_SERIES_VIEW_NAME_V1 = "metering_point_time_series_v1"
    CHARGE_LINK_PERIODS_VIEW_NAME_V1 = "charge_link_periods_v1"
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


def get_calculation_input_path(
    storage_account_name: str, input_folder: str | None = None
) -> str:
    input_folder = input_folder or env_vars.get_calculation_input_folder_name()
    return f"{get_container_root_path(storage_account_name)}{input_folder}/"
