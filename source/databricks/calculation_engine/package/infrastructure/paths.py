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

from package.codelists import BasisDataType
import package.infrastructure.environment_variables as env_vars

# Input database and tables
INPUT_DATABASE_NAME = "wholesale"
METERING_POINT_PERIODS_TABLE_NAME = "metering_point_periods"
TIME_SERIES_POINTS_TABLE_NAME = "time_series_points"
CHARGE_LINK_PERIODS_TABLE_NAME = "charge_link_periods"
CHARGE_MASTER_DATA_PERIODS_TABLE_NAME = "charge_masterdata_periods"
CHARGE_PRICE_POINTS_TABLE_NAME = "charge_price_points"
GRID_LOSS_METERING_POINTS_TABLE_NAME = "grid_loss_metering_points"

# Output database and tables
OUTPUT_DATABASE_NAME = "wholesale_output"
ENERGY_RESULT_TABLE_NAME = "energy_results"
WHOLESALE_RESULT_TABLE_NAME = "wholesale_results"

TEST = ""

# Paths
WHOLESALE_CONTAINER_NAME = "wholesale"
INPUT_FOLDER = "calculation_input"
OUTPUT_FOLDER = "calculation-output"
BASIS_DATA_FOLDER = "basis_data"


def get_storage_account_url(storage_account_name: str) -> str:
    return f"https://{storage_account_name}.dfs.core.windows.net"


def get_container_root_path(storage_account_name: str) -> str:
    return f"abfss://{WHOLESALE_CONTAINER_NAME}@{storage_account_name}.dfs.core.windows.net/"


def get_calculation_input_path(storage_account_name: str) -> str:
    input_folder = env_vars.get_calculation_input_folder_name()
    return f"{get_container_root_path(storage_account_name)}{input_folder}/"


def get_basis_data_root_path(basis_data_type: BasisDataType, batch_id: str) -> str:
    batch_path = get_batch_relative_path(batch_id)
    return f"{batch_path}/{BASIS_DATA_FOLDER}/{_get_basis_data_folder_name(basis_data_type)}"


def get_basis_data_path(
    basis_data_type: BasisDataType,
    batch_id: str,
    grid_area: str,
    energy_supplier_id: str | None = None,
) -> str:
    basis_data_root_path = get_basis_data_root_path(basis_data_type, batch_id)
    if energy_supplier_id is None:
        return f"{basis_data_root_path}/grouping=total_ga/grid_area={grid_area}"
    else:
        return f"{basis_data_root_path}/grouping=es_ga/grid_area={grid_area}/energy_supplier_gln={energy_supplier_id}"


def get_batch_relative_path(batch_id: str) -> str:
    return f"{OUTPUT_FOLDER}/batch_id={batch_id}"


def _get_basis_data_folder_name(basis_data_type: BasisDataType) -> str:
    if basis_data_type == BasisDataType.MASTER_BASIS_DATA:
        return "master_basis_data"
    elif basis_data_type == BasisDataType.TIME_SERIES_HOUR:
        return "time_series_hour"
    elif basis_data_type == BasisDataType.TIME_SERIES_QUARTER:
        return "time_series_quarter"
    else:
        raise ValueError(f"Unexpected BasisDataType: {basis_data_type}")
