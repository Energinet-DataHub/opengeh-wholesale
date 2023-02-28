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

from package.codelists import TimeSeriesType
from package.constants import PartitionKeyName
from typing import Union

WHOLESALE_CONTAINER_NAME = "wholesale"

OUTPUT_FOLDER = "calculation-output"
ACTORS_FOLDER = "actors"
RESULT_FOLDER = "result"
BASIS_DATA_FOLDER = "basis_data"


def get_storage_account_url(storage_account_name: str) -> str:
    return f"https://{storage_account_name}.dfs.core.windows.net"


def get_container_root_path(storage_account_name: str) -> str:
    return f"abfss://{WHOLESALE_CONTAINER_NAME}@{storage_account_name}.dfs.core.windows.net/"


def get_result_file_relative_path(
    batch_id: str,
    grid_area: str,
    energy_supplier_gln: Union[str, None],
    balance_responsible_gln: Union[str, None],
    time_series_type: TimeSeriesType,
    grouping: str,
) -> str:
    batch_path = get_batch_relative_path(batch_id)
    relative_path = f"{batch_path}/{RESULT_FOLDER}/grouping={grouping}/time_series_type={time_series_type.value}/grid_area={grid_area}"

    if (energy_supplier_gln is None) and (balance_responsible_gln is None):
        return relative_path

    if balance_responsible_gln is None:
        return f"{relative_path}/gln={energy_supplier_gln}"

    if energy_supplier_gln is None:
        return f"{relative_path}/gln={balance_responsible_gln}"

    return f"{relative_path}/{PartitionKeyName.BALANCE_RESPONSIBLE_PARTY_GLN}={balance_responsible_gln}/{PartitionKeyName.ENERGY_SUPPLIER_GLN}={energy_supplier_gln}"


def get_actors_file_relative_path(
    batch_id: str, grid_area: str, time_series_type: TimeSeriesType
) -> str:
    batch_path = get_batch_relative_path(batch_id)
    return f"{batch_path}/{ACTORS_FOLDER}/time_series_type={time_series_type.value}/grid_area={grid_area}"


def get_time_series_quarter_relative_path(batch_id: str, grid_area: str) -> str:
    batch_path = get_batch_relative_path(batch_id)
    return f"{batch_path}/{BASIS_DATA_FOLDER}/time_series_quarter/grouping=total_ga/grid_area={grid_area}"


def get_time_series_hour_relative_path(batch_id: str, grid_area: str) -> str:
    batch_path = get_batch_relative_path(batch_id)
    return f"{batch_path}/{BASIS_DATA_FOLDER}/time_series_hour/grouping=total_ga/grid_area={grid_area}"


def get_master_basis_data_relative_path(batch_id: str, grid_area: str) -> str:
    batch_path = get_batch_relative_path(batch_id)
    return f"{batch_path}/{BASIS_DATA_FOLDER}/master_basis_data/grouping=total_ga/grid_area={grid_area}"


def get_batch_relative_path(batch_id: str) -> str:
    return f"{OUTPUT_FOLDER}/batch_id={batch_id}"
