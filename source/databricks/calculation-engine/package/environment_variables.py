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

# Variables defined in the infrastructure repository (https://github.com/Energinet-DataHub/dh3-infrastructure)

import os

from enum import Enum
from typing import Dict, Any


class EnvironmentVariableName(Enum):
    TIME_ZONE = "TIME_ZONE"
    DATA_STORAGE_ACCOUNT_NAME = "DATA_STORAGE_ACCOUNT_NAME"
    TENANT_ID = "TENANT_ID"
    SPN_APP_ID = "SPN_APP_ID"
    SPN_APP_SECRET = "SPN_APP_SECRET"


class EnvironmentVariables:
    def __init__(self) -> None:
        self.variables = Dict()
        for var_name in EnvironmentVariableName:
            self.variables[var_name] = _get_or_throw(var_name.value)

    def get(self, name: EnvironmentVariableName) -> Any:
        return self.variables[name]


def _get_or_throw(variable_name: str) -> Any:
    variable_value = os.getenv(variable_name)
    if variable_value is None:
        raise ValueError(f"Environment variable not found: {variable_name}")
