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

from package.constants.time_series_type import TimeSeriesType
from package.constants.market_role import MarketRole
from enum import Enum

WHOLESALE_CONTAINER_NAME = "wholesale"

OUTPUT_FOLDER = "calculation-output"
ACTORS_FOLDER = "actors"
RESULT_FOLDER = "result"
BASIS_DATA_FOLDER = "basis_data"


def get_storage_account_url(storage_account_name: str) -> str:
    return f"https://{storage_account_name}.dfs.core.windows.net"


def get_container_root_path(storage_account_name: str) -> str:
    return f"abfss://{WHOLESALE_CONTAINER_NAME}@{storage_account_name}.dfs.core.windows.net/"


def get_result_file_path(
    container_root_path: str,
    batch_id: str,
    grid_area: str,
    gln: str,
    time_series_type: TimeSeriesType,
) -> str:
    batch_path = _get_batch_path(container_root_path, batch_id)
    return f"{batch_path}{RESULT_FOLDER}/grid_area={grid_area}/gln={gln}/time_series_type={time_series_type.value}"


def get_actors_file_path(
    container_root_path: str,
    batch_id: str,
    grid_area: str,
    time_series_type: TimeSeriesType,
    market_role: MarketRole,
) -> str:
    batch_path = _get_batch_path(container_root_path, batch_id)
    return f"{batch_path}/{ACTORS_FOLDER}/grid_area={grid_area}/time_series_type={time_series_type.value}/market_role={market_role.value}"


def get_time_series_quarter_path(
    container_root_path: str, batch_id: str, grid_area: str, gln: str
) -> str:
    batch_path = _get_batch_path(container_root_path, batch_id)
    return f"{batch_path}/{BASIS_DATA_FOLDER}/time_series_quarter/grid_area={grid_area}/gln={gln}"


def get_time_series_hour_path(
    container_root_path: str, batch_id: str, grid_area: str, gln: str
) -> str:
    batch_path = _get_batch_path(container_root_path, batch_id)
    return f"{batch_path}/{BASIS_DATA_FOLDER}/time_series_hour/grid_area={grid_area}/gln={gln}"


def get_master_basis_data_path(
    container_root_path: str, batch_id: str, grid_area: str, gln: str
) -> str:
    batch_path = _get_batch_path(container_root_path, batch_id)
    return f"{batch_path}/{BASIS_DATA_FOLDER}/master_basis_data/grid_area={grid_area}/gln={gln}"


def _get_batch_path(container_root_path: str, batch_id: str) -> str:
    return f"{container_root_path}/{OUTPUT_FOLDER}/batch_id={batch_id}"
