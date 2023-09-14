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


import yaml
from enum import Enum
import re
from tests.helpers import file_utils


class CalculationFileType(Enum):
    TIME_SERIES_QUARTER_BASIS_DATA_FOR_TOTAL_GA = (
        "time_series_quarter_basis_data_file_for_total_grid_area"
    )
    TIME_SERIES_HOUR_BASIS_DATA = "time_series_hour_basis_data_file_for_total_grid_area"
    MASTER_BASIS_DATA_FOR_TOTAL_GA = "master_basis_data_file_for_total_grid_area"
    TIME_SERIES_QUARTER_BASIS_DATA_FOR_ES_PER_GA = (
        "time_series_quarter_basis_data_file_for_es_per_ga"
    )
    TIME_SERIES_HOUR_BASIS_DATA_FOR_ES_PER_GA = (
        "time_series_hour_basis_data_file_for_es_per_ga"
    )
    MASTER_BASIS_DATA_FOR_ES_PER_GA = "master_basis_data_file_for_es_per_ga"


def _calculation_file_paths_contract(
    contracts_path: str, calculation_file_type: CalculationFileType
) -> tuple[str, str]:
    with open(f"{contracts_path}/calculation-file-paths.yml", "r") as stream:
        file_paths = yaml.safe_load(stream)
        extension = file_paths[calculation_file_type.value]["extension"]
        directory_expression = file_paths[calculation_file_type.value][
            "directory_expression"
        ]

        return (directory_expression, extension)


def assert_file_path_match_contract(
    contracts_path: str,
    actual_file_path: str,
    calculation_file_type: CalculationFileType,
) -> None:
    (directory_expression, extension) = _calculation_file_paths_contract(
        contracts_path, calculation_file_type
    )

    expected_path_expression = file_utils.create_file_path_expression(
        directory_expression,
        extension,
    )

    assert re.match(
        expected_path_expression, actual_file_path
    ), f"Actual path '{actual_file_path}' does not match expected path '{expected_path_expression}'"
