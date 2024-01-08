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

from azure.identity import ClientSecretCredential
import os
from enum import Enum
from typing import Any


class EnvironmentVariable(Enum):
    TIME_ZONE = "TIME_ZONE"
    DATA_STORAGE_ACCOUNT_NAME = "DATA_STORAGE_ACCOUNT_NAME"
    CALCULATION_INPUT_FOLDER_NAME = "CALCULATION_INPUT_FOLDER_NAME"
    TENANT_ID = "TENANT_ID"
    SPN_APP_ID = "SPN_APP_ID"
    SPN_APP_SECRET = "SPN_APP_SECRET"


def get_storage_account_credential() -> ClientSecretCredential:
    required_env_variables = [
        EnvironmentVariable.TENANT_ID,
        EnvironmentVariable.SPN_APP_ID,
        EnvironmentVariable.SPN_APP_SECRET,
        EnvironmentVariable.DATA_STORAGE_ACCOUNT_NAME,
    ]
    env_vars = get_env_variables_or_throw(required_env_variables)

    credential = ClientSecretCredential(
        env_vars[EnvironmentVariable.TENANT_ID],
        env_vars[EnvironmentVariable.SPN_APP_ID],
        env_vars[EnvironmentVariable.SPN_APP_SECRET],
    )

    return credential


def get_storage_account_name() -> str:
    return get_env_variable_or_throw(EnvironmentVariable.DATA_STORAGE_ACCOUNT_NAME)


def get_time_zone() -> str:
    return get_env_variable_or_throw(EnvironmentVariable.TIME_ZONE)


def get_calculation_input_folder_name() -> str:
    return get_env_variable_or_throw(EnvironmentVariable.CALCULATION_INPUT_FOLDER_NAME)


def get_env_variables_or_throw(environment_variable: list[EnvironmentVariable]) -> dict:
    env_variables = dict()
    for env_var in environment_variable:
        env_variables[env_var] = get_env_variable_or_throw(env_var)

    return env_variables


def get_env_variable_or_throw(variable: EnvironmentVariable) -> Any:
    env_variable = os.getenv(variable.value)
    if env_variable is None:
        raise ValueError(f"Environment variable not found: {variable.value}")

    return env_variable
